using System;
using com.brg.Common;
using com.brg.Common.Initialization;

namespace com.brg.UnityCommon.Ads
{
    public partial class AdManager
    {        
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
        
        protected override void StartInitializationBehaviour()
        {
#if MAX_SDK
            MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxInitialized;

            MaxSdk.SetSdkKey(SDK_KEY);
            MaxSdk.InitializeSdk();
#else
            EndInitialize(true);
#endif
        }

        protected override void EndInitializationBehaviour()
        {
            GM.Instance.Player.OnOwnEvent += OnOwn;
        }

        public void ManuallyInitializeBannerAds()
        {
            InitializeBannerAds();
        }
       
#if MAX_SDK
        private void OnMaxInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            InitializeInterstitialAds();
            InitializeRewardedAds();
            // InitializeBannerAds(); This cause hangs since requires remote config to set the theme
            
            EndInitialize(true);
        }
#else
#endif

        private void OnOwn(string id)
        {
            if (id == GlobalConstants.NO_AD_ITEM_NAME)
            {
                RequestHideBanner();
            }
        }
    }
}