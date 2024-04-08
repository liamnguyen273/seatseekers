using UnityEngine;
using UnityEngine.EventSystems;

namespace com.brg.UnityComponents
{
    public abstract class MenuItem<TOption> : MonoBehaviour, IPointerEnterHandler, ICancelHandler
        where TOption : class, IMenuOption
    {
        private IMenu _targetMenu;
        private TOption _option;

        public IMenu TargetMenu => _targetMenu;
        public TOption Option => _option;

        public virtual void AttachOption(IMenu targetMenu, TOption option)
        {
            _targetMenu = targetMenu;
            _option = option;
        }

        public virtual void OnSelectedInMenu()
        {
            // Do nothing
        }

        public virtual void OnDeselectedInMenu()
        {
            // Do nothing
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            // TODO?
        }
    }
}