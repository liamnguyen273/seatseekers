using System;
using System.Collections.Generic;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupRefill : PopupBehaviour
    {
        [SerializeField] private CompWrapper<UIButton> _xButton = "./Panel/TitleGroup/RightButtonGroup/XButton";
        
        private void Awake()
        {
            if (_xButton.NullableComp != null) _xButton.Comp.OnClicked += () => Popup.Hide();
        }
    }
}