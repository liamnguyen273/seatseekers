using System;
using com.brg.Common.Random;
using JSAM;
using System.Collections.Generic;
using com.brg.Common.Initialization;
using com.brg.UnityCommon.Data;
using com.brg.UnityCommon.RemoteConfig;
using com.brg.UnityCommon.UI;
using UnityEngine;

#if JSAM_LIBRARY
using Sounds = LibrarySounds;
using Musics = LibraryMusics;
#else
namespace com.brg.UnityCommon
{
    public enum Sounds
    {
        Button,
        PositiveFeedback,
    }

    public enum Musics
    {
        
    }
}
#endif

namespace com.brg.UnityCommon
{
    public partial class GM : MonoManagerBase
    {
        public static GM Instance { get; private set; }

        [Header("Params")]
        [SerializeField] private bool _shouldLog;
        [SerializeField] private bool _isCheat;  
        [SerializeField] private string _forcedTheme = "default";
        [SerializeField] private bool _showMaxDebugger;
        
        [Header("Common Components")]
        [SerializeField] private WaitScreenHelper _waitHelper;
        [SerializeField] private LoadingScreen _loadingScreen;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private GameObject[] _additionalCompHosts;

        private Dictionary<Type, IGMComponent> _components;
        
        private List<IInitializable> _primaryManagers;
        private List<IInitializable> _secondaryManagers;

        private IRandomEngine _rng;

#if FORCED_CHEAT
        public bool IsCheat => true;
#elif !UNITY_EDITOR
        public bool IsCheat => false;
#else
        public bool IsCheat => _isCheat;
#endif
        
        public IRandomEngine Rng => _rng;
        
        public LoadingScreen Loading => _loadingScreen;  
        public WaitScreenHelper WaitHelper => _waitHelper;

        public event Action<string> OnThemeChangeEvent;
        
        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = -1;

            var seed = (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds;
            _rng = RandomEngineFactory.CreateEngine(RandomEngineEnum.STANDARD, seed);
        }

        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            var dt = Time.deltaTime;

            UpdateInitialization(dt);
            UpdateCheckInternet(dt);
        }

        public T Get<T>() where T : IGMComponent
        {
            var hasComp = _components.TryGetValue(typeof(T), out var component);

            if (!hasComp)
            {
                throw new ArgumentException($"GM does not contain component {nameof(T)}. Getting this component is not possible");
            }
            
            return (T)component!;
        }

        public bool ResolveUnlockCondition(LevelEntry level)
        {
            var condition = level.UnlockCondition;
            if (IsCheat) return true;
            
            return condition switch
            {
                GlobalConstants.UNLOCK_CONDITION_NONE => true,
                GlobalConstants.UNLOCK_CONDITION_SEQUENTIAL => ResolveSequentialUnlockCondition(level),
                GlobalConstants.UNLOCK_CONDITION_OWN => Player.Own(level.Id),
                _ => ResolveLevelUnlockConditionByParsing(level)
            };
        }

        public void SetTheme(string themeName)
        {
            if (IsCheat) themeName = _forcedTheme;
            Ad.SetTheme(themeName);
            OnThemeChangeEvent?.Invoke(themeName);
        }

        public string GetTheme()
        {
            if (IsCheat) return _forcedTheme;
            return RemoteConfigs.GetValue(GameRemoteConfigs.GAME_THEME, GlobalConstants.DEFAULT_THEME);
        }

        private bool ResolveSequentialUnlockCondition(LevelEntry level)
        {
            var previousEntry = Data.GetPreviousEntry(level.Id);

            return Player.CheckLevelCompletion(previousEntry.Id) || Player.CheckLevelCompletion(level.Id);
        }

        private bool ResolveLevelUnlockConditionByParsing(LevelEntry level)
        {
            var condition = level.UnlockCondition;
            var tokens = condition.Split(':',StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
            {
                Log.Warn($"Unlock condition \"{condition}\" failed to parse. Will return false");
                return false;
            }

            var type = tokens[0];
            switch (type)
            {
                case GlobalConstants.UNLOCK_CONDITION_BEAT_LEVEL:
                    var levelToCheckBeat = tokens[1];
                    return Player.CheckLevelCompletion(levelToCheckBeat);
                default:
                    Log.Warn($"Unlock condition \"{condition}\" is not recognized. Will return false.");
                    return false;
            }
        }
    }
}