using System;
using com.brg.Common;
using SaintsField;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorSinglePlayable : CompPlayable
    {
        [SerializeField, AnimatorState] public AnimatorState Anim;

        private bool _playing;
        public override bool Playing => _playing;
        
        private Animator _animator;
        private event Action _completeEvent;

        private string AnimName => Anim.animationClip != null ? Anim.animationClip.name : "";

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            var success = false;
            for (var i = 0; i < _animator.runtimeAnimatorController.animationClips.Length; i++)
            {
                var clip = _animator.runtimeAnimatorController.animationClips[i];

                if (clip.name != AnimName) continue;
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
                    $"Did not find an animation with name {AnimName}.");
            }
        }

        public override void Play(Action completeCallback)
        {
            _completeEvent += completeCallback;
            _animator.Play(Anim.stateNameHash);
            _playing = true;
        }

        public override void Complete()
        {
            LogObj.Default.Error("AnimatorSinglePlayable does not have a complete implementation.");
        }

        public override void Kill()
        {
            _completeEvent = null;
            _animator.StopPlayback();
            _playing = false;
        }

        private void OnAnimationDone(string _)
        {
            _playing = false;
            _completeEvent?.Invoke();
            _completeEvent = null;
        }
    }
}