using System;
using com.brg.Common;
using com.brg.UnityCommon;
using UnityEngine;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorInOutPlayable : CompInOutPlayable
    {
        [SerializeField] public string InAnimName = "In";
        [SerializeField] public string OutAnimName = "Out";

        private Animator _animator;
        private event Action _inCompleteEvent;
        private event Action _outCompleteEvent;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            var hasIn = false;
            var hasOut = false;
            for (var i = 0; i < _animator.runtimeAnimatorController.animationClips.Length; i++)
            {
                var clip = _animator.runtimeAnimatorController.animationClips[i];

                if (clip.name == InAnimName)
                {
                    hasIn = true;
                
                    var animationEndEvent = new AnimationEvent();
                    animationEndEvent.time = clip.length;
                    animationEndEvent.functionName = "OnAnimationInDone";
                    animationEndEvent.stringParameter = clip.name;
                
                    clip.AddEvent(animationEndEvent);
                }                
                
                if (clip.name == OutAnimName)
                {
                    hasOut = true;
                
                    var animationEndEvent = new AnimationEvent();
                    animationEndEvent.time = clip.length;
                    animationEndEvent.functionName = "OnAnimationOutDone";
                    animationEndEvent.stringParameter = clip.name;
                
                    clip.AddEvent(animationEndEvent);
                }
            }

            if (!hasIn)
            {
                LogObj.Default.Warn($"AnimatorPlayable {gameObject.name}",
                    $"In animation: Did not find an animation with name {InAnimName}.");
            }
            
            if (!hasOut)
            {
                LogObj.Default.Warn($"AnimatorPlayable {gameObject.name}",
                    $"Out animation: Did not find an animation with name {OutAnimName}.");
            }
        }

        public override void PlayIn(Action completeCallback)
        {
            _animator.StopPlayback();
            _outCompleteEvent = null;
            
            _inCompleteEvent += completeCallback;
            _animator.Play(InAnimName);
        }
        
        public override void PlayOut(Action completeCallback)
        {
            _animator.StopPlayback();
            _inCompleteEvent = null;
            
            _outCompleteEvent += completeCallback;
            _animator.Play(OutAnimName);
        }
        
        public override void Kill()
        {
            _inCompleteEvent = null;
            _outCompleteEvent = null;
            
            _animator.StopPlayback();
        }

        private void OnAnimationInDone(string _)
        {
            // TODO: Check if it stops playing
            
            _inCompleteEvent?.Invoke();
            _inCompleteEvent = null;
            // _animator.StopPlayback();
        }
        
        private void OnAnimationOutDone(string _)
        {
            // TODO: Check if it stops playing
            
            _outCompleteEvent?.Invoke();
            _outCompleteEvent = null;
            // _animator.StopPlayback();
        }
    }
}