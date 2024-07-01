using System;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Unity.Consents;
using GoogleMobileAds.Ump.Api;

namespace com.brg.Unity.LevelPlay
{
    public static class LevelPlayInitialization
    {
#if UNITY_ANDROID
        private const string APP_KEY = "1ea9de905";
#elif UNITY_IOS
        private const string APP_KEY = "1ea9e228d";
#else
        private const string APP_KEY = "unexpected_platform";
        #endif

        private static IProgress _initProgress;
        private static TaskCompletionSource<bool> _tcs;

        public static event Action ShouldInitializeEvent;

        public static Task<bool> InitTask => _tcs.Task;
        public static bool Initialized => _tcs.Task.IsCompletedSuccessfully && _tcs.Task.Result;

        public static IProgress Initialize()
        {
            if (_initProgress != null) return _initProgress;
            _tcs = new TaskCompletionSource<bool>();
            _initProgress = new SingleTCSBoolProgress(_tcs, 1f);
            
            var progress = GoogleCMP.Initialize();
            progress.Task.ContinueWith((t) =>
            {
                var status = GoogleCMP.GetConsentStatus();
               
                IronSource.Agent.setConsent(status is ConsentStatus.NotRequired or ConsentStatus.Obtained);
                
                IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;
                IronSource.Agent.setManualLoadRewardedVideo(true);
                IronSource.Agent.init(APP_KEY, IronSourceAdUnits.BANNER, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.REWARDED_VIDEO);
            }, TaskScheduler.FromCurrentSynchronizationContext());

            return _initProgress;
        }

        private static void SdkInitializationCompletedEvent()
        {
            _tcs.SetResult(true);
        }
    }
}