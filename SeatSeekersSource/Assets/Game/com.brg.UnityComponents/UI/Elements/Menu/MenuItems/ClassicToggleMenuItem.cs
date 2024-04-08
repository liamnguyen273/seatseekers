using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public interface IContainsToggleGroup
    {
        public ToggleGroup GetToggleGroup();
    }
    
    public class ClassicToggleMenuItem : MenuItem<ClassicMenuOption>
    {
        [SerializeField] private TextLocalizer _text;
        [SerializeField] private Image _iconImage;
        [SerializeField] private UIToggle _toggle;
        
        public override void AttachOption(IMenu targetMenu, ClassicMenuOption option)
        {
            base.AttachOption(targetMenu, option);

            _iconImage.sprite = option.Icon;
            _text.Text = option.DisplayStringKey;
            _toggle.ValueChangedEvent += OnToggleValue;

            if (targetMenu is IContainsToggleGroup toggleGroupGetter)
            {
                var toggleGroup = toggleGroupGetter.GetToggleGroup();
                _toggle.ToggleValue = false;    // Default to false, to avoid weird stuffs.
                _toggle.Group = toggleGroup;
            }
        }

        public override void OnSelectedInMenu()
        {
            if (!_toggle.ToggleValue)
            {
                _toggle.ToggleValueNoEvent = true;
            }
            base.OnSelectedInMenu();
        }

        public override void OnDeselectedInMenu()
        {
            if (_toggle.ToggleValue)
            {
                _toggle.ToggleValueNoEvent = false;
            }
            base.OnDeselectedInMenu();
        }

        private void OnToggleValue(bool value)
        {
            if (value)
            {
                TargetMenu?.SelectOption(Option.Category, Option.Id);
            }
            else
            {
                TargetMenu?.DeselectOption(Option.Category, Option.Id);
            }
        }
    }
}