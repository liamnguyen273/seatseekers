using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Unity;
using com.brg.Unity.Consents;
using com.brg.Unity.FirebaseAnalytics;
using com.brg.Unity.LevelPlay;
using com.brg.Unity.Singular;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using DG.Tweening;
using JSAM;
using UnityEngine;
using Random = System.Random;

namespace com.tinycastle.SeatSeekers
{
    public static class GMExtensions
    {
        public static bool HasNextLevel(this GM gm, LevelEntry currentLevel, out LevelEntry entry)
        {
            entry = gm.Get<GameDataManager>().GetNextLevelEntry(currentLevel.Id);
            
            return entry != null;
        }

        public static void RequestPlayMultiplayer(this GM gm, int intensity)
        {
            var gen = new LevelGen();
            switch (intensity)
            {
                case 1:
                    gen.CarW = 5;
                    gen.CarH = 7;
                    gen.GenSeatCount = 24;
                    gen.GenColorCount = 4;
                    break;
                case 2:
                    gen.CarW = 6;
                    gen.CarH = 8;
                    gen.GenSeatCount = 32;
                    gen.GenColorCount = 5;
                    break;
                case 3:
                    gen.CarW = 6;
                    gen.CarH = 10;
                    gen.GenSeatCount = 42;
                    gen.GenColorCount = 7;
                    break;
                case 4:
                    gen.CarW = 7;
                    gen.CarH = 12;
                    gen.GenSeatCount = 60;
                    gen.GenColorCount = 8;
                    break;
                default:
                    return;
            }
                        
            var mainGame = gm.Get<MainGameManager>();
            var loading = gm.Get<LoadingScreen>();
            var popupManager = gm.Get<PopupManager>();
            var accessor = gm.Get<GameSaveManager>().PlayerData;
            var data = gm.Get<GameDataManager>().GetLeaderboardNames();
                            
            loading.RequestLoad(mainGame.Activate(),
                () =>
                {
                    gen.LevelData = new LevelData(Array.Empty<int>(), gen.CarW, gen.CarH);
                    GEditor.GenerateLevel(gen);
                    popupManager.HideAllPopups(true, true);

                    var entry = new LevelEntry();
                    entry.DisplayName = "Versus";
                    entry.SortOrder = -1;
                    entry.Bundle = "multi";
                    entry.Id = $"multi_{intensity}";

                    var enemy = data[UnityEngine.Random.Range(0, data.Count)];
                    
                    mainGame.LoadCustomLevel(entry, gen.LevelData);
                    mainGame.StartGameMultiplayer(intensity, enemy);
                },
                mainGame.StartGame);
        }
        
        public static void RequestPlayLevelWithValidation(this GM gm, LevelEntry entry, ref bool hasAdButtonTimer)
        {
            var popupManager = gm.Get<PopupManager>();
            var accessor = gm.Get<GameSaveManager>().PlayerData;

            var energy = accessor.GetFromResources(Constants.ENERGY_RESOURCE) ?? 0;
            var inf = accessor.GetFromResources(Constants.INFINITE_ENERGY_RESOURCE) ?? 0;
#if FORCED_CHEAT
            if (true)
            #else
            if (energy > 0 || inf > 0)
#endif
            {
#if !FORCED_CHEAT
                if (inf <= 0)
                {
                    energy -= 1;
                    accessor.SetInResources(Constants.ENERGY_RESOURCE, energy, true);
                    GM.Instance.Get<GameSaveManager>().SaveAll();
                }
#endif

                gm.RequestPlayLevel(entry);
            }
            else
            {
                var popup = popupManager.GetPopup<PopupRefill>(out var behaviour);
                if (hasAdButtonTimer)
                {
                    hasAdButtonTimer = false;
                    behaviour.Timer = 30f;
                }
                
                popup.Show();
            }
        }

        public static void RequestPlayLevel(this GM gm, LevelEntry entry)
        {
            var mainGame = gm.Get<MainGameManager>();
            var loading = gm.Get<LoadingScreen>();
            var popupManager = gm.Get<PopupManager>();

            loading.RequestLoad(mainGame.Activate(),
                () =>
                {
                    popupManager.HideAllPopups(true, true);
                    mainGame.LoadLevel(entry);
                    mainGame.LoadAppropriateMusic();
                },
                mainGame.StartGame);
        }
        
        public static void RequestPlayLevelWithoutLoading(this GM gm, LevelEntry entry)
        {
            var mainGame = gm.Get<MainGameManager>();
            var loading = gm.Get<LoadingScreen>();
            var popupManager = gm.Get<PopupManager>();

            mainGame.Activate();
            popupManager.HideAllPopups(true, true);
            mainGame.LoadLevel(entry);
            mainGame.LoadAppropriateMusic();
            mainGame.StartGame();
        }
    }
    
    [DisallowMultipleComponent]
    public class UnityGM : UnityComp<GM>
    {
        public static UnityGM Instance { get; private set; }
        
