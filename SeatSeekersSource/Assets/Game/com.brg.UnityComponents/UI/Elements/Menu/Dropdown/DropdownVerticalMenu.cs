using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public class DropdownVerticalMenu : VerticalMenu<ClassicMenuOption, ClassicToggleMenuItem>, IContainsToggleGroup
    {
        [SerializeField] private CompWrapper<ToggleGroup> _toggleGroup;
        
        public ToggleGroup GetToggleGroup()
        {
            return _toggleGroup.NullableComp != null ? _toggleGroup.Comp : null;
        }
        
    }
}