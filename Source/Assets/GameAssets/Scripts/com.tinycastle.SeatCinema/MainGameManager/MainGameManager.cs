using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using com.brg.Common.Initialization;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Data;
using com.brg.UnityCommon.Editor;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.SeatCinema
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
    
    public partial class MainGameManager : MonoManagerBase, IGMComponent
    {
        [Header("Components")] 
        [SerializeField] private CompWrapper<CarController> _car = "./Car";
        [SerializeField] private GOWrapper _customerHost = "./CustomerHost";
        
        [Header("Prefabs")] 
        [SerializeField] private GameObject _customerPrefab;
        
        [Header("Params")]
        [SerializeField] private float _queueDistance = 0.75f;

        private HashSet<Customer> _customerPool;
        private HashSet<Customer> _spawnedCustomers;
        
        private LevelEntry _levelEntry;
        private GameState _state = GameState.OUTSIDE_GAME;
        private float _timeLeft = -1f;
        private List<Customer>[] _queue;
        
        private bool _resolvingPlayerAction = false;

        private Tween? _playerMoveTween;

        public CarController Car => _car;

        public bool AllowPickUpSeat => _state == GameState.PLAYING && !_resolvingPlayerAction;

        public void LoadLevel(LevelEntry levelEntry)
        {
            if (!ValidateStateSwitch(_state, GameState.LOAD_LEVEL))
            {
                Log.Error($"Cannot load level at current state {_state}");
            }

            _levelEntry = levelEntry;
            SetState(GameState.LOAD_LEVEL);
        }

        public void StartGame()
        {
            SetState(GameState.PLAYING, true);
        }

        public void OnSeatPickedUpByPlayer()
        {
            // TODO
        }
        
        public void OnSeatDroppedByPlayer()
        {
            _resolvingPlayerAction = true;

            var queueStartPos = GetQueuePos(0);
            var validSeats = _car.Comp.GetPathfinding(queueStartPos);
            
            var moveSequence = DOTween.Sequence();
            var seatI = 0;
            var queueIndex = 0;
            for (var colorI = 0; colorI < _queue.Length; ++colorI)
            {
                var customerList = _queue[colorI];
                foreach (var customer in customerList)
                {
                    // Skip if customer is seated
                    if (customer.Seated) continue;
                    
                    // If still have seats
                    if (seatI < validSeats.Count)
                    {
                        var seat = validSeats[seatI];
                        
                        // If seat can be seated by customer
                        if (!seat.isCustomer2 ? seat.seat.CanAssign1(customer) : seat.seat.CanAssign2(customer))
                        {
                            // Assign seat
                            var sequence = customer.MoveToSeatViaPath(seat.path);
                            sequence.AppendCallback(() =>
                            {
                                if (!seat.isCustomer2) seat.seat.AssignCustomer1(customer);
                                else seat.seat.AssignCustomer2(customer);
                            });
                            moveSequence.Join(sequence);

                            ++seatI;
                        }
                        // Otherwise, is still in queue
                        else
                        {
                            var queuePos = GetQueuePos(queueIndex);
                            moveSequence.Join(customer.MoveInQueue(queuePos));
                            ++queueIndex;
                        }
                    }
                    // No more valid seat, only assign to Queue
                    else
                    {
                        var queuePos = GetQueuePos(queueIndex);
                        moveSequence.Join(customer.MoveInQueue(queuePos));
                        ++queueIndex;
                    }
                }
            }

            moveSequence.AppendCallback(() =>
            {
                _resolvingPlayerAction = false;
                _playerMoveTween = null;
                CheckForEndgameCondition();
            });

            _playerMoveTween = moveSequence.Play();
        }

        private void CheckForEndgameCondition()
        {
            var fullySeated = Car.CheckFilledAllSeats();
            if (fullySeated)
            {
                SetState(GameState.ENDING_GAME);
            }
        }

        private void SetState(GameState newState, bool validate = false)
        {
            if (validate && !ValidateStateSwitch(_state, newState))
            {
                Log.Error($"Cannot enter state \"{newState}\" at current state \"{_state}\"");
                return;
            }

            _state = newState;
            Log.Info($"State changed to ${_state}");
            
            switch (newState)
            {
                case GameState.OUTSIDE_GAME:
                    
                    break;
                case GameState.LOAD_LEVEL:
                    PerformLoadLevel();
                    break;
                case GameState.PLAYING:
                    
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
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
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
            var data = GEditor.Instance.GetLevelInput(_levelEntry.Id);
            _car.Comp.SetLevelData(data);
            
            // Set customer queue
            var customers = _car.Comp.Data.GetQueuers();

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
            _car.Comp.ClearLevel();
            
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
    }
}