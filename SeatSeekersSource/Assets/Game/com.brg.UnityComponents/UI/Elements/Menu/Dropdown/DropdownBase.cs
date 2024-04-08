using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public abstract class DropdownBase<TMenu, TOption, TItem> : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler
        where TOption : class, IMenuOption
        where TItem : MenuItem<TOption>
        where TMenu : Menu<TOption, TItem>
    {
        [SerializeField] protected CompWrapper<RectTransform> _clickBlocker = "./ClickBlocker";
        [SerializeField] protected CompWrapper<TMenu> _attachedMenu = "./Menu";

        protected override void Awake()
        {
            if (Application.isPlaying)
            {
                _attachedMenu.Comp.OptionSelectedEvent += OnOptionSelected;
                _attachedMenu.Comp.OptionDeselectedEvent += OnOptionDeselected;
                PerformHide(true);
            }

            base.Awake();
        }

        protected override void OnDisable()
        {
            if (Application.isPlaying)
            {
                PerformHide(true);
            }
            
            base.OnDisable();
        }

        public abstract void OnOptionSelected(TOption option);
        public abstract void OnOptionDeselected(TOption option);

        public void OnClickBlockerHide()
        {
            PerformHide(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_attachedMenu.Comp.IsShown) PerformShow(false);
            else PerformHide(false);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            PerformShow(false);
        }

        public void OnCancel(BaseEventData eventData)
        {
            PerformHide(false);
        }

        protected virtual void PerformShow(bool immediately)
        {
            if (_clickBlocker.NullableComp != null)
            {
                _clickBlocker.Comp.SetGOActive(true);
                var rootCanvas = gameObject.GetRootCanvas();
                if (rootCanvas == null)
                {
                    return;
                }
            
                _clickBlocker.Comp.FitRectTransformTo(rootCanvas.GetComponent<RectTransform>());
            }
            
            if (immediately) _attachedMenu.Comp.ShowImmediately(); else _attachedMenu.Comp.Show();
        }
        protected virtual void PerformHide(bool immediately)
        {            
            if (_clickBlocker.NullableComp != null)
            {
                _clickBlocker.Comp.SetGOActive(false);
            }

            if (immediately) _attachedMenu.Comp.HideImmediately(); else _attachedMenu.Comp.Hide();
        }
    }
}