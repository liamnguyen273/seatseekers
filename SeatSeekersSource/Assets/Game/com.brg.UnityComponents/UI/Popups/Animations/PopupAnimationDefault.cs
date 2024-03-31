using System;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using DG.Tweening;
using UnityEngine;

namespace com.brg.UnityComponents
{
    public class PopupAnimationDefault : PopupAnimationBase
    {
        [SerializeField] private float _time = 0.45f;
        
        [SerializeField] private GOWrapper _panel = "./Panel";
        [SerializeField] private CompWrapper<CanvasGroup> _background = "./Background";

        protected override IInOutPlayable MakeInOutPlayable()
        {
            return new TweenInOutPlayable(
                () => DOTween.Sequence()
                        .Join(GetShowBackgroundTweenOnly())
                        .Join(GetPanelShowTweenOnly()), 
                () => DOTween.Sequence()
                        .Join(GetHideBackgroundTweenOnly())
                        .Join(GetPanelHideTweenOnly()));
        }

        protected override IInOutPlayable MakeImmediateInOutPlayable()
        {
            return new ImmediateInOutPlayable(PerformShowImmediately, PerformHideImmediately);
        }

        public override IProgress Initialize()
        {
            if (_panel.Comp == null) _panel.Set(transform.Find("Panel").gameObject, "./Panel");
            if (_background.Comp == null) _background.Set(transform.Find("Background").GetComponent<CanvasGroup>(), "./Background");

            return base.Initialize();
        }

        private void PerformShowImmediately()
        {
            _panel.Transform.localScale = Vector3.one;
            _background.Comp.alpha = 0.75f;
        }
        
        private void PerformHideImmediately()
        {
            _panel.Transform.localScale = Vector3.zero;
            _background.Comp.alpha = 0f;
        }
        
        private Tween GetShowBackgroundTweenOnly()
        {
            return DOTween.To(
                () => _background.Comp.alpha,
                (x) => _background.Comp.alpha = x,
                1f, _time)
                .Play();
        }        
        
        private Tween GetHideBackgroundTweenOnly()
        {
            return DOTween.To(
                () => _background.Comp.alpha,
                (x) => _background.Comp.alpha = x,
                0f, _time)
                .Play();
        }
        
        private Tween GetPanelShowTweenOnly()
        {
            return _panel.Transform.DOScale(1, _time)
                .SetEase(Ease.OutBack)
                .SetUpdate(UpdateType.Normal, true)
                .Play();
        }

        private Tween GetPanelHideTweenOnly()
        {
            return _panel.Transform.DOScale(0, _time)
                .SetEase(Ease.InBack)
                .SetUpdate(UpdateType.Normal, true)
                .Play();
        }
    }
}