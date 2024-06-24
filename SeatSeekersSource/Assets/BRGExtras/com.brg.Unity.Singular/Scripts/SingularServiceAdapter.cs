using System.Linq;
using com.brg.Common;
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
            
            var parameters = eventBuilder.IterateParameters().ToDictionary(x => x.Item1, x => x.Item3);
            SingularSDK.Event(parameters, name);
            LogObj.Default.Info("SingularServiceAdapter", $"Logged event: {eventBuilder}");
        }

        public bool TranslateGameEventName(string name, out string translatedName)
        {
            var lowercaseName = name.ToLower();
            translatedName = "sng_" + lowercaseName;
            return true;
        }
    }
}