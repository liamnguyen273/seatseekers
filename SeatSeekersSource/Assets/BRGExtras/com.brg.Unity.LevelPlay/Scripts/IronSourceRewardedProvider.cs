using System.Threading;
using System.Threading.Tasks;
using com.brg.Common;

namespace com.brg.Unity.LevelPlay
{
    public class IronSourceRewardedProvider : IAdServiceProvider
    {
        private bool _loading;
        private bool _loaded;
        private bool _showingAd;
        private bool _adResult;

        private bool _subscribed;
        
        public IProgress Initialize()
        {
            if (!_subscribed)
            {
                IronSourceRewardedVideoEvents.onAdReadyEvent += RewardedVideoOnAdReadyEvent;
                IronSourceRewardedVideoEvents.onAdLoadFailedEvent += RewardedVideoOnAdLoadFailedEvent;
                IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
                IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
                IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
                IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
                IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
                
                _subscribed = true;
            }
            
            return LevelPlayInitialization.Initialize();
        }

        public bool Initialized => LevelPlayInitialization.Initialized;
        
        public bool CanHandleRequest(AdRequestType type)
        {
            return type is AdRequestType.REWARD_AD;
        }

        public bool IsOverlayingAd(AdRequestType type)
        {
            return true;
        }

        public void RequestPreloadAd()
        {
            IronSource.Agent.loadRewardedVideo();
        }

        public async Task<bool> LoadAdAsync(AdRequestType type, CancellationToken ct)
        {
            if (!_loading || _showingAd) return true;
            
            LoadAdIfNotAlready();
            
            await Utils.WaitWhileAsync(ct, () => _loading);
            return _loaded;
        }

        public async Task<bool> ShowAdAsync(AdRequestType type, CancellationToken ct)
        {
            if (!_loaded) return false;
            
            _showingAd = true;
            _adResult = false;

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            
            IronSource.Agent.showRewardedVideo();
            await Utils.WaitWhileVerboseAsync(ct, () => _showingAd, 50);
            return _adResult;
        }
        
        private void LoadAdIfNotAlready()
        {
            if (_loading) return;

            _loading = true;
            _loaded = false;
            
            IronSource.Agent.loadRewardedVideo();
        }
        
        private void RewardedVideoOnAdReadyEvent(IronSourceAdInfo adInfo)
        {
            LogObj.Default.Info(nameof(IronSourceRewardedProvider), $"Ad {adInfo.instanceId} is ready.");
            
            _loading = false;
            _loaded = true;
        }

        private void RewardedVideoOnAdLoadFailedEvent(IronSourceError error) 
        {
            LogObj.Default.Info(nameof(IronSourceRewardedProvider), $"Ad has failed. Error:\n{error}.");
                            
            _loading = false;
            _loaded = false;
        }

        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            LogObj.Default.Info(nameof(IronSourceRewardedProvider), $"Ad {adInfo.instanceId} is opened.");
        }

        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            LogObj.Default.Info(nameof(IronSourceRewardedProvider), $"Ad {adInfo.instanceId} is closed.");
            LoadAdIfNotAlready();
        }

        private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            LogObj.Default.Info(nameof(IronSourceRewardedProvider), $"Ad {adInfo.instanceId} is completed.");
            
            _adResult = true;
            _showingAd = false;
            LoadAdIfNotAlready();
        }

        private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {            
            LogObj.Default.Info(nameof(IronSourceRewardedProvider), $"Ad {adInfo.instanceId} failed to show. Error:\n{error}");
            
            _adResult = false;
            _showingAd = false;
            LoadAdIfNotAlready();
        }

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            LogObj.Default.Info(nameof(IronSourceRewardedProvider), $"Clicked on ad {adInfo.instanceId}");
        }
    }
}