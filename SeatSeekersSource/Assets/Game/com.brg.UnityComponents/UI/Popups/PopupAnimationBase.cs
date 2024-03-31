using System;
using com.brg.Common;
using com.brg.UnityCommon;
using DG.Tweening;
using UnityEngine;

namespace com.brg.UnityComponents
{
    public enum UIAnimState
    {
        NONE,
        SHOW_START,
        SHOW_END,
        HIDE_START,
        HIDE_END
    }
    
    public abstract class PopupAnimationBase: MonoBehaviour, IInitializable
    {
        private UIAnimState _state;
        
        public Popup Popup { get; internal set; }
        public Action<UIAnimState> OnStateChangeAction { get; set; }
        protected IInOutPlayable InOutPlayable { get; private set; }
        protected IInOutPlayable ImmediateInOutPlayable { get; private set; }
        
        private IProgress _initializeProgress;
        public IProgress InitializationProgress => _initializeProgress ??= new ImmediateProgress(true, 1f);
        
        public virtual IProgress Initialize()
        {
            InOutPlayable = MakeInOutPlayable();
            ImmediateInOutPlayable = MakeImmediateInOutPlayable();
            SetState(UIAnimState.NONE);
            PlayHideImmediately();
            return InitializationProgress;
        }

        protected abstract IInOutPlayable MakeInOutPlayable();
        protected abstract IInOutPlayable MakeImmediateInOutPlayable();
        
        internal void PlayShow()
        {
            CleanPlayables();
            gameObject.SetActive(true);
            
            InOutPlayable.PlayIn(() =>
            {
                SetState(UIAnimState.SHOW_END);
            });
            SetState(UIAnimState.SHOW_START);
        }

        internal void PlayHide()
        {            
            CleanPlayables();
            
            InOutPlayable.PlayOut(() =>
            {
                SetState(UIAnimState.HIDE_END);
                gameObject.SetActive(false);
                SetState(UIAnimState.NONE);
            });
            SetState(UIAnimState.HIDE_START);
        }

        internal void PlayShowImmediately()
        {
            CleanPlayables();
            
            gameObject.SetActive(true);
            SetState(UIAnimState.SHOW_START);
            ImmediateInOutPlayable.PlayIn(null);
            SetState(UIAnimState.SHOW_END);
        }

        internal void PlayHideImmediately()
        {
            CleanPlayables();
            
            SetState(UIAnimState.HIDE_START);
            ImmediateInOutPlayable.PlayOut(null);
            SetState(UIAnimState.HIDE_END);
            gameObject.SetActive(false);
            SetState(UIAnimState.NONE);
        }

        private void CleanPlayables()
        {
            InOutPlayable.Kill();
            ImmediateInOutPlayable.Kill();
        }

        private void SetState(UIAnimState state)
        {
            _state = state;
            // Log is too verbose
            LogObj.Default.Info(Popup.ExplicitName, $"Animation: state is set to {_state}");
            OnStateChangeAction?.Invoke(state);
        }
    }
}