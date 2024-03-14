using com.brg.Common;
using DG.Tweening;
using Firebase.Analytics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.brg.UnityCommon.Ads
{
    public partial class AdManager
    {
        private int _interRetryAttempt;

        public void InitializeInterstitialAds()
        {
#if MAX_SDK
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            LoadInterstitial();
#else
#endif
        }


        private void LoadInterstitial()
        {
#if MAX_SDK
            Log.Info($"MAX's interstitial load request (attempt {_interRetryAttempt})");
            MaxSdk.LoadInterstitial(INTERSTITIAL_AD_UNIT);
#else
#endif
        }
        
        private void ShowInterstitial()
        {
#if MAX_SDK
            Log.Warn($"Requested showing MAX's interstitial ad.");
            MaxSdk.ShowInterstitial(INTERSTITIAL_AD_UNIT);
#else
            OnAdCompleted(false);
#endif
        }


#if MAX_SDK
        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _interRetryAttempt = 0;
            Log.Info($"MAX's interstitial ad loaded at retry attempt {_interRetryAttempt}");
        }
#else
#endif

#if MAX_SDK
        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _interRetryAttempt++;
            Log.Warn($"MAX's interstitial ad failed to load (Attempt {_interRetryAttempt}).");
            Log.Error(errorInfo);
            double retryDelay = Math.Pow(1.5f, Math.Min(6, _interRetryAttempt));
            Invoke(nameof(LoadInterstitial), (float)retryDelay);
        }
#else
#endif

#if MAX_SDK
        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
        { 
            GM.Instance.Events.MakeEvent(GameEvents.INTERSTITIAL_AD_SHOW)
                .SendEvent();
        }
#else
#endif

#if MAX_SDK
        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            Log.Error("MAX's interstitial ad failed to display.");
            Log.Error(errorInfo);
            OnAdFailedToShow();
            LoadInterstitial();
        }
#else
#endif

#if MAX_SDK
        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
        {
            Log.Info("User clicked on MAX's interstitial ad.");
        }
#else
#endif

#if MAX_SDK
        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log.Info("MAX's interstitial ad is hidden.");
            
            OnAdCompleted(true);
            LoadInterstitial();
        }
#else
#endif
    }
}
