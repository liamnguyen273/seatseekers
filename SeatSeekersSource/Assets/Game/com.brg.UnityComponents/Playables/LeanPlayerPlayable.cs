using System;
using com.brg.UnityCommon;
using Lean.Transition;
using UnityEngine;

namespace com.brg.UnityComponents
{
    public class LeanPlayerPlayable : CompPlayable, IPlayable
    {
        [SerializeField] private LeanPlayer _leanPlayer;

        private void Awake()
        {
            
        }

        public override void Play(Action completeCallback)
        {
            _leanPlayer?.Begin();
            
            // Do not call.
        }

        public override void Kill()
        {
            // Do nothing
        }
    }
}