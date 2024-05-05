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
        
        private Tween _tween;
        
        public override void PlayIn(Action completeCallback)
        {
            Kill();
            _tween = GetComponent<CanvasGroup>().DOFade(1f, _fadeTime)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    completeCallback?.Invoke();
                    Kill();
                })
                .Play();
        }

        public override void PlayOut(Action completeCallback)
        {
            Kill();
            _tween = GetComponent<CanvasGroup>().DOFade(0f, _fadeTime)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    completeCallback?.Invoke();
                    Kill();
                })
                .Play();
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