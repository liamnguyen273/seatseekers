using System;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupLostBehaviour : PopupBehaviour
    {
        [SerializeField] private CompWrapper<UIButton> _xButton;
        [SerializeField] private CompWrapper<UIButton> _noAdButton;
        [SerializeField] private CompWrapper<UIButton> _tryAgainButton;

        private void Awake()
        {
            _xButton.Comp.OnClicked += OnXButton;
            _noAdButton.Comp.OnClicked += OnNoAdsButton;
            _tryAgainButton.Comp.OnClicked += OnRetryButton;
        }

        protected override void InnateOnShowStart()
        {
            base.InnateOnShowStart();
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
            var loading = GM.Instance.Get<LoadingScreen>();
            var popupManager = GM.Instance.Get<PopupManager>();
            
            loading.RequestLoad(mainGame.Deactivate(),
                () =>
                {
                    popupManager.HideAllPopups();
                    popupManager.GetPopup<PopupMainMenu>().Show();
                }, null);
        }

        private void OnRetryButton()
        {
            // TODO: Check energy

            Restartlevel();
        }

        private void Restartlevel()
        {
            var mainGame = GM.Instance.Get<MainGameManager>();
            var loading = GM.Instance.Get<LoadingScreen>();
            var popupManager = GM.Instance.Get<PopupManager>();
            
            loading.RequestLoad(mainGame.Activate(),
                () =>
                {
                    popupManager.HideAllPopups(true, true);
                    mainGame.RestartGame();
                }, mainGame.StartGame);
        }
    }
}