        [Header("Explicit manager references")]
        [SerializeField] private CompWrapper<UnityPopupManager> _unityPopupManager; 
        [SerializeField] private CompWrapper<UnityAdManager> _unityAdManager;
        [SerializeField] private CompWrapper<LoadingScreen> _loadingScreen;
        [SerializeField] private CompWrapper<WaitScreenHelper> _waitHelper;
        
        [Header("Main Game references")]
        [SerializeField] private CompWrapper<MainGameManager> _mainGameManager;

        [SerializeField] private List<RectTransform> _bannerDodgables;
        
        private LogObj Log { get; set; }

        public WaitScreenHelper WaitHelper => _waitHelper;

        public List<RectTransform> BannerDodgables => _bannerDodgables;
        public PurchaseManager Purchase { get; private set; }
        
        private void Awake()
        {
            AudioManager.MusicMuted = false;
            AudioManager.SoundMuted = false;
            
            Instance = this;
            Log = new LogObj("GM");
            
            Application.targetFrameRate = 120;
            Log.Info($"Target framerate: {Application.targetFrameRate}.");
            
            DontDestroyOnLoad(this);

            var consentManager = new ConsentManager();
            
            // Make managers
            var dataManager = new GameDataManager();
            var saveManager = new GameSaveManager(new PlayerDataAccessor(), new PlayerExtraDataAccessor());
            
            var localizationManager = new LocalizationManager(saveManager, GMUtils.MakeLocalizationSuppliers());
            var analyticsEventManager = new AnalyticsEventManager(GMUtils.MakeAnalyticsAdapters());

            var unityPopupManager = _unityPopupManager.Comp;
            unityPopupManager.AttachComps();
            var popupManager = _unityPopupManager.Comp.Comp;
            
            var unityAdManager = _unityAdManager.Comp;
            var adManager = new AdManager(saveManager, null, GMUtils.MakeAdServiceProviders());
            unityAdManager.Comp = adManager;
            
            unityAdManager.SetPlayables(new TweenInOutPlayable(() =>
            {
                AudioManager.MusicMuted = true;
                AudioManager.SoundMuted = true;
                return unityAdManager._canvas.Comp.DOFade(1f, 0.5f);
            }, () =>
            {
                AudioManager.MusicMuted = false;
                AudioManager.SoundMuted = false;
                return unityAdManager._canvas.Comp.DOFade(0f, 0.5f);
            }));
            
            var purchaseManager = new PurchaseManager();
            Purchase = purchaseManager;

            var questManager = new QuestManager(saveManager);

            var mainGameManager = _mainGameManager.Comp;
            
            Log.Success("Created managers.");
            
            LevelPlayInitialization.Initialize();
            IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;
            
            // Establish dependencies
            saveManager.AddDependencies(dataManager);
            // localizationManager.AddDependencies(saveManager);
            adManager.AddDependencies(consentManager);
            // adManager.AddDependencies(analyticsEventManager);
            // adManager.AddDependencies(saveManager);
            purchaseManager.AddDependencies(saveManager, dataManager);
            // questManager.AddDependencies(saveManager);
            
            Log.Success("Established managers' dependencies.");
                        
            // TODO: IAP
            var gm = new GM(new IGameComponent[]
            {
                _loadingScreen.Comp,
                dataManager, 
                saveManager, 
                consentManager,
                analyticsEventManager,
                localizationManager,
                questManager,
                unityAdManager,
                popupManager,
                mainGameManager,
                purchaseManager
            });
            
            Comp = gm;
            GM.Instance = Comp;
        }

        private void ImpressionDataReadyEvent(IronSourceImpressionData obj)
        {
            GM.Instance.Get<GameSaveManager>().ExtraData.AddLtv(obj.revenue ?? 0.0);
            GM.Instance.Get<AnalyticsEventManager>().MakeEvent("ad_impression")
                .Add("platform", "IronSource")
                .Add("currency", "USD")
                .Add("revenue", obj.revenue ?? 0.0)
                .SendEvent();
        }

        private void Start()
        {
            var progress = Initialize();

            _loadingScreen.Comp.Initialize();
            _loadingScreen.Comp.RequestLoad(progress,
                OnInitializationCompleted,
                OnInitializationLoadingOver
                );
        }

        private void OnInitializationCompleted()
        {
            Log.Success("Initialization completed. Await loading transition out.");
            // GM.Instance.Get<MainGameManager>().LoadLevel(GM.Instance.Get<GameDataManager>().GetLevelEntry("level_1"));

            GM.Instance.Get<GameSaveManager>().PlayerData.InitializeLeaderboard();
            _playerData = GM.Instance.Get<GameSaveManager>().PlayerData;
            InitializeEnergyWatch();
            
            var count = GM.Instance.Get<GameSaveManager>().PlayerData.GetCompletedLevelCount();

            if (count == 0)
            {
                var entry = GM.Instance.Get<GameDataManager>().GetLevelEntry("level_1");
                GM.Instance.RequestPlayLevelWithoutLoading(entry);
            }
            else
            {
                            
                var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupMainMenu>(out var behaviour);
                popup.Show();
            }
            
            GM.Instance.Get<UnityAdManager>().Comp.PreloadAds();
        }
        
