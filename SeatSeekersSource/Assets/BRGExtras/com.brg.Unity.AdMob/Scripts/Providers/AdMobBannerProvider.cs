using System.Threading;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.UnityCommon;
using GoogleMobileAds.Api;
using AdRequest = GoogleMobileAds.Api.AdRequest;
using Utils = com.brg.Common.Utils;

namespace com.brg.Unity.AdMob
{
    public class AdMobBannerProvider : IAdServiceProvider
    {
        private readonly string _adUnitKey;
        private readonly AdSize _adSize;
        private readonly AdPosition _adPosition;
        
        private BannerView _bannerView;
        private bool _loading;
        private bool _loadResult;
        
        public bool Initialized => AdMobHelper.Initialized;

        public AdMobBannerProvider(string adUnitKey, AdSize adSize, AdPosition adPosition)
        {
            _adUnitKey = adUnitKey;
            _adSize = adSize;
            _adPosition = adPosition;
            
            _loading = false;
        }
        
        public IProgress Initialize()
        {
            return AdMobHelper.Initialize();
        }
        
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
            LoadAdIfNotAlready();
        }
        
        public async Task<bool> LoadAdAsync(AdRequestType type, CancellationToken ct)
        {
            if (_bannerView != null && !_loading) return _loadResult;

            LoadAdIfNotAlready();
            await Utils.WaitWhileVerboseAsync(ct, () => _loading);
            return _loadResult;
        }

        public Task<bool> ShowAdAsync(AdRequestType type, CancellationToken ct)
        {
            // Is not applicable, return load result instead, though.
            return Task.FromResult(_bannerView != null && !_loading && _loadResult);
        }
        
        private void LoadAdIfNotAlready()
        {
            if (_loading) return;
            
            _bannerView?.Destroy();
            _bannerView = new BannerView(_adUnitKey, _adSize, _adPosition);
            
            RegisterEventHandlers(_bannerView);
            var request = new AdRequest();
            _bannerView.LoadAd(request);
        }
        
        private void RegisterEventHandlers(BannerView bannerView)
        {
            // Raised when an ad is loaded into the banner view.
            bannerView.OnBannerAdLoaded += () =>
            {
                LogObj.Default.Info("Banner view loaded an ad with response : "
                                    + bannerView.GetResponseInfo());
                
                _loading = false;
                _loadResult = true;
            };
            
            // Raised when an ad fails to load into the banner view.
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                LogObj.Default.Error("Banner view failed to load an ad with error : "
                                     + error);
                
                _loading = false;
                _loadResult = false;
                LoadAdIfNotAlready();
            };
            
            // Raised when the ad is estimated to have earned money.
            bannerView.OnAdPaid += (adValue) =>
            {
                LogObj.Default.Info($"Banner view paid {adValue.Value} {adValue.CurrencyCode}.");
            };
            
            // Raised when an impression is recorded for an ad.
            bannerView.OnAdImpressionRecorded += () =>
            {
                LogObj.Default.Info("Banner view recorded an impression.");
            };
            
            // Raised when a click is recorded for an ad.
            bannerView.OnAdClicked += () =>
            {
                LogObj.Default.Info("Banner view was clicked.");
            };
            
            // Raised when an ad opened full screen content.
            bannerView.OnAdFullScreenContentOpened += () =>
            {
                LogObj.Default.Info("Banner view full screen content opened.");
            };
            
            // Raised when the ad closed full screen content.
            bannerView.OnAdFullScreenContentClosed += () =>
            {
                LogObj.Default.Info("Banner view full screen content closed.");
            };
        }
    }
}