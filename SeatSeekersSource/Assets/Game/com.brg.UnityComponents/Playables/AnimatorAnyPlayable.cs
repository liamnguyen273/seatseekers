using System;
using System.Collections.Generic;
using com.brg.Common;
using com.brg.UnityCommon;
using UnityEngine;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorAnyPlayable : CompAnyPlayable
    {
        private Animator _animator;
        private event Action _completeEvent;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            
            for (var i = 0; i < _animator.runtimeAnimatorController.animationClips.Length; i++)
            {
                var clip = _animator.runtimeAnimatorController.animationClips[i];
                var animationEndEvent = new AnimationEvent();
                animationEndEvent.time = clip.length;
                animationEndEvent.functionName = "OnAnimationDone";
                animationEndEvent.stringParameter = clip.name;
                
                clip.AddEvent(animationEndEvent);
            }
        }

        public override void Play(string animName, Action completeCallback)
        {
            _completeEvent += completeCallback;
            _animator.Play(animName);
        }
        
        public override void Kill()
        {
            _completeEvent = null;
            _animator.StopPlayback();
        }

        private void OnAnimationDone(string animName)
        {
            // TODO: Check if it stops playing
            
            _completeEvent?.Invoke();
            _completeEvent = null;
            // _animator.StopPlayback();
        }
    }
}