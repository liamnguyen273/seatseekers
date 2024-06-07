using System;
using com.brg.Common;
using SaintsField;
using UnityEngine;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorInOutPlayable : CompInOutPlayable
    {
        [SerializeField, AnimatorState] public AnimatorState InAnim;
        [SerializeField, AnimatorState] public AnimatorState OutAnim;
        
        private bool _playingIn;
        private bool _playingOut;

        public override bool Playing => PlayingIn || PlayingOut;
        public override bool PlayingIn => _playingIn;
        public override bool PlayingOut => _playingOut;
        
        private Animator _animator;
        private event Action _inCompleteEvent;
        private event Action _outCompleteEvent;
        
        private string InAnimName => InAnim.animationClip != null ? InAnim.animationClip.name : "In";
        private string OutAnimName => OutAnim.animationClip != null ? OutAnim.animationClip.name : "Out";

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
                LogObj.Default.Warn($"AnimatorInOutPlayable {gameObject.name}",
                    $"In animation: Did not find an animation with name {InAnimName}.");
            }

            if (!hasOut)
            {
                LogObj.Default.Warn($"AnimatorInOutPlayable {gameObject.name}",
                    $"Out animation: Did not find an animation with name {OutAnimName}.");
            }
        }

        public override void PlayIn(Action completeCallback)
        {
            _animator.StopPlayback();
            _outCompleteEvent = null;
            _playingOut = false;

            _inCompleteEvent += completeCallback;
            _animator.Play(InAnim.stateNameHash);
            _playingIn = true;
        }

        public override void PlayOut(Action completeCallback)
        {
            _animator.StopPlayback();
            _inCompleteEvent = null;
            _playingIn = false;

            _outCompleteEvent += completeCallback;
            _animator.Play(OutAnim.stateNameHash);
            _playingOut = true;
        }

        public override void CompleteIn()
        {
            LogObj.Default.Error("AnimatorInOutPlayable does not have a complete implementation.");
        }

        public override void CompleteOut()
        {
            LogObj.Default.Error("AnimatorInOutPlayable does not have a complete implementation.");
        }

        public override void Kill()
        {
            _inCompleteEvent = null;
            _outCompleteEvent = null;

            _animator.StopPlayback();
            _playingIn = false;
            _playingOut = false;
        }

        private void OnAnimationInDone(string _)
        {
            _playingIn = false;
            _inCompleteEvent?.Invoke();
            _inCompleteEvent = null;
        }

        private void OnAnimationOutDone(string _)
        {
            _playingOut = false;
            _outCompleteEvent?.Invoke();
            _outCompleteEvent = null;
        }
    }
}