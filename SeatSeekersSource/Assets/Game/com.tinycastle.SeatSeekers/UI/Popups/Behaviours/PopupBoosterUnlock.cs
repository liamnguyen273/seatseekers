using System;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupBoosterUnlock : PopupBehaviour
    {
        [SerializeField] private CompWrapper<UIButton> _xButton = "./Panel/TitleGroup/RightButtonGroup/XButton";
        [SerializeField] private GOWrapper _boosterFreezeGroup = "./Panel/Content/BoosterFreezeGroup";
        [SerializeField] private GOWrapper _boosterJumpGroup = "./Panel/Content/BoosterJumpGroup";
        [SerializeField] private GOWrapper _boosterExpandGroup = "./Panel/Content/BoosterExpandGroup";

        private void Awake()
        {
            if (_xButton.NullableComp != null) _xButton.Comp.OnClicked += () => Popup.Hide();
        }

        public void Setup(string boosterType)
        {
            _boosterFreezeGroup.SetActive(boosterType == GlobalConstants.BOOSTER_FREEZE_RESOURCE);
            _boosterJumpGroup.SetActive(boosterType == GlobalConstants.BOOSTER_JUMP_RESOURCE);
            _boosterExpandGroup.SetActive(boosterType == GlobalConstants.BOOSTER_EXPAND_RESOURCE);
        }
    }
}