using System;
using com.brg.UnityCommon;
using DG.Tweening;

namespace com.brg.UnityComponents
{
    public class TweenPlayable : IPlayable
    {
        private readonly Func<Tween> _getter;
        private Tween _tween;
        private event Action _completeEvent;

        public TweenPlayable(Func<Tween> getter)
        {
            _getter = getter;
        }
        
        public void Play(Action completeCallback)
        {
            _completeEvent += completeCallback;

            if (_tween is not null) return;
            _tween = _getter()
                .OnComplete(OnTweenCompleted)
                .Play();
        }

        public void Kill()
        {
            _tween?.Kill();
            _tween = null;
            _completeEvent = null;
        }

        private void OnTweenCompleted()
        {
            _completeEvent?.Invoke();
            _completeEvent = null;
        }
    }

    public class TweenInOutPlayable : IInOutPlayable
    {
        private readonly Func<Tween> _inGetter;
        private readonly Func<Tween> _outGetter;
        private Tween _inTween;
        private Tween _outTween;
        private event Action _inCompleteEvent;
        private event Action _outCompleteEvent;

        public TweenInOutPlayable(Func<Tween> inGetter, Func<Tween> outGetter)
        {
            _inGetter = inGetter;
            _outGetter = outGetter;
        }

        public bool PlayingIn => _inTween != null;
        public bool PlayingOut => _inTween != null;
        
        public void PlayIn(Action completeCallback)
        {
            if (_inTween is not null) return;
            if (_outTween is not null) Kill();

            _inCompleteEvent += completeCallback;
            
            _inTween = _inGetter()
                .OnComplete(OnInCompleted)
                .Play();
        }

        public void PlayOut(Action completeCallback)
        {
            if (_outTween is not null) return;
            if (_inTween is not null) Kill();
            
            _outCompleteEvent += completeCallback;

            _outTween = _outGetter()
                .OnComplete(OnOutCompleted)
                .Play();
        }

        public void Kill()
        {
            _inTween = null;
            _outTween = null;
            _inCompleteEvent = null;
            _outCompleteEvent = null;
        }

        private void OnInCompleted()
        {
            _inCompleteEvent?.Invoke();
            _inCompleteEvent = null;

            _inTween = null;
        }
        
        private void OnOutCompleted()
        {
            _outCompleteEvent?.Invoke();
            _outCompleteEvent = null;

            _outTween = null;
        }
    }
}