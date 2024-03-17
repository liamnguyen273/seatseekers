using System;
using com.brg.Common.Initialization;
using com.brg.UnityCommon.Editor;
using UnityEngine;

namespace com.tinycastle.SeatCinema
{
    public partial class GEditor : MonoManagerBase
    {
        public static GEditor Instance { get; private set; }
        
        [Header("Components")]
        [SerializeField] private CompWrapper<MainGameManager> _mainGameManager = ".";

        [SerializeField] private string _csvPath = "../Data/level_inputs.csv";

        public MainGameManager MainGame => _mainGameManager.Comp;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Initialize();
        }

        public string GetLevelInput(string levelName)
        {
            return "6,8,1_1_1_1_0_0_0_0_0_0_0_1";
        }
    }
}