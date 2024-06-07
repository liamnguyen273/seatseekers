using System;
using System.Threading;
using System.Threading.Tasks;
using com.brg.Common;
using GoogleMobileAds.Api;
using AdRequest = GoogleMobileAds.Api.AdRequest;
using Utils = com.brg.Common.Utils;

namespace com.brg.Unity.AdMob
{
    public class AdMobRewardProvider : IAdServiceProvider
    {
        private readonly string _adUnitKey;
        private RewardedAd _loadedAd;
        private bool _loading;
        private bool _showingAd;
        private bool _adResult;
        private bool _hasReward;
        private bool _rewardResult;

        private LogObj Log { get; set; }

        public bool Initialized => AdMobHelper.Initialized;
        
        public AdMobRewardProvider(string adUnitKey)
        {
            _adUnitKey = adUnitKey;
            Log = new LogObj(GetType().ToString());
        }

        public IProgress Initialize()
        {
            return AdMobHelper.Initialize();
        }
        
        public bool CanHandleRequest(AdRequestType type)
        {
            return !_showingAd && type == AdRequestType.REWARD_AD;
        }

        public bool IsOverlayingAd(AdRequestType type)
        {
            return true;
        }

        public void RequestPreloadAd()
        {
            LoadAdIfNotAlready();
        }

        public async Task<bool> LoadAdAsync(AdRequestType type, CancellationToken ct)
        {
            if (_loadedAd != null && _showingAd) return true;

            LoadAdIfNotAlready();
            await Utils.WaitUntilAsync(ct, () => _loadedAd != null);
            return _loadedAd != null;
        }
                
        public async Task<bool> ShowAdAsync(AdRequestType type, CancellationToken ct)
        {
            if (_loadedAd == null || !_loadedAd.CanShowAd()) return false;
            RegisterRewardedEventHandlers(_loadedAd);
            _showingAd = true;
            _adResult = false;

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            
            _loadedAd.Show(OnAdRewarded);
            try
            {
                await Utils.WaitWhileVerboseAsync(ct, () => _showingAd && !_hasReward);
            }
            catch (TimeoutException e)
            {
                Log.Warn($"Timeout for rewarded ad to return reward occurred: Ad completion state is {!_showingAd}, and" +
                         $"reward status is {_hasReward}.");
                return false;
            }

            return _hasReward;
        }

        private void OnAdRewarded(Reward reward)
        {
            Log.Info($"Rewarded ad rewarded the user. Type: {reward.Type}, amount: {reward.Amount}.");
            _hasReward = true;
            _rewardResult = true;
        }

        private void LoadAdIfNotAlready()
        {
            if (_loading) return;
            
            _loadedAd?.Destroy();
            _loadedAd = null;
            
            _showingAd = false;
            _adResult = false;
            
            _loading = true;
            
            var request = new AdRequest();
            RewardedAd.Load(_adUnitKey, request, (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Log.Error("Rewarded ad failed to load an ad with error : " + error);
                    return;
                }
            
                Log.Info("Rewarded ad loaded with response : " + ad?.GetResponseInfo());

                _loadedAd = ad!;
                _loading = false;
            });
        }
        
        private void RegisterRewardedEventHandlers(RewardedAd interstitialAd)
        {
            interstitialAd.OnAdPaid += (adValue) =>
            {
                Log.Info($"Reward ad paid {adValue.Value} {adValue.CurrencyCode}.");
            };
            
            interstitialAd.OnAdImpressionRecorded += () =>
            {
                Log.Info("Reward ad recorded an impression.");
            };
            
            interstitialAd.OnAdClicked += () =>
            {
                Log.Info("Reward ad was clicked.");
            };
            
            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Log.Info("Reward ad full screen content opened.");
            };
            
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Log.Info("Reward ad full screen content closed.");

                _showingAd = false;
                _adResult = true;
                LoadAdIfNotAlready();
            };
            
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Log.Info("Reward ad failed to open full screen content " +
                                    "with error : " + error);

                _showingAd = false;
                _adResult = false;
                LoadAdIfNotAlready();
            };
        }
    }
}