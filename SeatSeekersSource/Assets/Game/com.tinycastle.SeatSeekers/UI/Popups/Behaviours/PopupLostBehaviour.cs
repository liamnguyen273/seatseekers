using System;
using com.brg.Unity;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using JSAM;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupLostBehaviour : PopupBehaviour
    {
        [SerializeField] private CompWrapper<UIButton> _xButton;
        [SerializeField] private CompWrapper<UIButton> _noAdButton;
        [SerializeField] private CompWrapper<UIButton> _tryAgainButton;
        [SerializeField] private CompWrapper<UIButton> _mainMenuButton;

        private void Awake()
        {
            _xButton.Comp.OnClicked += OnXButton;
            _noAdButton.Comp.OnClicked += OnNoAdsButton;
            _tryAgainButton.Comp.OnClicked += OnRetryButton;
            _mainMenuButton.Comp.OnClicked += OnMainMenu;
        }

        protected override void InnateOnShowStart()
        {
            base.InnateOnShowStart();
        }

        protected override void InnateOnShowEnd()
        {
            AudioManager.PlaySound(AudioLibrarySounds.sfx_fail, transform);
            base.InnateOnShowEnd();
        }

        protected override void InnateOnHideEnd()
        {
            base.InnateOnHideEnd();
        }

        private void OnNoAdsButton()
        {
            
        }

        private void OnXButton()
        {
            var mainGame = GM.Instance.Get<MainGameManager>();
            mainGame.TransitOut();
        }
        
        private void OnMainMenu()
        {
            var mainGame = GM.Instance.Get<MainGameManager>();
            mainGame.TransitOut();
        }

        private void OnRetryButton()
        {
            // TODO: Check energy

            Restartlevel();
        }

        private void Restartlevel()
        {
            var mainGame = GM.Instance.Get<MainGameManager>();
            mainGame.RestartLevelWithValidation();
        }
    }
}