        private void OnInitializationLoadingOver()
        {
            Log.Success("Game started, continue to load game screens.");

            var hasAdFree = GM.Instance.Get<GameSaveManager>().GetAdSkippability(AdRequestType.BANNER_AD);

            foreach (var dod in BannerDodgables)
            {
                dod.anchorMin = new Vector2(dod.anchorMin.x, hasAdFree ? 0f : 0.08f);
            }

            GM.Instance.Get<GameSaveManager>().PlayerData.OwnershipsChangedEvent += OnOwnershipChanged;
            var adFree = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromOwnerships(Constants.AD_FREE_PACKAGE) ?? false;
            OnOwnershipChanged(this, (Constants.AD_FREE_PACKAGE, false, adFree));
            
            // GM.Instance.Get<AnalyticsEventManager>().MakeEvent("test")
            //     .Add("test_params", "test_value")
            //     .SendEvent();
        }

        private void OnOwnershipChanged(object sender, (string key, bool isRemoved, bool item) e)
        {
            if (e is { key: Constants.AD_FREE_PACKAGE, item: true })
            {
                IronSource.Agent.destroyBanner();
            }
        }

        public static float PlayTime { get; set; }

        private void Update()
        {
            if (_playerData != null)
            {
                UpdateEnergyWatch(Time.deltaTime);
            }

            PlayTime += Time.unscaledDeltaTime;
        }

        private void InitializeEnergyWatch()
        {
            _timer = _playerData.EnergyRechargeTimer;
            var nowTime = DateTime.UtcNow;
            var lastSaveTime = _playerData.GetLastModified();
            var span = nowTime - lastSaveTime;
            var seconds = (int)Math.Ceiling(span.TotalSeconds);

            LogObj.Default.Info("GM", $"Total offline seconds: {seconds}");

            var energyCount = _playerData.GetFromResources(Constants.ENERGY_RESOURCE) ?? 0;

            while (energyCount < Constants.MAX_ENERGY && seconds > 0)
            {
                seconds -= Constants.ENERGY_RECHARGE_TIME;
                energyCount += 1;
            }

            if (energyCount >= Constants.MAX_ENERGY)
            {
                _timer = _playerData.EnergyRechargeTimer;
            }
            else if (seconds > 0)
            {
                _timer = Mathf.Max(0, _timer - seconds);
            }

            
            if (_timer < 0) _timer = 0;
            
            _playerData.SetInResources(Constants.ENERGY_RESOURCE, energyCount, true);
            _secondTimer = 1f; // Check every second
            _playerData.EnergyRechargeTimer = _timer;
        }

        private PlayerDataAccessor _playerData;
        private int _timer = 0;
        private float _secondTimer = 0f;
        private void UpdateEnergyWatch(float dt)
        {
            _secondTimer -= dt;
            while (_secondTimer < 0f)
            {
                _secondTimer += 1f;
                _timer -= 1;

                var infEnergyTime = _playerData.GetFromResources(Constants.INFINITE_ENERGY_RESOURCE) ?? 0;

                if (infEnergyTime > 0)
                {
                    infEnergyTime -= 1;
                    _timer = Constants.ENERGY_RECHARGE_TIME;
                    _playerData.SetInResources(Constants.INFINITE_ENERGY_RESOURCE, infEnergyTime, true);
                }
                
                var currEnergy = _playerData.GetFromResources(Constants.ENERGY_RESOURCE) ?? 0;
                if (_timer < 0 && currEnergy < Constants.MAX_ENERGY)
                {
                    currEnergy += 1;
                    _timer = Constants.ENERGY_RECHARGE_TIME;
                    _playerData.SetInResources(Constants.ENERGY_RESOURCE, currEnergy, true);
                }
                                    
                if (currEnergy >= Constants.MAX_ENERGY)
                {
                    _timer = Constants.ENERGY_RECHARGE_TIME;
                }
                
                _playerData.EnergyRechargeTimer = _timer;
            }
        }


        private void OnApplicationPause(bool pauseStatus)
        {
            IronSource.Agent.onApplicationPause(pauseStatus);
        }
    }

    internal static class GMUtils
    {
        public static IAnalyticsServiceAdapter[] MakeAnalyticsAdapters()
        {
            var list = new List<IAnalyticsServiceAdapter>();

            var firebaseAdapter = new FirebaseServiceAdapter();
            list.Add(firebaseAdapter);
            var singularAdapter = new SingularServiceAdapter();
            list.Add(singularAdapter);
            
            return list.ToArray();
        }

        internal static IAdServiceProvider[] MakeAdServiceProviders()
        {
            var list = new List<IAdServiceProvider>();

            var inter = new IronSourceInterstitialProvider();
            var reward = new IronSourceRewardedProvider();
            var banner = new IronSourceBannerProvider();
            list.Add(inter);
            list.Add(reward);
            list.Add(banner);
            
            return list.ToArray();
        }
        
        internal static ILocalizationSupplier[] MakeLocalizationSuppliers()
        {
            var list = new List<ILocalizationSupplier>();

            // Code localizations here
            
            return list.ToArray();
        }
    }
}