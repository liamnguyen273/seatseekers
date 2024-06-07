using Firebase;
using Firebase.Analytics;
using System;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common;

namespace com.brg.Unity.FirebaseAnalytics
{
    public class FirebaseServiceAdapter : IAnalyticsServiceAdapter
    {
        public IProgress Initialize()
        {
            var progress = new SingleTaskBoolProgress(FirebaseDependencyTaskAsync(), 1f);
            _initializationProgress = progress;
            return progress;
        }

        public bool Initialized => _initializationProgress.Successful is true;

        private IProgress _initializationProgress;
        public IProgress InitializationProgress => _initializationProgress ??= new ImmediateProgress(false, 0f);

        private async Task<bool> FirebaseDependencyTaskAsync()
        {
            var status = await FirebaseHelper.CheckDependencies();
            var available = status == DependencyStatus.Available;
            
            if (available)
            {
                LogObj.Default.Success("FirebaseServiceAdapter", $"Firebase initialized.");
            }
            else
            {
                LogObj.Default.Warn("FirebaseServiceAdapter", $"Firebase dependencies were not resolved (Result: {status}). Initialization halted.");
            }

            return available;
        }
    
        public void SendEvent(AnalyticsEventBuilder eventBuilder)
        {
            if (TranslateGameEventName(eventBuilder.Name, out var name))
            {
                var parameters = eventBuilder.IterateParameters()
                    .Select(x => GetParam(x.Item1, x.Item2, x.Item3))
                    .Where(x => x != null)
                    .ToArray();
                Firebase.Analytics.FirebaseAnalytics.LogEvent(name, parameters);
                LogObj.Default.Info("FirebaseServiceAdapter", $"Logged event: {eventBuilder}");
            }
            else
            {
                LogObj.Default.Info("FirebaseServiceAdapter", $"Event not sent. Does not have event {eventBuilder.Name} translation.");
            }
        }

        public virtual bool TranslateGameEventName(string name, out string translatedName)
        {
            translatedName = name;
            return true;
        }

        private static Parameter GetParam(string key, Type type, object value)
        {
            if (type == typeof(string)) return GetParam(key, (string)value);
            if (type == typeof(long)) return GetParam(key, (long)value);
            if (type == typeof(int)) return GetParam(key, (int)value);
            if (type == typeof(double)) return GetParam(key, (double)value);
            if (type == typeof(float)) return GetParam(key, (float)value);
            return null;
        }
    
        private static Parameter GetParam(string key, string value)
        {
            return new Parameter(key, value);
        }

        private static Parameter GetParam(string key, double value)
        {
            return new Parameter(key, value);
        }

        private static Parameter GetParam(string key, long value)
        {
            return new Parameter(key, value);
        }
    }
}