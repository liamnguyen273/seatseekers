using System.Threading;
using System.Threading.Tasks;
using com.brg.Common;
using GoogleMobileAds.Api;
using AdRequest = GoogleMobileAds.Api.AdRequest;
using Utils = com.brg.Common.Utils;

namespace com.brg.Unity.AdMob
{
    public class AdMobInterstitialProvider : IAdServiceProvider
    {
        private readonly string _adUnitKey;
        private InterstitialAd _loadedAd;
        private bool _loading;
        private bool _showingAd;
        private bool _adResult;

        private LogObj Log { get; set; }
        
        public bool Initialized => AdMobHelper.Initialized;
        
        public AdMobInterstitialProvider(string adUnitKey)
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
            return !_showingAd && type == AdRequestType.INTERSTITIAL_AD;
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
            RegisterInterstitialEventHandlers(_loadedAd);
            _showingAd = true;
            _adResult = false;

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            
            _loadedAd.Show();
            await Utils.WaitWhileVerboseAsync(ct, () => _showingAd, 50);
            return _adResult;
        }
        
        private void LoadAdIfNotAlready()
        {
            if (_loading) return;
            
            _loadedAd?.Destroy();
            _loadedAd = null;
            
             _loading = true;
            
            var request = new AdRequest();
            InterstitialAd.Load(_adUnitKey, request, (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Log.Error("Interstitial ad failed to load an ad with error : " + error);
                    return;
                }
            
                Log.Info("Interstitial ad loaded with response : " + ad?.GetResponseInfo());

                _loadedAd = ad!;
                _loading = false;
            });
        }
        
        private void RegisterInterstitialEventHandlers(InterstitialAd interstitialAd)
        {
            interstitialAd.OnAdPaid += (adValue) =>
            {
                Log.Info($"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}.");
            };
            
            interstitialAd.OnAdImpressionRecorded += () =>
            {
                Log.Info("Interstitial ad recorded an impression.");
            };
            
            interstitialAd.OnAdClicked += () =>
            {
                Log.Info("Interstitial ad was clicked.");
            };
            
            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Log.Info("Interstitial ad full screen content opened.");
            };
            
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Log.Info("Interstitial ad full screen content closed.");

                _adResult = true;
                _showingAd = false;
                LoadAdIfNotAlready();
            };
            
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Log.Info("Interstitial ad failed to open full screen content " +
                                    "with error : " + error);

                _adResult = false;
                _showingAd = false;
                LoadAdIfNotAlready();
            };
        }
    }
}