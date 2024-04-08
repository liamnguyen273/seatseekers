using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.SeatSeekers
{
    [SpawnableEffect("UIFloater", typeof(EffectMaker))]
    public class UIFloater : MonoBehaviour, IPooledEffect<UIFloater>
    {
        [SerializeField] private CompWrapper<TextLocalizer> _text = "./Text";
        [SerializeField] private CompWrapper<Image> _image = "./Image";

        public IObjectPool<UIFloater> Pool { get; set; }
        
        private float _delay = 0.75f;
        private float _upDist = 150f;
        private Tween _tween;

        private void OnDisable()
        {
            _delay = 0.75f;
            _upDist = 150f;
            _text.Comp.SetGOActive(false);
            _image.Comp.SetGOActive(false);
            _image.Comp.sprite = null;
            
            _tween?.Kill();
            _tween = null;
        }

        public void Play()
        {
            gameObject.SetActive(true);
            _tween = GetPlayTween()
                .OnComplete(() =>
                {
                    _tween = null;
                })
                .Play();
        }
        
        public void PlayAndReturn()
        {
            gameObject.SetActive(true);
            _tween = GetPlayTween()
                .OnComplete(() =>
                {
                    _tween = null;
                    Return();
                })
                .Play();
        }

        public void Return()
        {
            Pool.Return(this);
        }

        private Tween GetPlayTween()
        {
            var rect = GetComponent<RectTransform>();
            var y = rect.anchoredPosition.y;
            return DOTween.Sequence()
                .Append(rect.DOAnchorPosY(y + _upDist, 0.25f).SetEase(Ease.OutCubic))
                .AppendInterval(_delay);
        }

        public UIFloater SetText(string text)
        {
            _text.Comp.SetGOActive(true);
            _text.Comp.Text = text;
            return this;
        }
        
        public UIFloater SetImage(Sprite sprite)
        {
            _image.Comp.SetGOActive(true);
            _image.Comp.sprite = sprite;
            return this;
        }

        public UIFloater SetUpDist(float upDist)
        {
            _upDist = upDist;
            return this;
        }

        public UIFloater SetDelay(float delay)
        {
            _delay = delay;
            return this;
        }
    }
}