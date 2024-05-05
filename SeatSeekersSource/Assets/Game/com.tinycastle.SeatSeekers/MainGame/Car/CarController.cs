using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public partial class CarController : UnityComp
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
        [SerializeField] private CompWrapper<SpriteRenderer>[] _gridBackRenderers;
        
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
        
        private LevelData _currentLevelData;
        private bool _expanded = false;
        private LevelData _expandedLevelData;
        private Dictionary<int, IOccupyable> _occupyables;

        private HashSet<Obstacle> _spawnedObstacles;
        private HashSet<SeatController> _spawnedSeats;

        public LevelData CurrentLevelData
        {
            get => _expanded ? _expandedLevelData : _currentLevelData;
        }

        public bool AllowPickUpSeat => MainGame.AllowPickUpSeat && _expandTween == null;

        private IProgress _initializeProgress;
        public override IProgress InitializationProgress => _initializeProgress ?? new ImmediateProgress(false, 1f);
        
        public override IProgress Initialize()
        {
            _seatPool = new HashSet<SeatController>(_seatHost.GameObject.GetDirectOrderedChildComponents<SeatController>());
            _obstaclePool = new HashSet<Obstacle>(_obstacleHost.GameObject.GetDirectOrderedChildComponents<Obstacle>());

            foreach (var obstacle in _obstaclePool)
            {
                obstacle.Car = this;
                obstacle.SetGOActive(false);
            }
            
            foreach (var seat in _seatPool)
            {
                seat.Car = this;
                seat.SetGOActive(false);
            }

            _spawnedSeats = new HashSet<SeatController>();
            _spawnedObstacles = new HashSet<Obstacle>();

            _occupyables = new Dictionary<int, IOccupyable>();
            
            return base.Initialize();
        }
        
        public void SetLevelData(LevelData data, Theme theme)
        {
            _currentLevelData = data;
            _expanded = false;
            _expandedLevelData = null;
            
            RefreshGridAppearance(theme);
            PopulateSeatsAndObstacles();
            RefreshQueue();
            RefreshWalls();
        }

        public bool ExpandLevelByOneLane(float time, Action onComplete, Theme theme)
        {
            if (_expanded || _expandTween != null) return false;
            _expandedLevelData = new LevelData(_currentLevelData);
            _expandedLevelData.ExpandOneLaneLeft();
            _expanded = true;
            
            var tweenA = AnimateRefreshGridAppearance(time, theme);
            var tweenB = AnimateSeatMovePositions(time);
            _expandTween = DOTween.Sequence().Join(tweenA).Join(tweenB)
                .OnComplete(() =>
                {
                    _expandTween = null;
                    onComplete?.Invoke();
                }).Play();
            
            RefreshQueue();
            RefreshWalls();
            return true;
        }
        
        public void ClearLevel()
        {
            ClearSeatsAndObstacles();
        }
        
        public void RefreshGridAppearance(Theme theme)
        {
            _expandTween?.Kill();
            _expandTween = null;
            
            _gridRenderer.Comp.drawMode = SpriteDrawMode.Tiled;
            _gridRenderer.Comp.tileMode = SpriteTileMode.Continuous;
            _gridRenderer.Comp.size = new Vector2(CurrentLevelData.Width, CurrentLevelData.Height);
            
            // TODO: Randomize grid background
            for (var i = 0; i < _gridBackRenderers.Length; ++i)
            {
                _gridBackRenderers[i].SetGOActive(false);
            }
            
            var background = _gridBackRenderers[(int)theme].Comp;
            background.SetGOActive(true);
            
            var config = background.GetComponent<CarBackgroundConfig>();
            if (config != null)
            {
                background.size = GetBackgroundRendererSize(config);
            }
        }

        private Tween _expandTween;
        private Tween AnimateRefreshGridAppearance(float time, Theme theme)
        {
            _gridRenderer.Comp.drawMode = SpriteDrawMode.Tiled;
            _gridRenderer.Comp.tileMode = SpriteTileMode.Continuous;
            
            // TODO: Randomize grid background
            
            for (var i = 0; i < _gridBackRenderers.Length; ++i)
            {
                _gridBackRenderers[i].SetGOActive(false);
            }
            
            var background = _gridBackRenderers[(int)theme].Comp;
            background.SetGOActive(true);
            
            var config = background.GetComponent<CarBackgroundConfig>();
            
            var gridCurrSize = _gridRenderer.Comp.size;
            var backCurrSize = background.size;
            var gridTargetSize = new Vector2(CurrentLevelData.Width, CurrentLevelData.Height);
            var backTargetSize = GetBackgroundRendererSize(config);
            
            var ratio = 0f;
            return DOTween.To(() => ratio, (v) => ratio = v, 1f, time)
                .OnUpdate(() =>
                {
                    Debug.Log(ratio);
                    _gridRenderer.Comp.size = Vector2.Lerp(gridCurrSize, gridTargetSize, ratio);
                    background.size = Vector2.Lerp(backCurrSize, backTargetSize, ratio);
                });
        }

        private Tween AnimateSeatMovePositions(float time)
        {
            var newPairs = _occupyables.ToList().Select(pair =>
            {
                var index = pair.Key;
                var occupyable = pair.Value;

                var data = occupyable.Data;
                var newX = occupyable.Data.X + 1;
                var newY = occupyable.Data.Y;
                var newI = newX + newY * CurrentLevelData.Width;

                data.X = newX;
                data.Y = newY;

                occupyable.SetData(data, false);
                return (newI, occupyable);
            });
            
            _occupyables.Clear();

            var moveSequence = DOTween.Sequence();
            foreach (var (newI, occupyable) in newPairs)
            {
                _occupyables.Add(newI, occupyable);
                var x = occupyable.Data.X;
                var y = occupyable.Data.Y;
                moveSequence.Join(occupyable.Transform.DOLocalMove(GetCellPosition(x, y, false), time));
            }

            return moveSequence;
        }
        
        private Vector2 GetBackgroundRendererSize(CarBackgroundConfig config)
        {
            var wMin = config.Width3Size;
            var hMin = config.Height4Size;
            var w = (CurrentLevelData.Width - 3) * config.Expand + wMin;
            var h = (CurrentLevelData.Height - 4) * config.Expand + hMin;

            return new Vector2(w, h);
        }

        public void PopulateSeatsAndObstacles()
        {
            ClearSeatsAndObstacles();
            
            foreach (var seatData in CurrentLevelData.IterateAllSeats())
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
                
                var i = seatData.X + seatData.Y * CurrentLevelData.Width;
                var nextI = seatData.X + 1 + seatData.Y * CurrentLevelData.Width;
                _occupyables[i] = occupyable;
                if (seatData.IsDouble) _occupyables[nextI] = occupyable;
                occupyable.SetData(seatData);
            }
        }

        public void RefreshQueue()
        {
            // var x = _queueStartPos.Transform.localPosition.x;
            // var firstCellPos = GetCellPosition(CurrentLevelData.Width, 0, false);
            // var gridPos = _gridRenderer.Transform.localPosition;
            // var queueStartPos = gridPos + firstCellPos + new Vector3(_carDoorOffset, 0f, 0f);
            // queueStartPos.x = x;
            // _queueStartPos.Transform.localPosition = queueStartPos;
        }

        public void RefreshWalls()
        {
            // Set walls
            _wallLeft.Transform.localPosition = new Vector3(-(CurrentLevelData.Width + 1) / 2f, 0f, 0f);
            _wallRight.Transform.localPosition = new Vector3((CurrentLevelData.Width + 1) / 2f, 0f, 0f);
            _wallUp.Transform.localPosition = new Vector3(0f, (CurrentLevelData.Height + 1) / 2f, 0f);
            _wallDown.Transform.localPosition = new Vector3(0f, -(CurrentLevelData.Height + 1) / 2f, 0f);
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
                seat.ResetPickup();
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
            var dW = (CurrentLevelData.Width - 1) * _cellUnit / 2f;
            var dH = (CurrentLevelData.Height - 1) * _cellUnit / 2f;
            var wX = x * _cellUnit - dW;
            var wY = y * _cellUnit - dH;
            var localPos = new Vector3(wX, -wY, 0);

            if (!worldSpace) return localPos;
            
            var worldPos = _gridRenderer.Transform.TransformPoint(localPos);
            return worldPos;
        }        
        
        public Vector3 GetQueueStartPos(bool worldSpace = false)
        {
            return worldSpace ? _queueStartPos.Transform.position : _queueStartPos.Transform.localPosition;
        }
        
        public bool ResolveSeatDrop(IOccupyable occupyable, Vector3 localPosition, out int newX, out int newY)
        {
            var oldX = occupyable.Data.X;
            var oldY = occupyable.Data.Y;
            var oldI = oldX + oldY * CurrentLevelData.Width;
            var oldNextI = oldX + 1 + oldY * CurrentLevelData.Width;
            // Do not allow drop if existing, or out of bounds
            var canDrop = CheckSeatDrop(occupyable, localPosition, out _, out var x, out var y);
            var i = x + y * CurrentLevelData.Width;
            var nextI = x + 1 + y * CurrentLevelData.Width;
            
            if (!canDrop)
            {
                newX = occupyable.Data.X;
                newY = occupyable.Data.Y;
                return false;
            }
            
            // Clear old occupyables
            _occupyables.Remove(oldI);
            if (occupyable.Data.IsDouble && !IsLastCellOfRow(oldX)) _occupyables.Remove(oldNextI);
            
            _occupyables[i] = occupyable;
            if (occupyable.Data.IsDouble) _occupyables[nextI] = occupyable;

            newX = x;
            newY = y;

            return true;
        }

        public bool CheckSeatDrop(IOccupyable occupyable, Vector3 localPosition, out Vector3 snappedPosition, out int newX, out int newY)
        {
            var x = (int)(localPosition.x + CurrentLevelData.Width * _cellUnit * 0.5f);
            var y = -(int)(localPosition.y - CurrentLevelData.Height * _cellUnit * 0.5f);

            var i = x + y * CurrentLevelData.Width;
            var nextI = x + 1 + y * CurrentLevelData.Width;

            newX = x;
            newY = y;
            
            snappedPosition = GetCellPosition(x, y, false);

            return !(x < 0 || x >= CurrentLevelData.Width || y < 0 || y >= CurrentLevelData.Height || _occupyables.ContainsKey(i) ||
                   (occupyable.Data.IsDouble && (IsLastCellOfRow(x) || _occupyables.ContainsKey(nextI))));
        }

        public List<(SeatController seat, bool isCustomer2, List<Vector3> path)> GetPathfinding(params Vector3[] prependPositions)
        {
            var start = CurrentLevelData.Width - 1;

            if (_occupyables.ContainsKey(CurrentLevelData.Width - 1))
            {
                return new List<(SeatController seat, bool isCustomer2, List<Vector3> path)>();
            }
            var engine = new DijkstraPathfindEngine<int>(start, QueryMoveCost, GetPriority,
                GetNeighborsOf);

            // Run engine, add async later
            engine.Run();

            var validSeats = new List<(SeatController seat, bool isCustomer2, List<Vector3> path)>();
            foreach (var seat in _spawnedSeats)
            {
                var i1 = seat.X + seat.Y * CurrentLevelData.Width;
                
                if (engine.GetPathTo(i1, out var path))
                {
                    var positions = path!
                        .Select(si => GetCellPosition(si % CurrentLevelData.Width, si / CurrentLevelData.Width, true)).ToList();
                    
                    for (var i = prependPositions.Length - 1; i >= 0; --i)
                    {
                        positions.Add(prependPositions[i]);
                    }
                    
                    positions.Reverse();
                    validSeats.Add((seat, false, positions));
                }

                if (!seat.IsDoubleSeat) continue;
                
                var i2 = seat.X + 1+ seat.Y * CurrentLevelData.Width;
                if (engine.GetPathTo(i2, out var path2))
                {
                    var positions = path2!
                        .Select(si => GetCellPosition(si % CurrentLevelData.Width, si / CurrentLevelData.Width, true)).ToList();
                    
                    for (var i = prependPositions.Length - 1; i >= 0; --i)
                    {
                        positions.Add(prependPositions[i]);
                    }
                    
                    positions.Reverse();
                    validSeats.Add((seat, true, positions));               
                }
            }

            validSeats.Sort((a, b) => a.Item1.SortValue - b.Item1.SortValue);
            return validSeats;
        }

        public bool CheckFilledAllSeats()
        {
            foreach (var seat in _spawnedSeats)
            {
                if (!seat.FullySeated) return false;
            }

            return true;
        }

        private int QueryMoveCost(int from, int to)
        {
            // var hasOccupyableFrom = _occupyables.TryGetValue(from, out var occupyableFrom);
            // var hasOccupyableTo = _occupyables.TryGetValue(from, out var occupyableTo);
            // return int.MaxValue;

            return 1;
        }

        public IEnumerable<int> GetNeighborsOf(int index)
        {
            var x = index % CurrentLevelData.Width;
            var y = index / CurrentLevelData.Width;

            var hasCurrOccupyable = _occupyables.TryGetValue(index, out _);
            if (x < 0 || x >= CurrentLevelData.Width || y < 0 || y >= CurrentLevelData.Height || hasCurrOccupyable) yield break;

            foreach (var (dx, dy) in new[] { (-1, 0), (0, 1), (1, 0), (0, -1) })
            {
                var nx = x + dx;
                var ny = y + dy;
                if (nx < 0 || nx >= CurrentLevelData.Width || ny < 0 || ny >= CurrentLevelData.Height) continue;

                var ni = nx + ny * CurrentLevelData.Width;
                var hasOccupyable = _occupyables.TryGetValue(ni, out var occupyable);
                if (!hasOccupyable || occupyable!.CanEnterFrom(x, y))
                {
                    yield return ni;
                }
            }
        }

        public void SetJumpGuide(bool on)
        {
            foreach (var seat in _spawnedSeats)
            {
                seat.SeatInJumpMode = on;
            }
        }
        
        private bool IsLastCellOfRow(int x)
        {
            return (x + 1) % CurrentLevelData.Width == 0;
        }

        private int GetPriority(int index)
        {
            return (CurrentLevelData.Width - index % CurrentLevelData.Width) + index / CurrentLevelData.Width;
        }
    }
}