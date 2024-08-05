using System.Threading;
using System.Threading.Tasks;
using com.brg.Common;
using DG.Tweening;

namespace com.brg.Unity.LevelPlay
{
    public class IronSourceInterstitialProvider : IAdServiceProvider
    {
        private bool _loading;
        private bool _loaded;
        private bool _showingAd;
        private bool _adResult;

        private bool _init;
        
        public IProgress Initialize()
        {
            if (_init) return new ImmediateProgress();
            LevelPlayInitialization.Initialize();
            LevelPlayInitialization.InitTask.ContinueWith((t) =>
            {
                IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
                IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
                IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
                IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
                IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
                IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
                IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;
                IronSource.Agent.loadInterstitial();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            _init = true;
            return new ImmediateProgress();
        }

        public bool Initialized => LevelPlayInitialization.Initialized;
        
        public bool CanHandleRequest(AdRequestType type)
        {
            return type == AdRequestType.INTERSTITIAL_AD;
        }

        public bool IsOverlayingAd(AdRequestType type)
        {
            return true;
        }

        public void RequestPreloadAd()
        {
            // IronSource.Agent.loadInterstitial();
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
            if (!_loaded || !IronSource.Agent.isInterstitialReady()) return false;
            
            _showingAd = true;
            _adResult = false;

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            
            IronSource.Agent.showInterstitial();
            await Utils.WaitWhileVerboseAsync(ct, () => _showingAd, 50);
            return _adResult;
        }
        
        private void LoadAdIfNotAlready()
        {
            if (_loading || _loaded || !IronSource.Agent.isInterstitialReady()) return;

            _loading = true;
            _loaded = false;
            
            IronSource.Agent.loadInterstitial();
        }
        
        private void InterstitialOnAdReadyEvent(IronSourceAdInfo obj)
        {
            LogObj.Default.Info(nameof(IronSourceInterstitialProvider), $"Ad {obj.instanceId} is ready.");
            
            _loading = false;
            _loaded = true;
        }
        
        private void InterstitialOnAdLoadFailed(IronSourceError error)
        {
            LogObj.Default.Info(nameof(IronSourceInterstitialProvider), $"Ad failed to load with error:\n{error}");
                
            _loading = false;
            _loaded = false;
            DOVirtual.DelayedCall(10f, LoadAdIfNotAlready).Play();
        }
        
        private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo obj)
        {
            LogObj.Default.Info(nameof(IronSourceInterstitialProvider), $"Ad {obj.instanceId} is shown.");
        }
        
        private void InterstitialOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo info)
        {
            LogObj.Default.Info(nameof(IronSourceInterstitialProvider), $"Ad {info.instanceId} failed to show. Error:\n{error}");
            
            _adResult = false;
            _showingAd = false;
            LoadAdIfNotAlready();
        }
        
        private void InterstitialOnAdClosedEvent(IronSourceAdInfo obj)
        {
            LogObj.Default.Info(nameof(IronSourceInterstitialProvider), $"Ad {obj.instanceId} is closed.");
            
            _adResult = true;
            _showingAd = false;
            LoadAdIfNotAlready();
        }

        private void InterstitialOnAdClickedEvent(IronSourceAdInfo obj)
        {
            LogObj.Default.Info(nameof(IronSourceInterstitialProvider), $"Clicked on ad {obj.instanceId}");
        }

        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo obj)
        {
            LogObj.Default.Info(nameof(IronSourceInterstitialProvider), $"Ad {obj.instanceId} is opened.");
        }
    }
}