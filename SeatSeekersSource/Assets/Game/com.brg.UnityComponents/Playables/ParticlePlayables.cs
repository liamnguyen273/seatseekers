using System;
using com.brg.UnityCommon;
using UnityEngine;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticlePlayable : CompPlayable
    {
        public override bool Playing => _particleSystem.isPlaying;
        
        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            var mainModule = _particleSystem.main;
            mainModule.stopAction = ParticleSystemStopAction.Callback;
            _particleSystem.Stop();
        }

        private event Action _completeEvent;

        public override void Play(Action completeCallback)
        {
            _completeEvent += completeCallback;
            _particleSystem.SetGOActive(true);
            _particleSystem.Play();
        }

        public override void Complete()
        {
            _particleSystem.Stop();
        }

        public override void Kill()
        {
            _completeEvent = null;
            _particleSystem.Stop();
        }
        
        private void OnParticleSystemStopped()
        {
            // TODO: Check if it stops playing

            _completeEvent?.Invoke();
            _completeEvent = null;
            _particleSystem.SetGOActive(false);
        }
    }
}