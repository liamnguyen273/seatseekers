using com.brg.Common;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using DG.Tweening;
using UnityEngine;

namespace com.brg.UnityCommon.UI
{
    public class PopupAnimationUpThenDownFloat : PopupAnimationBase
    {
        [SerializeField] private float _time = 0.45f;
        
        [SerializeField] private CompWrapper<RectTransform> _panel = "./Panel";
        [SerializeField] private CompWrapper<CanvasGroup> _background = "./Background";
        private float _height;

        public override IProgress Initialize()
        {
            if (_panel.NullableComp == null) _panel.SetUp(transform.Find("Panel").GetComponent<RectTransform>(), gameObject);
            if (_background.NullableComp == null) _background.SetUp(transform.Find("Background").GetComponent<CanvasGroup>(), gameObject);
            
            _height = _panel.Comp.rect.height;
            return base.Initialize();
        }
        
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

        private void PerformShowImmediately()
        {
            _panel.Comp.anchoredPosition = Vector2.zero;
            _background.Comp.alpha = 0.75f;
        }
        
        private void PerformHideImmediately()
        {
            _panel.Comp.anchoredPosition = new Vector2(_panel.Comp.anchoredPosition.x, -_height - 15);
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
            _panel.Comp.anchoredPosition = new Vector2(_panel.Comp.anchoredPosition.x, -_height - 15);
            return _panel.Comp.DOAnchorPosY(0f, _time)
                .SetEase(Ease.OutSine)
                .SetUpdate(UpdateType.Normal, true)
                .Play();
        }

        private Tween GetPanelHideTweenOnly()
        {
            return _panel.Comp.DOAnchorPosY(-_height - 15, _time)
                .SetEase(Ease.OutSine)
                .SetUpdate(UpdateType.Normal, true)
                .Play();
        }
    }
}