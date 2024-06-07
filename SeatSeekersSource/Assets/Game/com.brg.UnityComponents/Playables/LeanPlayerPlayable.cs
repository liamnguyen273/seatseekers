using System;
using com.brg.Common;
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

        public override void Complete()
        {
            LogObj.Default.Error("LeanPlayerPlayable does not have a complete implementation.");
        }

        public override void Kill()
        {
            // Do nothing
        }

        public override bool Playing
        {
            get
            {
                LogObj.Default.Error("LeanPlayer does not have a method to reliably get whether is playable.");
                return false;
            }
        }
    }
}