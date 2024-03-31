#if HAS_APPSFLYER

using AppsFlyerSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;

namespace com.brg.ExtraComponents
{
    public class AppFlyerServiceAdapter : IAnalyticsServiceAdapter
    {
        private static readonly string DevKey = "INSERT_KEY_HERE";
        
        public IProgress Initialize()
        {
            try
            {
                AppsFlyer.initSDK(DevKey, null);
                AppsFlyer.startSDK();
                AppsFlyerAdRevenue.start();
                
                // AppsFlyer.setIsDebug(true);
                // AppsFlyerAdRevenue.setIsDebug(true);
                
                LogObj.Default.Success("AppFlyerServiceAdapter", $"Initialize successfully.");
                _initializationProgress = new ImmediateProgress(true, 1f);
                return _initializationProgress;
            }
            catch (Exception e)
            {
                LogObj.Default.Info("AppFlyerServiceAdapter", $"Failed to initialize. Reason: {e}");
                _initializationProgress = new ImmediateProgress(false, 1f);
                return InitializationProgress;
            }
        }

        private IProgress _initializationProgress;
        public IProgress InitializationProgress => _initializationProgress ??= new ImmediateProgress(false, 0f);
        
        public void SendEvent(AnalyticsEventBuilder eventBuilder)
        {
            if (!TranslateGameEventName(eventBuilder.Name, out var name))
            {
                LogObj.Default.Info("AppFlyerServiceAdapter",
                    $"Event not sent. Does not have event {eventBuilder.Name} translation.");
                return;
            }

            var parametersPreParse = eventBuilder.IterateParameters().ToDictionary(p => p.Item1, p => p.Item3);
            var parameters = parametersPreParse.ToDictionary(x => x.Key, x => x.Value.ToString());

            if (IsAdImpressionOrRevenueEvent(eventBuilder.Name))
            {
                HandleAdImpressionOrRevenueEvent(eventBuilder, parametersPreParse);
            }
            else
            {
                AppsFlyer.sendEvent(name, parameters);
                LogObj.Default.Info("AppFlyerServiceAdapter", $"Logged event: {eventBuilder}");
            }
        }
        
        private void HandleAdImpressionOrRevenueEvent(AnalyticsEventBuilder eventBuilder, Dictionary<string, object> parameters)
        {
            try
            {
                var source = (string)parameters["ad_source"];
                var revenue = (double)parameters["value"];
                
                LogObj.Default.Info("AppFlyerServiceAdapter", $"Will send to revenue connector (Network: {source}, Revenue: {revenue}).");
                AppsFlyerAdRevenue.logAdRevenue(
                    source,
                    AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
                    revenue,
                    "USD",
                    parameters);
            }
            catch (Exception e)
            {
                LogObj.Default.Warn("AppFlyerServiceAdapter", $"Revenue connector sent failed. Reason: {e}");
            }
        }

        public virtual bool TranslateGameEventName(string name, out string translatedName)
        {
            translatedName = name;
            return true;
        }

        protected virtual bool IsAdImpressionOrRevenueEvent(string name)
        {
            return name == BasicGameEvents.AD_IMPRESSION_OR_REVENUE;
        }
    }
}
#endif