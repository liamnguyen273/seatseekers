using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using JSAM;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupWinBehaviour : PopupBehaviour
    {
        [SerializeField] private CompWrapper<TextLocalizer> _doubleCoinText;
        [SerializeField] private CompWrapper<TextLocalizer> _coinText;
        [SerializeField] private CompWrapper<UIButton> _doubleCoinButton;
        [SerializeField] private CompWrapper<UIButton> _continueButton;
        [SerializeField] private CompWrapper<Animator> _fireworkLeft;
        [SerializeField] private CompWrapper<Animator> _fireworkRight;

        private LevelEntry _entry;
        private int _coinValue = 0;
        private bool _received = false;

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

            _coinText.Comp.Text = $"+{_coinValue}";
            _doubleCoinText.Comp.Text = $"+{_coinValue * 2}";
            _received = false;
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
            
            _fireworkLeft.Comp.Play("fire");
            _fireworkRight.Comp.Play("fire");
            AudioManager.PlaySound(AudioLibrarySounds.sfx_fanfare, transform);
        }

        protected override void InnateOnHideEnd()
        {
            base.InnateOnHideEnd();
            _coinValue = 0;
            _entry = null;
            _fireworkLeft.Comp.StopPlayback();
            _fireworkRight.Comp.StopPlayback();
        }

        private void OnDoubleButton()
        {
            // TODO: Show ad
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            var curr = accessor.GetFromResources(GlobalConstants.COIN_RESOURCE) ?? 0;
            curr += _coinValue * 2;
            accessor.SetInResources(GlobalConstants.COIN_RESOURCE, curr, true);
            accessor.WriteDataAsync();
            _received = true;
            
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

            if (!_received)
            {
                var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
                var curr = accessor.GetFromResources(GlobalConstants.COIN_RESOURCE) ?? 0;
                curr += _coinValue;
                accessor.SetInResources(GlobalConstants.COIN_RESOURCE, curr, true);
                accessor.WriteDataAsync();
            }

            var canGoNext = mainGame.HasNextLevel(out var nextLevel);
            
            if (!canGoNext)
            {
                mainGame.TransitOut();
            }
            else
            {
                GM.Instance.Get<MainGameManager>().ForceClearLevel();
                GM.Instance.RequestPlayLevelWithValidation(nextLevel);
            }
        }
    }
}