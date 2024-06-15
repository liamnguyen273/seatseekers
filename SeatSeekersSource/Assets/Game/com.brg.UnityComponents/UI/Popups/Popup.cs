using System;
using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using UnityEngine;

namespace com.brg.UnityComponents
{
    public class Popup : UnityComp
    {
	    [SerializeField] private string _explicitName = "";
	    [SerializeField] private CompWrapper<UIButton> _backgroundButton = "./Background";
	    
	    private PopupManager _popupManager;
		private PopupAnimationBase _animation;
		private PopupBehaviour _behaviour;
		
		private RectTransform _rect;
		private int _popupZ;
		
		public int PopupZ
		{
			get => _popupZ;
			set
			{
				_rect ??= GetComponent<RectTransform>();
				_popupZ = value;
				var pos = _rect.anchoredPosition3D;
				pos.z = -_popupZ;
				_rect.anchoredPosition3D = pos;

				var canvas = GetComponent<Canvas>();
				if (canvas != null)
				{
					canvas.sortingOrder = _popupZ;
				}
			}
		}

		public string ExplicitName => _explicitName;
		public PopupBehaviour Behaviour => _behaviour;
		public PopupAnimationBase Animation => _animation;

		public PopupManager Manager => _popupManager ??= GM.Instance.Get<PopupManager>();

		public bool Active
		{
			get => _functionallyActive;
			set
			{
				if (value)
				{
					Show();
				}
				else
				{
					Hide();
				}
			}
		}

		private void OnDestroy()
		{
			if (_animation)
			{
				_animation.OnStateChangeAction -= OnAnimStateChange;
			}
		}

		public bool CloseByBackgroundTouch
		{
			get => _backgroundButton.Comp?.Interactable ?? false;
			set
			{
				if (_backgroundButton.Comp.Interactable != null)
				{
					_backgroundButton.Comp.Interactable = value;
				}
			}
		}

		public override IProgress InitializationProgress => new ImmediateProgress(true, 1f);

		public override IProgress Initialize()
		{
			if (Initialized) return new ImmediateProgress();
			
			if (_explicitName is "" or "")
			{
				_explicitName = name;
				LogObj.Default.Warn(_explicitName, $"UIPopup is missing explicit name, will use name of game object ({name})");
			}
			
			LogObj.Default.Info(_explicitName, $"Popup \"{_explicitName}\" is initializing.");
			
			// Get components
			_rect = GetComponent<RectTransform>();
			_animation = GetComponent<PopupAnimationBase>() ?? gameObject.AddComponent<PopupAnimationDefault>();
			_behaviour = GetComponent<PopupBehaviour>() ?? gameObject.AddComponent<PopupBehaviour>();
			
			_rect.anchoredPosition = new Vector2(0f, 0f);

			_animation.Popup = this;
			_behaviour.Popup = this;

			_behaviour.Initialize();
			_animation.Initialize();

			if (_backgroundButton.Comp != null)
			{
				_backgroundButton.Comp.OnClicked += HideByBackground;
			}

			_animation.OnStateChangeAction += OnAnimStateChange;

			Initialized = true;
			
			LogObj.Default.Success(_explicitName, $"Popup \"{_explicitName}\" has completed initialization.");
			return new ImmediateProgress(true, 1f);
		}

		private bool _functionallyActive;
		private bool _transiting;

		public bool IsShowing => _functionallyActive && _transiting;
		public bool IsHiding => !_functionallyActive && _transiting;
		public bool FunctionallyActive => _functionallyActive;
		public bool Transiting => _transiting;

		public void Show(bool immediately = false)
		{
			LogObj.Default.Success(_explicitName, $"Show request is called (immediately: {immediately})");
			Manager.ShowPopup(this, immediately, "");
		}

		public void ShowAndChain(Type chainTo, bool immediately = false)
		{
			LogObj.Default.Success(_explicitName, $"Show request is called (immediately: {immediately}, chaining to {chainTo})");
			Manager.ShowPopup(this, immediately, chainTo);
		}
		
		public void ShowAndChain(string chainTo, bool immediately = false)
		{
			LogObj.Default.Success(_explicitName, $"Show request is called (immediately: {immediately}, chaining to {chainTo})");
			Manager.ShowPopup(this, immediately, chainTo);
		}

		public void Hide(bool immediately = false)
		{
			LogObj.Default.Success(_explicitName, $"Hide request is called (immediately: {immediately})");
			Manager.HidePopup(this, immediately);
		}
		
		internal void ManagerShow(bool immediately)
		{
			_functionallyActive = true;
			if (immediately) _animation.PlayShowImmediately(); else _animation.PlayShow();
		}

		internal void ManagerHide(bool immediately)
		{			
			_functionallyActive = false;
			if (immediately) _animation.PlayHideImmediately(); _animation.PlayHide();
		}

		private void HideByBackground()
		{
			Hide();
		}

		private void OnAnimStateChange(UIAnimState state)
		{
			_transiting = state == UIAnimState.SHOW_START || state == UIAnimState.HIDE_START;
			switch (state)
			{
				case UIAnimState.SHOW_START:
					_behaviour.ShowStartEvent?.Invoke();
					break;
				case UIAnimState.SHOW_END:
					_behaviour.ShowEndEvent?.Invoke();
					break;
				case UIAnimState.HIDE_START:
					_behaviour.HideStartEvent?.Invoke();
					break;
				case UIAnimState.HIDE_END:
					_behaviour.HideEndEvent?.Invoke();
					break;
				case UIAnimState.NONE:
					_behaviour.CleanUpOnShowSessionCompleted();
					break;
			}
		}
    }
}
