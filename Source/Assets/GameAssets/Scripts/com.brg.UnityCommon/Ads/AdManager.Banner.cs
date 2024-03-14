using System;
using UnityEngine;

namespace com.brg.UnityCommon.Ads
{
    public partial class AdManager
    {
        private static readonly Color BANNER_COLOR = Utilities.FromHex("#a66a1f");
        private static readonly Color BANNER_COLOR_CHRISTMAS = Utilities.FromHex("#205ca8");

        private bool _hasBanner;
        
        public event Action OnBannerShowEvent;
        public event Action OnBannerHideEvent;

        public bool HasBanner => !GM.Instance.Player.Own(GlobalConstants.NO_AD_ITEM_NAME) && _hasBanner;

        public Rect GetBannerLayout()
        {
#if MAX_SDK
            return MaxSdk.GetBannerLayout(BANNER_AD_UNIT);
#else
            return new Rect(0, 0, 200, 100);
#endif
        }

        public void SetTheme(string themeName)
        {
#if MAX_SDK
            MaxSdk.SetBannerBackgroundColor(BANNER_AD_UNIT, 
                GM.Instance.GetTheme() == GlobalConstants.CHRISTMAS_THEME ? BANNER_COLOR_CHRISTMAS : BANNER_COLOR);
#else
#endif
        }
        
        public void InitializeBannerAds()
        {
#if MAX_SDK
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(BANNER_AD_UNIT, MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional
            SetTheme(GlobalConstants.DEFAULT_THEME);

            MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
#else
#endif
        }

        public void RequestShowBanner()
        {
            if (GM.Instance.Player.Own(GlobalConstants.NO_AD_ITEM_NAME)) return;
      
#if MAX_SDK
            MaxSdk.ShowBanner(BANNER_AD_UNIT);
#else
#endif
            _hasBanner = true;
            OnBannerShowEvent?.Invoke();
        }

        public void RequestHideBanner()
        {
#if MAX_SDK
            MaxSdk.HideBanner(BANNER_AD_UNIT);
#else
#endif
            _hasBanner = false;
            OnBannerHideEvent?.Invoke();
        }

#if MAX_SDK        
        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _hasBanner = true;
        }
#else
#endif

#if MAX_SDK
        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            Log.Warn($"Banner ad load failed, reason: {errorInfo.Message}");
        }
#else
#endif
        
#if MAX_SDK
        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log.Info("Banner ad clicked");
        }
#else
#endif
        
#if MAX_SDK
        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log.Info("Banner ad expanded");
        }
#else
#endif
        
#if MAX_SDK
        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log.Info("Banner ad collapsed");
        }
#else
#endif
    }
}