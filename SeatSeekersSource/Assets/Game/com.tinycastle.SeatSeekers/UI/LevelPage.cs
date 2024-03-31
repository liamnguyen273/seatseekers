using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class LevelPage : MonoBehaviour
    {
        [SerializeField] private GOWrapper _levelHost = "./Levels";

        public List<LevelItem> LevelItems { get; private set; }

        private void Awake()
        {
            
        }

        public void InitItems()
        {
            LevelItems ??= _levelHost.GameObject.GetDirectOrderedChildComponents<LevelItem>().ToList();
        }
    }
}