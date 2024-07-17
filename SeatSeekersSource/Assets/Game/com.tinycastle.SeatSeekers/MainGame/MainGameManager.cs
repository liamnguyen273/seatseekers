using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using DG.Tweening;
using JSAM;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.tinycastle.SeatSeekers
{
    public enum GameState
    {
        OUTSIDE_GAME = 0,
        LOAD_LEVEL,
        PLAYING,
        PAUSED,
        ENDING_GAME,
        END_GAME,
        EXIT
    }

    public enum Theme
    {
        CAR_1 = 0,
        CAR_2 = 1,
        CAR_3 = 2,
        CAR_4 = 3,
        STAGE_1 = 4,
        STAGE_2 = 5,
        STAGE_3 = 6,
    }
    
    public partial class MainGameManager : UnityComp
    {
        [Header("Components")] 
        [SerializeField] private CompWrapper<CarController> _car = "./Car";
        [SerializeField] private GOWrapper _customerHost = "./CustomerHost";
        [SerializeField] private CompWrapper<TextLocalizer> _levelText = "UICanvas/TopGroup/LevelText";
        [SerializeField] private CompWrapper<TextLocalizer> _timeText = "UICanvas/TopGroup/TimeText";
        [SerializeField] private CompWrapper<MainGameHud> _mainHud = "UIGroups/BackCanvas/MainGameHud";
        [SerializeField] private CompWrapper<CompPlayable> _freezePlayable;
        [SerializeField] private CompWrapper<FadeInOutPlayable> _freezePlayable2;

        [SerializeField] private CompWrapper<MultiplayerAddon> _multiplayerAddon = "./";
        
        [Header("Prefabs")] 
        [SerializeField] private GameObject _customerPrefab;

        [Header("Themes")] 
        [SerializeField] private GOWrapper[] _backgrounds;
        
        [Header("Params")]
        [SerializeField] private float _queueDistance = 0.75f;

        private HashSet<Customer> _customerPool;
        private HashSet<Customer> _spawnedCustomers;
        
        private LevelEntry _levelEntry;
        private GameState _state = GameState.OUTSIDE_GAME;
        private List<Customer>[] _queue;
        private float _timeLeft = -1;
        private bool _usedTimeoutChance = false;
        private bool _isWin = false;

        private float _freezeTimer = -1f;
        private Tween _freezeTween;
        private bool _seatInJumpMode = false;
        
        private bool _resolvingPlayerAction = false;

        private Tween _playerMoveTween;

        public float TimeLeft
        {
            get => _timeLeft;
            set
            {
                _timeLeft = value;

                var m = (int)_timeLeft / 60;
                var s = (int)Math.Ceiling(_timeLeft - m * 60f);
                _timeText.Comp.Text = $"{m:00}:{s:00}";
            }
        }

        public CarController Car => _car;

        public bool AllowPickUpSeat => _state == GameState.PLAYING;

        private IProgress _initializeProgress;
        public IProgress InitializationProgress => _initializeProgress ?? new ImmediateProgress(false, 1f);

        public float FreezeTimer
        {
            get => _freezeTimer;
            set
            {
                var oldTime = _freezeTimer;
                _freezeTimer = value;
                if (oldTime <= 0f && value > 0f)
                {
                    // Toggle on
                    _freezePlayable.NullableComp?.Play(null);
                    _freezePlayable2.NullableComp?.PlayIn(null);
                }
                else if (oldTime > 0f && value <= 0f)
                {
                    _freezePlayable.NullableComp?.Kill();
                    _freezePlayable2.NullableComp?.PlayOut(null);
                    _mainHud.Comp.GetBoosterButton(Constants.BOOSTER_FREEZE_RESOURCE).ToggleOff();
                }
            }
        }

        public bool SeatInJumpMode
        {
            get => _seatInJumpMode;
            set
            {
                _car.Comp.SetJumpGuide(value);
                _seatInJumpMode = value;
            }
        }

        public MultiplayerAddon MultiplayerAddon => _multiplayerAddon;

        public bool ResolvingPlayerAction
        {
            get => _resolvingPlayerAction;
            private set => _resolvingPlayerAction = value;
        }

        public override IProgress Initialize()
        {
            _car.Comp.MainGame = this;
            _car.Comp.Initialize();

            _customerPool = new(_customerHost.GameObject.GetDirectOrderedChildComponents<Customer>());
            foreach (var customer in _customerPool)
            {
                customer.SetGOActive(false);
            }
            
            _mainHud.Comp.SetGOActive(false);
            _seatInJumpMode = false;

            MultiplayerAddon.Initialize();

            _spawnedCustomers = new();
            _initializeProgress = new ImmediateProgress(true, 1f);
            return _initializeProgress;
        }

        public override IProgress Activate()
        {
            AudioManager.StopMusic(AudioLibraryMusic.music_game, transform);
            AudioManager.StopMusic(AudioLibraryMusic.BPMusic1, transform);
            AudioManager.StopMusic(AudioLibraryMusic.BPMusic2, transform);
            AudioManager.StopMusic(AudioLibraryMusic.BPMusic3, transform);
            AudioManager.StopMusic(AudioLibraryMusic.BPMusic4, transform);
            AudioManager.StopMusic(AudioLibraryMusic.music_mainmenu);
            _mainHud.SetGOActive(true);
            _freezePlayable2.Comp.PlayOut(null);
            _freezePlayable2.Comp.Complete();
            return base.Activate();
        }

        public override void Pause()
        {
            MultiplayerAddon.Pause();
            base.Pause();
        }

        public override void Resume()
        {
            MultiplayerAddon.Resume();
            base.Resume();
        }

        public override IProgress Deactivate()
        {
            SetState(GameState.EXIT, false);
            SetState(GameState.OUTSIDE_GAME, false);
            _mainHud.SetGOActive(false);
            MultiplayerAddon.Deactivate();
            return base.Deactivate();
        }

        public void LoadLevel(LevelEntry levelEntry)
        {
            if (!ValidateStateSwitch(_state, GameState.LOAD_LEVEL))
            {
                LogObj.Default.Error("MainGameManager", $"Cannot load level at current state {_state}");
            }

            _levelEntry = levelEntry;
            _customLevelData = null;
            SetState(GameState.LOAD_LEVEL);
        }

        public void LoadAppropriateMusic()
        {
            var rand = Random.Range(0, 4);
            switch (CurrentTheme)
            {
                case Theme.CAR_1 or Theme.CAR_2 or Theme.CAR_3 or Theme.CAR_4:
                    AudioManager.PlayMusic(AudioLibraryMusic.music_game, transform);
                    break;
                case Theme.STAGE_1 or Theme.STAGE_2 or Theme.STAGE_3 when rand == 0: 
                    AudioManager.PlayMusic(AudioLibraryMusic.BPMusic1, transform);
                    break;
                case Theme.STAGE_1 or Theme.STAGE_2 or Theme.STAGE_3 when rand == 1: 
                    AudioManager.PlayMusic(AudioLibraryMusic.BPMusic2, transform);
                    break;
                case Theme.STAGE_1 or Theme.STAGE_2 or Theme.STAGE_3 when rand == 2: 
                    AudioManager.PlayMusic(AudioLibraryMusic.BPMusic3, transform);
                    break;
                case Theme.STAGE_1 or Theme.STAGE_2 or Theme.STAGE_3 when rand == 3: 
                    AudioManager.PlayMusic(AudioLibraryMusic.BPMusic4, transform);
                    break;
            }
        }

        private LevelData _customLevelData;

        public void LoadCustomLevel(LevelEntry levelEntry, LevelData levelData)
        {
            if (!ValidateStateSwitch(_state, GameState.LOAD_LEVEL))
            {
                LogObj.Default.Error("MainGameManager", $"Cannot load level at current state {_state}");
            }

            _levelEntry = levelEntry;
            _customLevelData = levelData;
            SetState(GameState.LOAD_LEVEL);
        }

        public void StartGame()
        {
            SetState(GameState.PLAYING, true);
        }

        public void StartGameMultiplayer(int intensity, string enemyName)
        {
            MultiplayerAddon.Activate();
            MultiplayerAddon.StartGame(intensity, enemyName, _queue, () =>
            {
                SetState(GameState.PLAYING, true);
            });
        }

        public void RestartGame()
        {
            SetState(GameState.EXIT, false);
            SetState(GameState.OUTSIDE_GAME, false);
            SetState(GameState.LOAD_LEVEL, true);
        }

        public void RestartLevelWithValidation()
        {
            if (_levelEntry is null) return;
            SetState(GameState.EXIT, false);
            SetState(GameState.OUTSIDE_GAME, false);
            var hasAdButtonTimer = false;
            GM.Instance.RequestPlayLevelWithValidation(_levelEntry, ref hasAdButtonTimer);
        }

        public bool HasNextLevel(out LevelEntry entry)
        {
            entry = null;
            if (_levelEntry is null) return false;
            return GM.Instance.HasNextLevel(_levelEntry, out entry);
        }

        public void ForceClearLevel()
        {
            SetState(GameState.EXIT, false);
            SetState(GameState.OUTSIDE_GAME, false);
        }
        
        public void StartNextLevel()
        {
            SetState(GameState.EXIT, false);
            SetState(GameState.OUTSIDE_GAME, false);
            var entry = GM.Instance.Get<GameDataManager>().GetNextLevelEntry(_levelEntry.Id);
            
            if (entry != null)
            {
                LoadLevel(entry);
            }
            
            SetState(GameState.PLAYING);
        }
        
        public void StartPreviousLevel()
        {
            SetState(GameState.EXIT, false);
            SetState(GameState.OUTSIDE_GAME, false);
            var entry = GM.Instance.Get<GameDataManager>().GetPrevLevelEntry(_levelEntry.Id);
            
            if (entry != null)
            {
                LoadLevel(entry);
            }
            SetState(GameState.PLAYING);
        }
        
        public void OnSeatDroppedByPlayer(bool blockSeating = false)
        {
            ResolvingPlayerAction = true;
            var moveSequence = GetMoveCustomerSequenceToSeats(blockSeating)
                .AppendCallback(() =>
                {
                    ResolvingPlayerAction = false;
                    _playerMoveTween = null;
                    CheckForEndgameCondition();
                });
            
            _playerMoveTween = moveSequence.Play();
        }
        
        public void MultiplayerSetEnemyEndGame()
        {
            _isWin = false;
            SetState(GameState.ENDING_GAME);    
        }

        private Sequence GetMoveCustomerSequenceToSeats(bool blockSeating = false)
        {
            var queueStartPos = GetQueuePos(0);
            var validSeats = blockSeating 
                ? new() 
                : _car.Comp.GetPathfinding(queueStartPos);
            
            var moveSequence = DOTween.Sequence();
            var queueIndex = 0;
            var seatingTerminated = false;
            for (var colorI = _queue.Length - 1; colorI >= 0 ; --colorI)
            {
                var customerList = _queue[colorI];
                foreach (var customer in customerList)
                {
                    // Skip if customer is seated
                    if (customer.Seated) continue;
                    
                    // Only find seat if has valid seats and not terminated
                    if (!seatingTerminated && validSeats.Count > 0)
                    {
                        // Check each seat for seat-ability
                        var seatedSeatIndex = -1;
                        for (var i = 0; i < validSeats.Count; ++i)
                        {
                            var seat = validSeats[i];
                            // If seat can be seated by customer
                            if (!seat.isCustomer2 ? seat.seat.CanAssign1(customer) : seat.seat.CanAssign2(customer))
                            {
                                // Assign seat
                                var sequence = customer.MoveToSeatViaPath(seat.path);
                                seat.seat.Locked = true;
                                sequence.AppendCallback(() =>
                                {
                                    if (!seat.isCustomer2) seat.seat.AssignCustomer1(customer);
                                    else seat.seat.AssignCustomer2(customer);
                                    seat.seat.Locked = false;
                                });
                                moveSequence.Join(sequence);

                                seatedSeatIndex = i;
                                break;
                            }
                        }

                        if (seatedSeatIndex >= 0)
                        {
                            validSeats.RemoveAt(seatedSeatIndex);
                            continue;
                        }
                        else
                        {
                            // Cannot find seat => terminate seating
                            seatingTerminated = true;
                        }
                    }
                    
                    // No more valid seat or cut off seating, only assign to Queue
                    var queuePos = GetQueuePos(queueIndex);
                    var tween = customer.MoveInQueue(queuePos);
                    if (tween != null) moveSequence.Join(tween);
                    ++queueIndex;
                }
            }

            if (MultiplayerAddon.Activated)
            {
                moveSequence.AppendCallback(() =>
                {
                    MultiplayerAddon.OnPlayerMove(_queue);
                });
            }
            
            return moveSequence;
        }

        private void CheckForEndgameCondition()
        {
            var fullySeated = Car.CheckFilledAllSeats();
            if (fullySeated)
            {
                _isWin = true;
                SetState(GameState.ENDING_GAME);
            }
        }

        private void SetState(GameState newState, bool validate = false)
        {
            if (validate && !ValidateStateSwitch(_state, newState))
            {
                LogObj.Default.Error("MainGameManager", $"Cannot enter state \"{newState}\" at current state \"{_state}\"");
                return;
            }

            _state = newState;
            LogObj.Default.Info("MainGameManager",$"State changed to ${_state}");
            var popupManager = GM.Instance.Get<PopupManager>();
            
            switch (newState)
            {
                case GameState.OUTSIDE_GAME:
                    
                    break;
                case GameState.LOAD_LEVEL:
                    PerformLoadLevel();
                    _levelText.Comp.Text = _levelEntry?.DisplayName ?? "";
                    _usedTimeoutChance = false;
                    TimeLeft = 60 * 5f;
                    _isWin = false;
                    break;
                case GameState.PLAYING:
                    _mainHud.Comp.OnLevel(_levelEntry!.SortOrder);
                    break;
                case GameState.PAUSED:
                    
                    break;
                case GameState.ENDING_GAME:
                    if (MultiplayerAddon.Activated)
                    {
                        // TODO: ANIM
                        DOVirtual.DelayedCall(1f, () => SetState(GameState.END_GAME)).Play();
                        return;
                    }
                    
                    if (_isWin)
                    {
                        // TODO: ANIM
                        DOVirtual.DelayedCall(1f, () => SetState(GameState.END_GAME)).Play();
                    }
                    else
                    {
                        // Timeout
                        if (_usedTimeoutChance)
                        {
                            var popup = popupManager.GetPopup<PopupTimeoutBehaviour>();
                            popup.Show();
                        }
                        else
                        {
                            SetState(GameState.END_GAME);
                        }
                    }
                    break;
                case GameState.END_GAME:

                    if (MultiplayerAddon.Activated)
                    {
                        var popup = popupManager.GetPopup<PopupMultiplayerEnd>(out var behaviour);
                        behaviour.Setup(_isWin, MultiplayerAddon.EnemyName, MultiplayerAddon.Intensity);
                        popup.Show();
                        return;
                    }
                    
                    CheckAndShowConcludePopups(popupManager);
                    break;
                case GameState.EXIT:
                    ClearLevel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        private int _adDelay = 1;
        private void CheckAndShowConcludePopups(PopupManager popupManager)
        {
            Action next = () =>
            {
                if (_isWin)
                {
                    var popup = popupManager.GetPopup<PopupWinBehaviour>(out var behaviour);
                    behaviour.SetCoin(_levelEntry, CountCustomers() * 5);
                    popup.Show();
                }
                else
                {
                    var popup = popupManager.GetPopup<PopupLostBehaviour>(out var behaviour);
                    popup.Show();
                }
            };
            
            if ((_levelEntry?.SortOrder ?? 99) <= 7)
            {
                next();
                return;
            }
            
            _adDelay -= 1;

            if (_adDelay <= 0)
            {
                GM.Instance.Get<UnityAdManager>().RequestAd(new AdRequest(AdRequestType.INTERSTITIAL_AD, () =>
                {
                    _adDelay = 3;
                    next();
                    GM.Instance.Get<GameSaveManager>().ExtraData.AddInterstitialView();
                }, () =>
                {
                    _adDelay = 1;
                    next();
                }));
            }
            else
            {
                next();
            }
        }

        public void UseTimeoutChanceAndContinue()
        {
            if (_usedTimeoutChance) return;

            TimeLeft = 31f;
            _usedTimeoutChance = true;
            SetState(GameState.PLAYING, false);
        }

        public void OnRefuseTimeoutChance()
        {
            var popupManager = GM.Instance.Get<PopupManager>();
            var popup = popupManager.GetPopup<PopupLostBehaviour>();
            popup.Show();
        }

        public bool OnBoosterOn(string boosterName)
        {
            switch (boosterName)
            {
                case Constants.BOOSTER_FREEZE_RESOURCE:
                    if (FreezeTimer > 0f) return false;
                    FreezeTimer = 30f;
                    return true;
                case Constants.BOOSTER_JUMP_RESOURCE:
                    SeatInJumpMode = true;
                    return true;
                case Constants.BOOSTER_EXPAND_RESOURCE:
                    if (_playerMoveTween != null) return false;
                    var willExpand = _car.Comp.ExpandLevelByOneLane(1.5f, OnExpandComplete, CurrentTheme);
                    if (willExpand)
                    {
                        AnimateExpandCustomerMove();
                    }
                    return willExpand;
            }

            return false;
        }

        public bool OnBoosterOff(string boosterName)
        {
            switch (boosterName)
            {
                case Constants.BOOSTER_FREEZE_RESOURCE:
                    return FreezeTimer <= 0f;
                case Constants.BOOSTER_JUMP_RESOURCE:
                    SeatInJumpMode = false;
                    return true;
                case Constants.BOOSTER_EXPAND_RESOURCE:
                    return false;
            }

            return false;
        }

        public void OnSeatWantCustomerToJumpTo(SeatController seat)
        {
            Tween jumpTween = null;
            foreach (var customers in _queue)
            {
                foreach (var customer in customers.Where(customer => !customer.Seated))
                {
                    if (seat.CanAssign1(customer))
                    {
                        seat.Locked = true;
                        jumpTween = AnimateJumpToSeat(customer, seat);
                        break;
                    }
                    else if (seat.CanAssign2(customer))
                    {
                        seat.Locked = true;
                        jumpTween = AnimateJumpToSeat(customer, seat);
                        break;
                    }
                }

                if (jumpTween != null) break;
            }

            if (jumpTween != null)
            {
                var moveSequence = GetMoveCustomerSequenceToSeats(true);
                _playerMoveTween = moveSequence.Join(jumpTween)
                    .AppendCallback(() =>
                    {
                        seat.Locked = false;
                        ResolvingPlayerAction = false;
                        _playerMoveTween = null;
                        CheckForEndgameCondition();
                    }).Play();
            }
            
            _mainHud.Comp.GetBoosterButton(Constants.BOOSTER_JUMP_RESOURCE)
                ?.ToggleOff();
        }

        private Tween AnimateJumpToSeat(Customer customer, SeatController seat)
        {
            if (seat.CanAssign1(customer)) seat.AssignCustomer1(customer, false);
            else if (seat.CanAssign2(customer)) seat.AssignCustomer2(customer, false);
            var moveTween = customer.MoveToSeatViaPath(new List<Vector3>() { seat.Transform.position }, true);
            return moveTween;
        }

        private void OnExpandComplete()
        {
            
        }

        private void AnimateExpandCustomerMove()
        {
            OnSeatDroppedByPlayer(true);   
        }

        public void TransitOut()
        {
            var mainGame = GM.Instance.Get<MainGameManager>();
            var loading = GM.Instance.Get<LoadingScreen>();
            var popupManager = GM.Instance.Get<PopupManager>();

            loading.RequestLoad(new ImmediateProgress(true, 1f),
                () =>
                {
                    mainGame.Deactivate();
                    popupManager.HideAllPopups();
                    popupManager.GetPopup<PopupMainMenu>().Show();
                }, null);
        }

        private void Update()
        {
            switch (_state)
            {
                case GameState.OUTSIDE_GAME:
                    
                    break;
                case GameState.LOAD_LEVEL:
                    
                    break;
                case GameState.PLAYING:
                    if (FreezeTimer > 0f)
                    {
                        FreezeTimer -= Time.deltaTime;
                    }
                    else
                    {
                        FreezeTimer = -1f;
                        TimeLeft -= Time.deltaTime;

                        if (TimeLeft <= 0f)
                        {
                            SetState(GameState.ENDING_GAME);
                        }
                    }
                    break;
                case GameState.PAUSED:
                    
                    break;
                case GameState.ENDING_GAME:
                    
                    break;
                case GameState.END_GAME:
                    
                    break;
                case GameState.EXIT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool ValidateStateSwitch(GameState oldState, GameState newState)
        {
            return (newState) switch
            {
                GameState.OUTSIDE_GAME => true,
                GameState.LOAD_LEVEL => oldState == GameState.OUTSIDE_GAME || oldState >= GameState.END_GAME,
                GameState.PLAYING => oldState == GameState.PAUSED || oldState == GameState.LOAD_LEVEL,
                GameState.PAUSED => oldState == GameState.PLAYING,
                GameState.ENDING_GAME => oldState == GameState.PAUSED || oldState == GameState.PLAYING,
                GameState.END_GAME => oldState == GameState.ENDING_GAME,
                GameState.EXIT => true,
                _ => false
            };
        }

        private void PerformLoadLevel()
        {
            // Set car
            LevelData data;

            if (_customLevelData != null)
            {
                data = _customLevelData;
            }
            else
            {
                if (GM.Instance != null)
                {
                    var gmData = GM.Instance.Get<GameDataManager>();
                    data = gmData.GetLevelData(_levelEntry.Id);
                }
                else
                {
                    data = GEditor.Instance.GetLevelData(_levelEntry.Id);
                }
            }

            if (data == null || !data.Playable)
            {
                LogObj.Default.Error("MainGameManager", "Cannot play level.");
                SetState(GameState.OUTSIDE_GAME, false);
                return;
            }
            
            EvaluateTheme();
            
            _car.Comp.SetLevelData(data, CurrentTheme);
            
            MakeCustomerQueue();
        }

        private Theme _currentTheme;
        public Theme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                for (var i = 0; i < _backgrounds.Length; ++i)
                {
                    _backgrounds[i].SetActive(false);
                }
                _backgrounds[(int)value].SetActive(true);
            }
        }

        private void EvaluateTheme()
        {
            // Set background
            var level = _levelEntry.SortOrder;
            Theme theme;
            switch (level)
            {
                case 1 or 2:
                    theme = Theme.STAGE_1;
                    break;
                case 3 or 4:
                    theme = Theme.STAGE_2;
                    break;
                case 5:
                    theme = Theme.STAGE_3;
                    break;
                default:
                    if (level == -1)
                    {
                        level = Random.Range(0, 4);
                    }
                    var de = (level - 1) % 5;
                    if (de >= 4)
                    {
                        var de3 = level % 3;
                        theme = (Theme)(4 + de3);
                    }
                    else theme = (Theme)de;
                    break;
            }

            CurrentTheme = theme;
        }

        private void MakeCustomerQueue()
        {
            // Set customer queue
            var customers = _car.Comp.CurrentLevelData.GetQueuers();

            _queue = new List<Customer>[customers.Length];
            var queueNumber = 0;
            for (var i =  customers.Length - 1; i >= 0; --i)
            {
                _queue[i] = new List<Customer>();

                var count = customers[i];

                for (var j = 0; j < count; ++j)
                {
                    var customer = GetCustomer();
                    customer.SetGOActive(true);
                    customer.Color = (int)SeatEnum.BLUE + i;
                    var queuePos = GetQueuePos(queueNumber);
                    customer.transform.position = queuePos;
                    
                    _queue[i].Add(customer);
                    ++queueNumber;
                }
            }
        }

        private void ClearLevel()
        {
            SeatInJumpMode = false;
            
            _car.Comp.ClearLevel();
            
            _playerMoveTween?.Kill();
            _playerMoveTween = null;
            ResolvingPlayerAction = false;
            
            // Clear all customers
            foreach (var customer in _spawnedCustomers.ToList())
            {
                ReturnCustomer(customer);
            }

            _queue = Array.Empty<List<Customer>>();
        }

        private Vector3 GetQueuePos(int queueIndex)
        {
            var pos = _car.Comp.GetQueueStartPos(true);
            pos.z -= queueIndex * _queueDistance;
            return pos;
        }

        private int CountCustomers()
        {
            return _queue.SelectMany(l => l, (l, e) => e).Count();
        }
    }
}