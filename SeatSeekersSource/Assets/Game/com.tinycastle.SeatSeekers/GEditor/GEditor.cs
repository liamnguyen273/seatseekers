using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.tinycastle.SeatSeekers
{
    public partial class GEditor : UnityComp
    {
        public static GEditor Instance { get; private set; }
        
        [Header("Components")]
        [SerializeField] private CompWrapper<MainGameManager> _mainGameManager = ".";

        private LevelGen[] _allLevels;
        private int _currentLevel;
        
        public MainGameManager MainGame => _mainGameManager.Comp;

        private IProgress _initializationProgress;
        public override IProgress InitializationProgress => _initializationProgress ?? new ImmediateProgress(false, 1f);
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Initialize();
        }

        public override IProgress Initialize()
        {
            _allLevels = Utils.ReadLevelGenCsv();
            MainGame.Initialize();
            return base.Initialize();
        }

        public void SaveData()
        {
            Utils.WriteCsvs(_allLevels);
        }

        public void GenerateAllLevels()
        {
            foreach (var levelGen in _allLevels)
            {
                Debug.Log($"Generating level {levelGen.LevelName}");
                GenerateLevel(levelGen, false);
            }
            
            Utils.WriteCsvs(_allLevels);
        }

        public LevelData GetLevelData(string name)
        {
            var index = Array.FindIndex(_allLevels, x => x.LevelName == name);
            if (index == -1)
            {
                return null;
            }

            return _allLevels[index].LevelData;
        }

        private void GenerateLevel(LevelGen gen, bool overrideData = false)
        {
            if (gen.LevelData.Playable && !overrideData)
            {
                return;
            }

            var logger = new StringBuilder();

            var levelData = gen.LevelData;
            var w = gen.CarW;
            var h = gen.CarH;
            var initialized = levelData.InitializeGridToEmpty(w, h);
            var topRight = w - 1;
            var bottomLeft = 0 + (h - 1) * w;

            logger.AppendLine("Creating engine...");

            var pathFinder = new DijkstraPathfindEngine<int>(
                topRight,
                (a, b) => 1,
                (x) => Random.Range(0, 100),
                (index) =>
                {
                    var x = index % w;
                    var y = index / w;
                    var result = new List<int>();
                    foreach (var (dx, dy) in new[] { (-1, 0), (0, 1), (1, 0), (0, -1) })
                    {
                        var nx = x + dx;
                        var ny = y + dy;
                        if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                        result.Add(nx + ny * w);
                    }

                    return result;
                }
            );
            
            pathFinder.Run();
            
            pathFinder.GetPathTo(bottomLeft, out var reservedCells);

            logger.AppendLine("Finished getting central path.");

            var availableCells = Enumerable.Range(0, w * h)
                .Where(i => !reservedCells!.Contains(i))
                .OrderBy(x => Random.Range(0f, 100f))
                .ToList();
            
            var count = availableCells.Count;
            var colorCount = gen.GenColorCount;
            var seatCount = gen.GenSeatCount;
            
            var colorSeatCount = seatCount / colorCount;
            var colorSeatCounts = Enumerable.Repeat(colorSeatCount, colorCount).ToArray();
            var leftOver = colorSeatCount * colorCount;
            
            logger.AppendLine($"Available cells: {availableCells.Count} / {w * h}");
            logger.AppendLine($"Gen seat cells: {seatCount}.");
            logger.AppendLine($"Seat per color: {colorSeatCount}.");
            logger.AppendLine($"Leftover: {leftOver}.");

            if (colorCount > 1)
            {
                for (var i = 0; i < 30; ++i)
                {
                    var randA = Random.Range(0, colorCount);
                    var randB = Random.Range(0, colorCount);
                    if (randA == randB || colorSeatCounts[randA] <= 0) continue;

                    colorSeatCounts[randA] -= 1;
                    colorSeatCounts[randB] += 1;
                }
            }

            var debug = string.Join(", ", colorSeatCounts);
            logger.AppendLine($"Final color seat counts: {debug}.");

            var grid = gen.LevelData.Grid;

            for (var colorI = 0; colorI < colorCount; ++colorI)
            {
                var color = (int)SeatEnum.BLUE + colorI;
                for (var i = 0; i < colorSeatCounts[colorI]; ++i)
                {
                    var cellI = availableCells[^1];
                    availableCells.RemoveAt(availableCells.Count - 1);
                    grid[cellI] = color;
                }
            }
            
            logger.AppendLine($"Generation completed.");
            
            Debug.Log("LOG: " + logger);
        }
    }
}