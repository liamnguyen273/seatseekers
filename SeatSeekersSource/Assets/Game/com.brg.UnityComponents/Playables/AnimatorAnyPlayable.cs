using System;
using com.brg.Common;
using UnityEngine;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorAnyPlayable : CompAnyPlayable
    {
        private bool _playing;
        public override bool Playing => _playing;
        
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

        public override void Play(string stateName, Action completeCallback)
        {
            _completeEvent += completeCallback;
            _animator.Play(stateName);
            _playing = true;
        }

        public override void CompleteCurrent()
        {
            LogObj.Default.Error("AnimatorAnyPlayable does not have a complete implementation.");
        }

        public override void Complete(string name)
        {
            LogObj.Default.Error("AnimatorAnyPlayable does not have a complete implementation.");
        }

        public override void Kill()
        {
            _completeEvent = null;
            _animator.StopPlayback();
            _playing = false;
        }

        private void OnAnimationDone(string animName)
        {
            _playing = false;
            _completeEvent?.Invoke();
            _completeEvent = null;
        }
    }
}