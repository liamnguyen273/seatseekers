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
        private int _rewardRetryAttempt;
        private bool _receivedReward = false;
        
        public void InitializeRewardedAds()
        {
#if MAX_SDK
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            
            LoadRewardAd();
#else
#endif
        }

        private void LoadRewardAd()
        {
#if MAX_SDK
            Log.Info($"MAX's rewarded load request (attempt {_rewardRetryAttempt})");
            MaxSdk.LoadRewardedAd(REWARD_AD_UNIT);
#else
#endif
        }
        
        private void ShowRewardAd()
        {
#if MAX_SDK
            Log.Warn($"Requested showing MAX's rewarded ad.");
            MaxSdk.ShowRewardedAd(REWARD_AD_UNIT);
#else
            OnAdCompleted(false);
#endif
        }

#if MAX_SDK
        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _rewardRetryAttempt = 0;
            Log.Info($"MAX's rewarded ad loaded at retry attempt {_rewardRetryAttempt}");
        }
#else
#endif
#if MAX_SDK        
        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _rewardRetryAttempt++;
            Log.Warn($"MAX's rewarded ad failed to load (Attempt {_interRetryAttempt}).");
            Log.Error(errorInfo);
            double retryDelay = Math.Pow(1.5f, Math.Min(6, _rewardRetryAttempt));

            Invoke(nameof(LoadRewardAd), (float)retryDelay);
        }
#else
#endif

#if MAX_SDK
        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _receivedReward = false;

            GM.Instance.Events.MakeEvent(GameEvents.REWARD_AD_SHOW)
                .Add(_request.ArbitraryData)
                .SendEvent();
        }
#else
#endif

#if MAX_SDK
        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            Log.Error("MAX's rewarded ad failed to display.");
            Log.Error(errorInfo);
            OnAdFailedToShow();
            LoadRewardAd();
        }
#else
#endif

#if MAX_SDK
        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log.Info("User clicked on MAX's rewarded ad.");
        }
#else
#endif
        
#if MAX_SDK
        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log.Info("MAX's rewarded ad is hidden.");
            
            // Let's allow receive rewards
            OnAdCompleted(true);
            
            // TODO: Move this when appropriate
            GM.Instance.Events.MakeEvent(GameEvents.REWARD_AD_REWARDED)
                .SendEvent();
            
            LoadRewardAd();
        }
#else
#endif

#if MAX_SDK
        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            Log.Info("User received rewards from MAX's rewarded ad");
            _receivedReward = true;
        }
#else
        private void OnRewardedAdReceivedRewardEvent(string adUnitId, object reward, object adInfo)
        {
            _receivedReward = true;
        }
#endif
    }
}
