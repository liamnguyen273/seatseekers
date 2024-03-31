using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public class LoadingScreen : UnityComp
    {
        [Header("Components")]
        [SerializeField] private GOWrapper _initBackground = "./InitBackground";
        [SerializeField] private GOWrapper _inputBlocker = "./Blocker";
        [SerializeField] private CompWrapper<RectTransform> _animRect = "./AnimRect";      
        [SerializeField] private CompWrapper<Slider> _progressSlider = "./AnimRect/Appearance/Bar";
        
        [Header("Params")]
        [SerializeField] private float _transitTime = 1f;
        [SerializeField] private float _naturalDelayTime = 0.5f;

        private float _screenWidth;
        
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

            _screenWidth = GetComponent<RectTransform>().rect.width;
            _animRect.Comp.anchoredPosition = new Vector2(-_screenWidth - 15, 0f);
            
            // Make playable
            _transitPlayable = new TweenInOutPlayable(
                () => DOTween.Sequence()
                            .Append(_animRect.Comp.DOAnchorPosX(0, _transitTime)
                            .SetEase(Ease.OutCubic))
                            .AppendInterval(_naturalDelayTime), 
                () => DOTween.Sequence()
                            .AppendInterval(_naturalDelayTime)
                            .Append(_animRect.Comp.DOAnchorPosX(_screenWidth + 15, _transitTime)
                            .SetEase(Ease.OutCubic))
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

            if (_transitPlayable is TweenInOutPlayable { PlayingIn: true })
            {
                return;
            }
            
            if (_overallProgress.Finished)
            {
                OnProgressDone();
            }
        }

        private void OnDestroy()
        {
            GM.Instance = null;
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
            
            _animRect.Comp.anchoredPosition = new Vector2(-_screenWidth - 15, 0f);
            _transitPlayable.PlayIn(() =>
            {
                _initBackground.GameObject.SetActive(false);
            });
        }
        
        private void OnProgressDone()
        {
            _overallProgress = null;
            _progresses.Clear();
            
            _beforeOutEvent?.Invoke();
            _beforeOutEvent = null;
            
            SetProgressBar(1f);
            
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