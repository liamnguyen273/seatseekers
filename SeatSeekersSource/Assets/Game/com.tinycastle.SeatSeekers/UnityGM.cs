using System;
using System.Collections.Generic;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public static class GMExtensions
    {
        public static bool HasNextLevel(this GM gm, LevelEntry currentLevel, out LevelEntry entry)
        {
            entry = gm.Get<GameDataManager>().GetNextLevelEntry(currentLevel.Id);
            
            return entry != null;
        }
        
        public static void RequestPlayLevelWithValidation(this GM gm, LevelEntry entry)
        {
            var mainGame = gm.Get<MainGameManager>();
            var loading = gm.Get<LoadingScreen>();
            var popupManager = gm.Get<PopupManager>();
            var accessor = gm.Get<GameSaveManager>().PlayerData;

            var energy = accessor.GetFromResources(GlobalConstants.ENERGY_RESOURCE) ?? 0;
            if (energy > 0)
            {
                energy -= 1;
                accessor.SetInResources(GlobalConstants.ENERGY_RESOURCE, energy, true);
                accessor.WriteDataAsync();
                
                loading.RequestLoad(mainGame.Activate(),
                    () =>
                    {
                        popupManager.HideAllPopups(true, true);
                        mainGame.LoadLevel(entry);
                    },
                    mainGame.StartGame);
            }
            else
            {
                var popup = popupManager.GetPopup<PopupRefill>(out var behaviour);
                popup.Show();
            }
        }
    }
    
    [DisallowMultipleComponent]
    public class UnityGM : UnityComp<GM>
    {
        [Header("Explicit manager references")]
        [SerializeField] private CompWrapper<UnityPopupManager> _unityPopupManager; 
        [SerializeField] private CompWrapper<UnityEffectMaker> _unityEffectMaker;
        [SerializeField] private CompWrapper<UnityAdManager> _unityAdManager;
        [SerializeField] private CompWrapper<LoadingScreen> _loadingScreen;
        
        [Header("Main Game references")]
        [SerializeField] private CompWrapper<MainGameManager> _mainGameManager;
        
        private LogObj Log { get; set; }
        
        private void Awake()
        {
            Log = new LogObj("GM");
            
            Application.targetFrameRate = 120;
            Log.Info($"Target framerate: {Application.targetFrameRate}.");
            
            DontDestroyOnLoad(this);
            
            // Make managers
            var dataManager = new GameDataManager();
            var saveManager = new GameSaveManager(new PlayerDataAccessor());
            
            var localizationManager = new LocalizationManager(saveManager, GMUtils.MakeLocalizationSuppliers());
            var analyticsEventManager = new AnalyticsEventManager(GMUtils.MakeAnalyticsAdapters());

            var unityPopupManager = _unityPopupManager.Comp;
            unityPopupManager.AttachComps();
            var popupManager = _unityPopupManager.Comp.Comp;

            var unityEffectMaker = _unityEffectMaker.Comp;
            unityEffectMaker.AttachComps();
            var effectMaker = _unityEffectMaker.Comp.Comp;

            var unityAdManager = _unityAdManager.Comp;
            var adManager = new AdManager(saveManager, GMUtils.MakeAdServiceProviders());
            unityAdManager.Comp = adManager;

            var mainGameManager = _mainGameManager.Comp;
            
            Log.Success("Created managers.");
            
            // Establish dependencies
            localizationManager.AddDependencies(saveManager);
            adManager.AddDependencies(saveManager);
            
            Log.Success("Established managers' dependencies.");
                        
            // TODO: IAP
            var gm = new GM(new IGameComponent[]
            {
                _loadingScreen.Comp,
                dataManager, 
                saveManager, 
                localizationManager,
                analyticsEventManager,
                popupManager,
                effectMaker,
                adManager,
                mainGameManager,
            });
            
            Comp = gm;
            GM.Instance = Comp;
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
            var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupMainMenu>(out var behaviour);
            popup.Show();

            _playerData = GM.Instance.Get<GameSaveManager>().PlayerData;
            InitializeEnergyWatch();
        }
        
        private void OnInitializationLoadingOver()
        {
            Log.Success("Game started, continue to load game screens.");
        }

        private void Update()
        {
            if (_playerData != null)
            {
                UpdateEnergyWatch(Time.deltaTime);
            }
        }

        private void InitializeEnergyWatch()
        {
            _timer = _playerData.EnergyRechargeTimer;
            var nowTime = DateTime.UtcNow;
            var lastSaveTime = _playerData.GetLastModified();
            var span = nowTime - lastSaveTime;
            var seconds = (int)Math.Ceiling(span.TotalSeconds);

            _timer -= seconds;
            var energyCount = _playerData.GetFromResources(GlobalConstants.ENERGY_RESOURCE) ?? 0;

            while (energyCount < GlobalConstants.MAX_ENERGY && _timer < 0)
            {
                _timer += GlobalConstants.ENERGY_RECHARGE_TIME;
                ++energyCount;
            }

            if (_timer < 0) _timer = 0;

            _secondTimer = 1f; // Check every second
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
                var currEnergy = _playerData.GetFromResources(GlobalConstants.ENERGY_RESOURCE) ?? 0;
                if (_timer < 0 && currEnergy < GlobalConstants.MAX_ENERGY)
                {
                    currEnergy += 1;
                    _timer = GlobalConstants.ENERGY_RECHARGE_TIME;
                    _playerData.SetInResources(GlobalConstants.ENERGY_RESOURCE, currEnergy, true);
                }
                
                _playerData.EnergyRechargeTimer = _timer;
            }
        }
    }

    internal static class GMUtils
    {
        public static IAnalyticsServiceAdapter[] MakeAnalyticsAdapters()
        {
            var list = new List<IAnalyticsServiceAdapter>();
            
#if HAS_FIREBASE
            var firebaseAdapter = new FirebaseServiceAdapter();
            list.Add(firebaseAdapter);
#endif

#if HAS_APPSFLYER
            var appsflyerAdapter = new AppFlyerServiceAdapter();
            list.Add(appsflyerAdapter);
#endif
            
            return list.ToArray();
        }

        internal static IAdServiceProvider[] MakeAdServiceProviders()
        {
            var list = new List<IAdServiceProvider>();

#if TO_DO
            var firebaseAdapter = new FirebaseServiceAdapter();
            list.Add(firebaseAdapter);
#endif
            
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