using System;
using com.brg.Common;
using com.brg.UnityCommon;
using UnityEngine;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorSinglePlayable : CompPlayable
    {
        [SerializeField] public string PlayAnimName = "Transition";

        private Animator _animator;
        private event Action _completeEvent;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            var success = false;
            for (var i = 0; i < _animator.runtimeAnimatorController.animationClips.Length; i++)
            {
                var clip = _animator.runtimeAnimatorController.animationClips[i];
                
                if (clip.name != PlayAnimName) continue;
                success = true;
                
                var animationEndEvent = new AnimationEvent();
                animationEndEvent.time = clip.length;
                animationEndEvent.functionName = "OnAnimationDone";
                animationEndEvent.stringParameter = clip.name;
                
                clip.AddEvent(animationEndEvent);
            }

            if (!success)
            {
                LogObj.Default.Warn($"AnimatorPlayable {gameObject.name}",
                    $"Did not find an animation with name {PlayAnimName}.");
            }
        }

        public override void Play(Action completeCallback)
        {
            _completeEvent += completeCallback;
            _animator.Play(PlayAnimName);
        }
        
        public override void Kill()
        {
            _completeEvent = null;
            _animator.StopPlayback();
        }

        private void OnAnimationDone(string _)
        {
            // TODO: Check if it stops playing
            
            _completeEvent?.Invoke();
            _completeEvent = null;
            // _animator.StopPlayback();
        }
    }
}