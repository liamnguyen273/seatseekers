using System;
using com.brg.UnityComponents;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeInOutPlayable : CompInOutPlayable
    {
        [SerializeField] private float _fadeTime = 0.65f;

        private bool _playIn;
        private Tween _tween;

        public override bool Playing => PlayingIn || PlayingOut;
        public override bool PlayingIn => _tween != null && _playIn;
        public override bool PlayingOut { get; }
        
        public override void PlayIn(Action completeCallback)
        {
            Kill();
            _playIn = true;
            _tween = GetComponent<CanvasGroup>().DOFade(1f, _fadeTime)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    Kill();
                    completeCallback?.Invoke();
                })
                .Play();
        }

        public override void PlayOut(Action completeCallback)
        {
            Kill();
            _playIn = false;
            _tween = GetComponent<CanvasGroup>().DOFade(0f, _fadeTime)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    Kill();
                    completeCallback?.Invoke();
                })
                .Play();
        }

        public override void CompleteIn()
        {
            if (PlayingIn) Complete();
        }

        public override void CompleteOut()
        {
            if (PlayingOut) Complete();
        }

        public void Complete()
        {
            _tween.Complete();
            _tween = null;
        }

        public override void Kill()
        {
            _tween?.Kill();
            _tween = null;
        }
    }
}