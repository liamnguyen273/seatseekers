using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common.Initialization;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.SeatCinema
{
    public partial class CarController : MonoBehaviour
    {
        [Header("Params")] 
        [SerializeField] private float _cellUnit = 1f;
        [SerializeField] private float _carDoorOffset = 0.5f;

        [Header("Components")] 
        [SerializeField] private GOWrapper _seatHost = "./SeatPoolHost";
        [SerializeField] private GOWrapper _obstacleHost = "./ObstaclePoolHost";
        
        [Header("Grid")]
        [SerializeField] private CompWrapper<SpriteRenderer> _gridRenderer = "./Grid";
        [SerializeField] private GOWrapper _queueStartPos = "./QueueStart";
        
        [Header("Walls")] 
        [SerializeField] private GOWrapper _wallLeft = "./Grid/WallLeft";
        [SerializeField] private GOWrapper _wallRight = "./Grid/WallRight";
        [SerializeField] private GOWrapper _wallUp = "./Grid/WallUp";
        [SerializeField] private GOWrapper _wallDown = "./Grid/WallDown";

        [Header("Prefabs")] 
        [SerializeField] private GameObject _singleSeatPrefab;
        [SerializeField] private GameObject _doubleSeatPrefab;
        [SerializeField] private GameObject _obstaclePrefab;

        private HashSet<Obstacle> _obstaclePool;
        private HashSet<SeatController> _seatPool;
        
        public MainGameManager MainGame { get; set; }
        
        private LevelData _levelData;
        private Dictionary<int, IOccupyable> _occupyables;
        private bool _seatLock = false;

        private HashSet<Obstacle> _spawnedObstacles;
        private HashSet<SeatController> _spawnedSeats;

        public LevelData Data => _levelData;
        
        public bool SeatLock
        {
            get => _seatLock;
            set => _seatLock = value;
        }
        
        public void SetLevelData(string levelInput)
        {
            SetLevelData(LevelData.Make(levelInput));
        }
        
        public void SetLevelData(LevelData data)
        {
            _levelData = data;
            RefreshGridAppearance();
            PopulateSeatsAndObstacles();
            RefreshQueue();
            RefreshWalls();
        }
        
        public void ClearLevel()
        {
            ClearSeatsAndObstacles();
        }
        
        public void RefreshGridAppearance()
        {
            _gridRenderer.Comp.drawMode = SpriteDrawMode.Tiled;
            _gridRenderer.Comp.tileMode = SpriteTileMode.Continuous;
            _gridRenderer.Comp.size = new Vector2(_levelData.Width, _levelData.Height);
        }

        public void PopulateSeatsAndObstacles()
        {
            ClearSeatsAndObstacles();
            
            foreach (var seatData in _levelData.IterateAllSeats())
            {
                if (seatData.Value == (int)SeatEnum.NONE) continue;

                IOccupyable occupyable;
                if (seatData.Value > (int)SeatEnum.BROWN)
                {
                    var obstacle = GetObstacle();
                    obstacle.SetGOActive(true);
                    
                    _spawnedObstacles.Add(obstacle);
                    occupyable = obstacle;
                }
                else
                {
                    var seat = GetSeat();
                    seat.SetGOActive(true);
                    
                    _spawnedSeats.Add(seat);
                    occupyable = seat;
                }
                
                var i = seatData.X + seatData.Y * _levelData.Width;
                var nextI = seatData.X + 1 + seatData.Y * _levelData.Width;
                _occupyables[i] = occupyable;
                if (seatData.IsDouble) _occupyables[nextI] = occupyable;
                occupyable.SetData(seatData);
            }
        }

        public void RefreshQueue()
        {
            var firstCellPos = GetCellPosition(_levelData.Width, 0, false);
            var gridPos = _gridRenderer.Transform.localPosition;
            var queueStartPos = gridPos + firstCellPos + new Vector3(_carDoorOffset, 0f, 0f);
            _queueStartPos.Transform.localPosition = queueStartPos;
        }

        public void RefreshWalls()
        {
            // Set walls
            _wallLeft.Transform.localPosition = new Vector3(-(_levelData.Width + 1) / 2f, 0f, 0f);
            _wallRight.Transform.localPosition = new Vector3((_levelData.Width + 1) / 2f, 0f, 0f);
            _wallUp.Transform.localPosition = new Vector3(0f, (_levelData.Height + 1) / 2f, 0f);
            _wallDown.Transform.localPosition = new Vector3(0f, -(_levelData.Height + 1) / 2f, 0f);
        }

        public void ResetSeatsAndObstacles()
        {
            PopulateSeatsAndObstacles();
        }

        public void ClearSeatsAndObstacles()
        {
            foreach (var seat in _spawnedSeats.ToList())
            {
                seat.RemoveCustomers();
                ReturnSeat(seat);
            }

            foreach (var obstacle in _spawnedObstacles.ToList())
            {
                ReturnObstacle(obstacle);
            }

            _occupyables.Clear();
        }
        
        public Vector3 GetCellPosition(int x, int y, bool worldSpace = false)
        {
            var worldPosOrigin = _gridRenderer.Transform.position;
            var dW = (_levelData.Width - 1) * _cellUnit / 2f;
            var dH = (_levelData.Height - 1) * _cellUnit / 2f;
            var wX = x * _cellUnit - dW;
            var wY = y * _cellUnit - dH;
            var localPos = new Vector3(wX, -wY, 0);
            return worldSpace ? worldPosOrigin + localPos : localPos;
        }        
        
        public Vector3 GetQueueStartPos(bool worldSpace = false)
        {
            return worldSpace ? _queueStartPos.Transform.position : _queueStartPos.Transform.localPosition;
        }
        
        public bool ResolveSeatDrop(IOccupyable occupyable, Vector3 localPosition, out int newX, out int newY)
        {
            var oldX = occupyable.Data.X;
            var oldY = occupyable.Data.Y;
            var oldI = oldX + oldY * _levelData.Width;
            var oldNextI = oldX + 1 + oldY * _levelData.Width;
            // Do not allow drop if existing, or out of bounds
            var canDrop = CheckSeatDrop(occupyable, localPosition, out _, out var x, out var y);
            var i = x + y * _levelData.Width;
            var nextI = x + 1 + y * _levelData.Width;
            
            if (!canDrop)
            {
                newX = occupyable.Data.X;
                newY = occupyable.Data.Y;
                return false;
            }
            
            // Clear old occupyables
            _occupyables.Remove(oldI);
            if (!IsLastCellOfRow(oldX) && occupyable.Data.IsDouble) _occupyables.Remove(oldNextI);
            
            _occupyables[i] = occupyable;
            if (occupyable.Data.IsDouble) _occupyables[nextI] = occupyable;

            newX = x;
            newY = y;
            
            // Perform pathfinding to get available seat.
            
            return true;
        }

        public bool CheckSeatDrop(IOccupyable occupyable, Vector3 localPosition, out Vector3 snappedPosition, out int newX, out int newY)
        {
            var x = (int)(localPosition.x + _levelData.Width * _cellUnit * 0.5f);
            var y = -(int)(localPosition.y - _levelData.Height * _cellUnit * 0.5f);

            var i = x + y * _levelData.Width;
            var nextI = x + 1 + y * _levelData.Width;

            newX = x;
            newY = y;
            
            snappedPosition = GetCellPosition(x, y, false);
            
            MainGame.OnSeatDroppedByPlayer();

            return !(x < 0 || x >= _levelData.Width || y < 0 || y >= _levelData.Height || _occupyables.ContainsKey(i) ||
                   (occupyable.Data.IsDouble && (IsLastCellOfRow(x) || _occupyables.ContainsKey(nextI))));
        }

        public Dictionary<int, List<(SeatController seat, List<Vector3> path)>> GetPathfinding()
        {
            var start = _levelData.Width - 1;
            var engine = new DijkstraPathfindEngine<int>(start, QueryMoveCost, DistanceComparer.GetInstance(this),
                GetNeighborsOf);

            // Run engine, add async later
            engine.Run();

            var validSeats = new Dictionary<int, List<(SeatController seat, List<Vector3> path)>>();
            foreach (var seat in _spawnedSeats)
            {
                var i = seat.X + seat.Y * _levelData.Width;
                var color = seat.SeatColor;
                if (engine.GetPathTo(i, out var path))
                {
                    if (!validSeats.ContainsKey(color))
                    {
                        validSeats[color] = new List<(SeatController seat, List<Vector3> path)>();
                    }
                    
                    validSeats[color].Add((seat, path!.Select(si => GetCellPosition(si % _levelData.Width, si / _levelData.Width, true)).ToList()));
                }
            }

            return validSeats;
        }

        private int QueryMoveCost(int from, int to)
        {
            return 1;
        }

        private IEnumerable<int> GetNeighborsOf(int index)
        {
            var x = index % _levelData.Width;
            var y = index / _levelData.Width;

            if (x < 0 || x >= _levelData.Width || y < 0 || y >= _levelData.Height) yield break;

            foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
            {
                var nx = x + dx;
                var ny = y + dy;
                if (nx < 0 || nx >= _levelData.Width || ny < 0 || ny >= _levelData.Height) continue;

                var ni = nx + ny * _levelData.Width;
                if (_occupyables.TryGetValue(ni, out var occupyable) && occupyable.CanEnterFrom(x, y))
                {
                    yield return ni;
                }
            }
        }
        
        private bool IsLastCellOfRow(int x)
        {
            return (x + 1) % _levelData.Width == 0;
        }

        private class DistanceComparer : IComparer<int>
        {
            private static DistanceComparer? _instance;
            public static DistanceComparer GetInstance(CarController car)
            {
                if (_instance == null)
                {
                    _instance = new DistanceComparer(car);
                }
                else
                {
                    _instance._car = car;
                }

                return _instance;
            }
            
            private CarController _car;
            private DistanceComparer(CarController car)
            {
                _car = car;
            }
            
            public int Compare(int a, int b)
            {
                var xa = a % _car._levelData.Width;
                var ya = a / _car._levelData.Width; 
                var xb = b % _car._levelData.Width;
                var yb = b / _car._levelData.Width;
                return (xa + ya) - (xb + yb);
            }
        }
    }
}