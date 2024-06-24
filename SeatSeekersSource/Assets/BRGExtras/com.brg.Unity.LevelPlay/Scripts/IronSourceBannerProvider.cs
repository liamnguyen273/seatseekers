using System.Threading;
using System.Threading.Tasks;
using com.brg.Common;

namespace com.brg.Unity.LevelPlay
{
    public class IronSourceBannerProvider : IAdServiceProvider
    {
        private bool _loading;
        private bool _loaded;
        private bool _subscribed;
        private TaskCompletionSource<bool> _showTcs;
        
        public IProgress Initialize()
        {
            if (!_subscribed)
            {
                IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
                IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
                IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
                IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
                IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
                IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;
                _subscribed = true;
            }
            LevelPlayInitialization.Initialize();
            return new ImmediateProgress();
        }

        public bool Initialized => LevelPlayInitialization.Initialized;
        
        public bool CanHandleRequest(AdRequestType type)
        {
            return Initialized && type is AdRequestType.BANNER_AD;
        }

        public bool IsOverlayingAd(AdRequestType type)
        {
            return false;
        }

        public void RequestPreloadAd()
        {
            LoadBanner();
        }

        public async Task<bool> LoadAdAsync(AdRequestType type, CancellationToken ct)
        {
            if (_loading || _loaded) return true;
            LoadBanner();
            return true;
        }

        public Task<bool> ShowAdAsync(AdRequestType type, CancellationToken ct)
        {
            if (_showTcs != null) return _showTcs.Task;
            _showTcs = new TaskCompletionSource<bool>();
            
            IronSource.Agent.displayBanner();
            
            ct.Register(OnAdCancelled);
            return _showTcs.Task;
        }

        private void OnAdCancelled()
        {
            if (_loaded)
            {
                IronSource.Agent.hideBanner();
            }
        }

        private void LoadBanner()
        {
            if (_loading || _loaded) return;
            _loading = true;
            _loaded = false;
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
        }

        private void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
        {
            LogObj.Default.Info(nameof(IronSourceBannerProvider), $"Banner loaded.");

            if (_loading)
            {
                _loading = false;
                _loaded = true;
            }
            else
            {
                _loading = false;
                _loaded = true;
                IronSource.Agent.hideBanner();
            }
        }

        private void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError) 
        {
            LogObj.Default.Info(nameof(IronSourceBannerProvider), $"Banner failed to load. Error:\n{ironSourceError}");
            _loading = false;
            _loaded = false;
            LoadBanner();
        }

        private void BannerOnAdClickedEvent(IronSourceAdInfo adInfo) 
        {
        }

        private void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo) 
        {
        }

        private void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo) 
        {
        }

        private void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo) 
        {
        }
    }
}