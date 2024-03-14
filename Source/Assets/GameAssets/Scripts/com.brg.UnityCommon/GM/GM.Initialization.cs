using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common.Initialization;
using com.brg.Common.ProgressItem;
using com.brg.UnityCommon.AnalyticsEvents;
using JSAM;
using UnityEngine;

namespace com.brg.UnityCommon
{
    public partial class GM
    {
        private bool _primaryLock = false;
        private bool _secondaryLock = false;
        private ProgressItemGroup _groupPrimary;
        private ProgressItemGroup _groupSecondary;
        private ProgressItemGroup _joinProgress;
        
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;

        protected override IProgressItem MakeProgressItem()
        {
            if (_joinProgress == null)
            {
                _groupPrimary = new ProgressItemGroup(_primaryManagers.Select(x => x.GetInitializeProgressItem()));
                _groupSecondary = new ProgressItemGroup(_secondaryManagers.Select(x => x.GetInitializeProgressItem()));
                _joinProgress = new ProgressItemGroup(_groupPrimary, _groupSecondary);
            }

            return _joinProgress;
        }
        
        protected override void StartInitializationBehaviour()
        {
            FindGMComponents();
            
            _primaryManagers = new List<IInitializable>()
            {
                Data,
                Player,
            };

            _secondaryManagers = new List<IInitializable>()
            {
                Events,
                RemoteConfigs,
                Purchases,
                Ad,
                Facebook,
                Popups,
            };
            
            _primaryLock = false;
            _secondaryLock = false;
            InitializePrimary();

            MakeProgressItem();            
            
            _loadingScreen.RequestLoad(GetInitializeProgressItem(), () =>
            {
                
            });
        }
        
        private void FindGMComponents()
        {
            var components = new List<MonoBehaviour>();
            foreach (var component in gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
            {
                components.Add(component);
            }
            
            foreach (var host in _additionalCompHosts)
            {
                foreach (var component in host.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
                {
                    components.Add(component);
                }
            }

            _components = new Dictionary<Type, IGMComponent>();
            foreach (var component in components)
            {
                if (component is IGMComponent castedComponent)
                {
                    var type = component.GetType();

                    if (!_components.TryAdd(type, castedComponent))
                    {
                        Log.Warn($"GM has duplication of GMComponent {type}, please check your hierarchy." +
                                 $"The duplicate component will be ignored and cannot be accessed through GM");
                    }
                    
                    castedComponent.OnFoundByGM();
                }
            }
        }

        private void UpdateInitialization(float dt)
        {
            if (State != InitializationState.INITIALIZING) return;

            if (!_primaryLock && _groupPrimary.Completed)
            {
                _primaryLock = true;
                InitializeSecondary();
            }

            if (!_secondaryLock && _groupSecondary.Completed)
            {
                _secondaryLock = true;
                EndInitialize(_joinProgress.IsSuccess);
            }
        }

        protected override void EndInitializationBehaviour()
        {
            // TODO
            Events.MakeEvent(GameEvents.OPEN_APP)
                .SendEvent();

            var pref = Player.GetPreference();

            AudioManager.MusicVolume = pref.MusicVolume > 0 ? 0.5f : 0f;
            AudioManager.SoundVolume = pref.SfxVolume > 0 ? 0.5f : 0f;

            AttachEvents();
            
            // Theming
            SetTheme(GetTheme());

            // Now initialize banner ad
            Ad.InitializeBannerAds();
            
            // Initialize leaderboard
            if (Player.GetLeaderboardShouldInitialize())
            {
                var names = Data.GetLeaderboardNames().OrderBy(x => Rng.GetFloat(-1.0f, 1.0f));
                Player.UpdateLeaderboard("You", 0, true);

                var score = 20;
                var scoreIncrease = 12;
                var scoreIntensifier = 5;
                foreach (var name in names)
                {
                    Player.UpdateLeaderboard(name, score, true);
                    scoreIncrease += Rng.GetInteger(scoreIntensifier + 1);
                    score += scoreIncrease;
                }
                
                Player.RequestSaveData(true, false);
            }
            else
            {
                UpdateLeaderboardAfterLaunch();
            }
        }

        private void InitializePrimary()
        {
            _primaryManagers.ForEach(x => x.Initialize());
        }

        private void InitializeSecondary()
        {
            Purchases.SetComponents(Data, Player);
            
            _secondaryManagers.ForEach(x => x.Initialize());
        }
    }
}