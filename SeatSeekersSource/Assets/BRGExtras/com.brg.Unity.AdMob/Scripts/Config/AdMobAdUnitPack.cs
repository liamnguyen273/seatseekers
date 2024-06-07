using System;
using System.Collections.Generic;
using com.brg.Common;
using GoogleMobileAds.Api;

namespace com.brg.Unity.AdMob
{
    [Serializable]
    public class AdMobAdUnitPack
    {
        public string Interstitial;
        public string Rewarded;
        public string Banner;
        public string InterstitialRewarded;
        public string Native;
        public string AppOpen;

        public List<IAdServiceProvider> CreateAvailableProviders()
        {
            var list = new List<IAdServiceProvider>();

            if (!string.IsNullOrWhiteSpace(Interstitial))
            {
                list.Add(new AdMobInterstitialProvider(Interstitial));
            }
            
            if (!string.IsNullOrWhiteSpace(Rewarded))
            {
                list.Add(new AdMobRewardProvider(Rewarded));
            }

            if (!string.IsNullOrWhiteSpace(Banner))
            {
                var bannerConfig = AdMobHelper.BannerConfig;
                list.Add(new AdMobBannerProvider(Banner, bannerConfig.Size, bannerConfig.Position));
            }
            
            if (!string.IsNullOrWhiteSpace(InterstitialRewarded))
            {
                LogObj.Default.Warn("Interstitial Rewarded Ad provider is not implemented yet.");
            }
                        
            if (!string.IsNullOrWhiteSpace(Native))
            {
                LogObj.Default.Warn("Native Ad provider is not implemented yet.");
            }                
            
            if (!string.IsNullOrWhiteSpace(AppOpen))
            {
                LogObj.Default.Warn("App Open Ad provider is not implemented yet.");
            }

            return list;
        }
    }
}