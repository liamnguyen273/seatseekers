using System.Threading;
using System.Threading.Tasks;
using com.brg.Common;

namespace com.brg.Unity.LevelPlay
{
    public class IronSourceBannerProvider : IAdServiceProvider
    {
        private bool _loading;
        private bool _loaded;
        private TaskCompletionSource<bool> _showTcs;
        
        private bool _init;
        
        public IProgress Initialize()
        {
            if (_init) return new ImmediateProgress();
            LevelPlayInitialization.Initialize();
            LevelPlayInitialization.InitTask.ContinueWith((t) =>
            {
                IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
                IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
                IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
                IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
                IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
                IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;
                LoadBanner();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            _init = true;
            return new ImmediateProgress();
        }

        public bool Initialized => LevelPlayInitialization.Initialized;
        
        public bool CanHandleRequest(AdRequestType type)
        {
            return type == AdRequestType.BANNER_AD;
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
            _loading = false;
            _loaded = true;
            IronSource.Agent.displayBanner();
        }

        private void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError) 
        {
            LogObj.Default.Info(nameof(IronSourceBannerProvider), $"Banner failed to load. Error:\n{ironSourceError}");
            _loading = false;
            _loaded = false;
            
            Task.Delay(5000).ContinueWith((t) =>
            {
                LoadBanner();
            }, TaskScheduler.FromCurrentSynchronizationContext());
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