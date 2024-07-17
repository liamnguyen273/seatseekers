using System;
using System.Linq;
using com.brg.Common;
using Singular;
using UnityEngine;

namespace com.brg.Unity.Singular
{
    public class SingularServiceAdapter : IAnalyticsServiceAdapter
    {
        private IProgress _initProgress;
        
        // Test, really
        public bool Initialized => true;
        
        public IProgress Initialize()
        {
            if (_initProgress != null) return _initProgress;
            
#if !UNITY_EDITOR
            var sdk = SingularSDK.Instance;
            SingularSDK.InitializeSingularSDK();
#endif

            LogObj.Default.Info("SingularServiceAdapter", "initialized.");
            _initProgress = new ImmediateProgress(true);
            return _initProgress;
        }
        
        public void SendEvent(AnalyticsEventBuilder eventBuilder)
        {
            if (!Initialized)
            {
                LogObj.Default.Info("SingularServiceAdapter", $"Event not sent. Singular is not initialized.");
                return;
            }
            
            if (!TranslateGameEventName(eventBuilder.Name, out var name))
            {
                LogObj.Default.Info("SingularServiceAdapter", $"Event not sent. Does not have event {eventBuilder.Name} translation.");
                return;
            }

            if (eventBuilder.Name == "ad_impression")
            {
                var parameters = eventBuilder.IterateParameters().ToDictionary(x => x.Item1, x => x.Item3);
                SingularAdData adData;

                try
                {
                    adData = new SingularAdData((string)parameters["platform"], (string)parameters["currency"],
                        (double)parameters["revenue"]);
                    SingularSDK.AdRevenue(adData);
                    LogObj.Default.Info("SingularServiceAdapter", $"Logged revenue: {adData}.");
                }
                catch (Exception e)
                {
                    adData = new SingularAdData("", "", 0.0);
                    LogObj.Default.Error(e);
                }
            }
            else
            {
                var parameters = eventBuilder.IterateParameters().ToDictionary(x => x.Item1, x => x.Item3);
                SingularSDK.Event(parameters, name);
                LogObj.Default.Info("SingularServiceAdapter", $"Logged event: {eventBuilder}");
            }
        }

        public bool TranslateGameEventName(string name, out string translatedName)
        {
            var lowercaseName = name.ToLower();
            translatedName = lowercaseName;
            return true;
        }
    }
}