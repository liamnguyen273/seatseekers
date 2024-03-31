using System.Collections.Generic;
using System.Linq;
using System.Threading;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
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
            var saveManager = new GameSaveManager();
            
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
        }
        
        private void OnInitializationLoadingOver()
        {
            Log.Success("Game started, continue to load game screens.");
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