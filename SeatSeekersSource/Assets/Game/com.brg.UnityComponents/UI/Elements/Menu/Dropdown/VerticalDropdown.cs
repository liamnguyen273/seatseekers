using com.brg.Common;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public class VerticalDropdown : DropdownBase<DropdownVerticalMenu, ClassicMenuOption, ClassicToggleMenuItem>
    {
        [Header("Caption display")] 
        [SerializeField] private CompWrapper<TextLocalizer> _captionText = "./CaptionLabel";
        [SerializeField] private CompWrapper<Image> _captionIcon = "./CaptionIcon";

        [Header("Selection")] 
        [SerializeField] private bool _hideOnSelect = true;
        [SerializeField] private bool _toggleDefault = true;
        [SerializeField] private bool _invokeDeselectedEvent = true;
        [SerializeField] private string _defaultItemCategory = "";
        [SerializeField] private string _defaultItemId = "";
        
        [Header("Events")]
        [SerializeField] private EventWrapper<ClassicMenuOption> SelectedEvent;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Application.isPlaying)
            {
                if (_toggleDefault)
                {
                    _attachedMenu.Comp.SelectOption(_defaultItemCategory, _defaultItemId);
                }
                else
                {
                    _captionText.Comp.Text = "";
                    _captionIcon.Comp.sprite = null;
                }
            }
        }

        public override void OnOptionSelected(ClassicMenuOption option)
        {
            _captionText.Comp.Text = option.DisplayStringKey;
            _captionIcon.Comp.sprite = option.Icon;
            SelectedEvent?.Invoke(option);

            if (_hideOnSelect)
            {
                PerformHide(false);
            }
        }

        public override void OnOptionDeselected(ClassicMenuOption option)
        {
            _captionText.Comp.Text = "";
            _captionIcon.Comp.sprite = null;
            if (_invokeDeselectedEvent) SelectedEvent?.Invoke(null);
            
            if (_hideOnSelect)
            {
                PerformHide(false);
            }
        }
    }
}