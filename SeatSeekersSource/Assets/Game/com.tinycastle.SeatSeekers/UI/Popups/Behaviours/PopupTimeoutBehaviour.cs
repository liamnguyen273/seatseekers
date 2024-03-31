using System;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupTimeoutBehaviour : PopupBehaviour
    {
        [SerializeField] private CompWrapper<UIButton> _xButton;
        [SerializeField] private CompWrapper<UIButton> _continueButton;
        // [SerializeField] private CompWrapper<UIButton> _goldpackButton;

        private void Awake()
        {
            _xButton.Comp.OnClicked += OnXButton;
            _continueButton.Comp.OnClicked += OnContinueButton;
        }

        protected override void InnateOnShowStart()
        {
            base.InnateOnShowStart();
        }

        protected override void InnateOnHideEnd()
        {
            base.InnateOnHideEnd();
        }

        private void OnContinueButton()
        {
            // TODO: Ads
            
            var mainGame = GM.Instance.Get<MainGameManager>();
            mainGame.UseTimeoutChanceAndContinue();
            Popup.Hide();
        }

        private void OnXButton()
        {
            var mainGame = GM.Instance.Get<MainGameManager>();
            mainGame.OnRefuseTimeoutChance();
            
            Popup.Hide();
        }
    }
}