using System;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Unity.Consents;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace com.brg.Unity.AdMob
{
    public static class AdMobHelper
    {
        private const string BANNER_CONFIG_PATH = "AdMobSettings/AdMobBannerConfig";
        private const string CONFIG_PATH = "AdMobSettings/AdMobConfig";
        
        private static AdMobConfig _config = null;
        private static AdMobBannerConfig _bannerConfig = null;
        
        private static bool _initializing = false;
        private static bool _initialized = false;
        private static IProgress _initProgress;

        public static bool Initialized => _initialized;

        public static AdMobConfig Config
        {
            get
            {
                if (_config == null)
                {
                    try
                    {
                        var config = Resources.Load<AdMobConfig>(CONFIG_PATH);
                        _config = config;
                    }
                    catch (Exception e)
                    {
                        LogObj.Default.Error("AdMobHelper", e);
                        LogObj.Default.Warn("AdMobHelper", "Failed to read config.");
                        _config = ScriptableObject.CreateInstance<AdMobConfig>();
                    }
                }

                return _config;
            }
        }        
        
        public static AdMobBannerConfig BannerConfig
        {
            get
            {
                if (_bannerConfig == null)
                {
                    try
                    {
                        var config = Resources.Load<AdMobBannerConfig>(BANNER_CONFIG_PATH);
                        _bannerConfig = config;
                    }
                    catch (Exception e)
                    {
                        LogObj.Default.Error("AdMobHelper", e);
                        LogObj.Default.Warn("AdMobHelper", "Failed to read banner config, will use default.");
                        _bannerConfig = ScriptableObject.CreateInstance<AdMobBannerConfig>();
                    }
                }

                return _bannerConfig;
            }
        }

        internal static IProgress InitProgress
        {
            get
            {
                _initProgress ??= new SingleProgress(
                    () => !_initializing,
                    () => _initialized,
                    () => _initialized ? 1f : 0f, 
                    () => 1f);
                return _initProgress;
            }
        }
        
        public static IProgress Initialize()
        {
            if (_initializing || _initialized)
            {
                return InitProgress;
            }
            
            _initializing = true;

            var progress = GoogleCMP.Initialize();
            progress.Task.ContinueWith((t) =>
            {
                OnConsentUpdated(t.Result);
            }, TaskScheduler.FromCurrentSynchronizationContext());

            return InitProgress;
        }

        private static void OnConsentUpdated(bool successfully)
        {
            if (!successfully)
            {
                LogObj.Default.Error("AdMob", "GoogleCMP cannot gathered consent form. The application will attempt to use old consent data (if any).");
            }

            if (ConsentInformation.CanRequestAds())
            {
                LogObj.Default.Info("AdMob","Ad can be requested.");
                MobileAds.Initialize(OnInitialized);
            }
            else
            {
                LogObj.Default.Error("AdMob","Cannot initialize the mobile ads. There is no consent form.");
                _initializing = false;
                _initialized = false;
            }
        }

        private static void OnInitialized(InitializationStatus status)
        {
            LogObj.Default.Success("AdMob","Initialization success.");
            _initializing = false;
            _initialized = true;
        }
    }
}