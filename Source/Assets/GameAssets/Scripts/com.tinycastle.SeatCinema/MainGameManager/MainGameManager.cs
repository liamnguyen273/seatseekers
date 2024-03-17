using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using com.brg.Common.Initialization;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Data;
using com.brg.UnityCommon.Editor;
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

        public CarController Car => _car;

        public bool AllowPickUpSeat => !_resolvingPlayerAction;

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
            var validSeats = _car.Comp.GetPathfinding();
            
            
        }

        private void SetState(GameState newState, bool validate = false)
        {
            if (validate && !ValidateStateSwitch(_state, newState))
            {
                Log.Error($"Cannot enter state \"{newState}\" at current state \"{_state}\"");
                return;
            }

            _state = newState;
            
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
                GameState.LOAD_LEVEL => oldState >= GameState.OUTSIDE_GAME && oldState < GameState.ENDING_GAME,
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
            var queuePos = _car.Comp.GetQueueStartPos(true);
            var customers = _car.Comp.Data.GetQueuers();

            _queue = new List<Customer>[customers.Length];
            for (var i =  customers.Length - 1; i >= 0; --i)
            {
                _queue[i] = new List<Customer>();

                var count = customers[i];

                for (var j = 0; j < count; ++j)
                {
                    var customer = GetCustomer();
                    customer.SetGOActive(true);
                    customer.Color = (int)SeatEnum.BLUE + i;
                    customer.transform.position = queuePos;
                    queuePos.z -= _queueDistance;
                    
                    _queue[i].Add(customer);
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
    }
}