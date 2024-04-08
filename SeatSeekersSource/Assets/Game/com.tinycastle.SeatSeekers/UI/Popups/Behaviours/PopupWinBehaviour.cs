using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupWinBehaviour : PopupBehaviour
    {
        [SerializeField] private CompWrapper<TextLocalizer> _doubleCoinText;
        [SerializeField] private CompWrapper<TextLocalizer> _coinText;
        [SerializeField] private CompWrapper<UIButton> _doubleCoinButton;
        [SerializeField] private CompWrapper<UIButton> _continueButton;

        private LevelEntry _entry;
        private int _coinValue = 0;

        public void SetCoin(LevelEntry entry, int coin)
        {
            _coinValue = coin;
            _entry = entry;
        }

        private void Awake()
        {
            _doubleCoinButton.Comp.OnClicked += OnDoubleButton;
            _continueButton.Comp.OnClicked += OnContinueButton;
        }

        protected override void InnateOnShowStart()
        {
            base.InnateOnShowStart();

            _coinText.Comp.Text = $"{_coinValue}";
            _doubleCoinText.Comp.Text = $"{_coinValue * 2}";
        }

        protected override void InnateOnShowEnd()
        {
            base.InnateOnShowEnd();
            
            // Save
            if (_entry != null)
            {
                var saveManager = GM.Instance.Get<GameSaveManager>();
                saveManager.PlayerData.SetInCompletedLevels(_entry.Id, true);
                saveManager.SavePlayerData();
            }
            else
            {
                LogObj.Default.Warn("Popup Win does not have an _entry, cannot save level.");
            }
        }

        protected override void InnateOnHideEnd()
        {
            base.InnateOnHideEnd();
            _coinValue = 0;
            _entry = null;
        }

        private void OnDoubleButton()
        {
            // TODO: Show ad
            
            
            
            GoToNextLevel();
        }

        private void OnContinueButton()
        {
            
            GoToNextLevel();
        }

        private void GoToNextLevel()
        {
            var mainGame = GM.Instance.Get<MainGameManager>();
            var loading = GM.Instance.Get<LoadingScreen>();
            var popupManager = GM.Instance.Get<PopupManager>();

            var canGoNext = mainGame.HasNextLevel(out var nextLevel);
            
            if (!canGoNext)
            {
                mainGame.TransitOut();
            }
            else
            {
                GM.Instance.RequestPlayLevelWithValidation(nextLevel);
            }
        }
    }
}