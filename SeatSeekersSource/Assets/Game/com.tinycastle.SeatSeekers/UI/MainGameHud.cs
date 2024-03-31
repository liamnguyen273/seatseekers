using System;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class MainGameHud : MonoBehaviour
    {
        [SerializeField] private CompWrapper<UIButton> _buttonSettings;
        [SerializeField] private CompWrapper<UIButton> _buttonNoAds;
        [SerializeField] private CompWrapper<UIButton> _buttonBoosterFreezeTime;
        [SerializeField] private CompWrapper<UIButton> _buttonBoosterFreezeGetMore;
        [SerializeField] private CompWrapper<UIButton> _buttonBoosterJump;
        [SerializeField] private CompWrapper<UIButton> _buttonBoosterJumpGetMore;
        [SerializeField] private CompWrapper<UIButton> _buttonBoosterLine;
        [SerializeField] private CompWrapper<UIButton> _buttonBoosterLineGetMore;

        private void Awake()
        {
            _buttonSettings.Comp.OnClicked += OnButtonSettings;
            _buttonNoAds.Comp.OnClicked += OnButtonNoAds;
            _buttonBoosterFreezeTime.Comp.OnClicked += OnBoosterFreeze;
            _buttonBoosterFreezeGetMore.Comp.OnClicked += OnBoosterFreezeGetMore;
            _buttonBoosterJump.Comp.OnClicked += OnBoosterJump;
            _buttonBoosterJumpGetMore.Comp.OnClicked += OnBoosterJumpGetMore;
            _buttonBoosterLine.Comp.OnClicked += OnBoosterLine;
            _buttonBoosterLineGetMore.Comp.OnClicked += OnBoosterLineGetMore;
        }

        private void OnButtonSettings()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupSettingsBehaviour>(out var behaviour);
            behaviour.ShowQuitButton();
            popup.Show();
        }

        private void OnButtonNoAds()
        {
            
        }

        private void OnBoosterFreeze()
        {
            
        }

        private void OnBoosterJump()
        {
            
        }

        private void OnBoosterLine()
        {
            
        }

        private void OnBoosterFreezeGetMore()
        {
            
        }

        private void OnBoosterJumpGetMore()
        {
            
        }

        private void OnBoosterLineGetMore()
        {
            
        }
    }
}