using System;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.brg.Unity.AdMob
{
    public enum AdMobCommonBannerSize
    {
        Banner,
        MediumRectangle,
        IABBanner,
        Leaderboard,
    }
    
    [CreateAssetMenu(menuName = "BRG/Extras/AdMob/Banner Config", fileName = "AdMobBannerConfig", order = 2)]
    public class AdMobBannerConfig : ScriptableObject
    {
        [SerializeField] private AdMobCommonBannerSize _commonSize = AdMobCommonBannerSize.Banner;
        [SerializeField] private AdPosition _adPosition = AdPosition.Bottom;

        public AdSize Size
        {
            get
            {
                return _commonSize switch
                {
                    AdMobCommonBannerSize.Banner => AdSize.Banner,
                    AdMobCommonBannerSize.MediumRectangle => AdSize.MediumRectangle,
                    AdMobCommonBannerSize.IABBanner => AdSize.IABBanner,
                    AdMobCommonBannerSize.Leaderboard => AdSize.Leaderboard,
                    _ => AdSize.Banner
                };
            }
        }
        public AdPosition Position => _adPosition;
    }
}