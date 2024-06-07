using System;
using System.Collections.Generic;
using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public class LoadingScreen : UnityComp
    {
        [Header("Components")]
        [SerializeField] private GOWrapper _inputBlocker = "./Blocker";
        [SerializeField] private CompWrapper<RectTransform> _animRect = "./AnimRect";      
        [SerializeField] private CompWrapper<Slider> _progressSlider = "./AnimRect/Appearance/Bar";
        [SerializeField] private GOWrapper _initBackground = "./InitBackground";
        
        [Header("Params")]
        [SerializeField] private float _transitTime = 1f;
        [SerializeField] private float _naturalDelayTime = 0.5f;

        private float _screenWidth;

        private bool _transitInDone = false;
        private IProgress _overallProgress;
        private readonly List<IProgress> _progresses = new List<IProgress>();
        private event Action _beforeOutEvent;
        private event Action _completeEvent;
        
        private IInOutPlayable _transitPlayable;
        
        private void Awake()
        {
            var rect = GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, 0f);
            SetProgressBar(0f);
            
            _initBackground.SetActive(true);

            _screenWidth = GetComponent<RectTransform>().rect.width;
            _animRect.Comp.anchoredPosition = new Vector2(-_screenWidth - 15, 0f);
            
            // Make playable
            _transitPlayable = new TweenInOutPlayable(
                () => DOTween.Sequence()
                            .AppendCallback(() => _animRect.Comp.anchoredPosition = new Vector2(-_screenWidth - 15, 0f))
                            .Append(_animRect.Comp.DOAnchorPosX(0, _transitTime)
                            .SetEase(Ease.InOutSine))
                            .AppendInterval(_naturalDelayTime), 
                () => DOTween.Sequence()
                            .AppendCallback(() => _animRect.Comp.anchoredPosition = new Vector2(0f, 0f))
                            .AppendInterval(_naturalDelayTime)
                            .Append(_animRect.Comp.DOAnchorPosX(_screenWidth + 15, _transitTime)
                            .SetEase(Ease.InOutSine))
                );
        }
        
        private void LateUpdate()
        {
            if (_overallProgress is null)
            {
                // No load
                return;
            }

            UpdateProgressBar(_overallProgress);
            
            if (_transitInDone && _overallProgress.Finished)
            {
                OnProgressDone();
            }
        }

        public void RequestLoad(IProgress item,  
            Action beforeOutAction = null, 
            Action completeAction = null
            )
        {
            _progresses.Add(item);
            _overallProgress = new ProgressGroup(ProgressLeniency.REQUIRE_ALL_SUCCEEDED, _progresses);

            _beforeOutEvent += beforeOutAction;
            _completeEvent += completeAction;
            
            _animRect.GameObject.SetActive(true);
            _inputBlocker.GameObject.SetActive(true);
            _transitInDone = false;
            _transitPlayable.PlayIn(() =>
            {
                _initBackground.SetActive(false);
                _transitInDone = true;
            });
        }
        
        private void OnProgressDone()
        {
            _overallProgress = null;
            _progresses.Clear();
            
            _beforeOutEvent?.Invoke();
            _beforeOutEvent = null;
            
            _transitPlayable.PlayOut(() =>
            {
                _completeEvent?.Invoke();
                _completeEvent = null;
                _inputBlocker.GameObject.SetActive(false);
                _animRect.GameObject.SetActive(false);
            });
        }

        private void UpdateProgressBar(IProgress progress)
        {
            var percentage = progress.PercentageProgress;
            SetProgressBar(percentage);
        }

        private void SetProgressBar(float value)
        {
            _progressSlider.Comp.value = value;
        }

        public override IProgress InitializationProgress => new ImmediateProgress(true, 1f);
    }
}