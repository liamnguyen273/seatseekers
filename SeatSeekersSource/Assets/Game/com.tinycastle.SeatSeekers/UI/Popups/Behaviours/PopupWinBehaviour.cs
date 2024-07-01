using System;
using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityCommon;
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
        [SerializeField] private CompWrapper<UIButton> _mainMenuButton;

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
            _mainMenuButton.Comp.OnClicked += OnMainMenu;
        }

        protected override void InnateOnShowStart()
        {
            base.InnateOnShowStart();
            _adButtonTimer = true;
            _doubleCoinButton.Comp.Interactable = true;
            _continueButton.Comp.Interactable = true;

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
                var has = saveManager.PlayerData.GetFromCompletedLevels(_entry.Id) ?? false;
                saveManager.PlayerData.SetInCompletedLevels(_entry.Id, true);

                if (!has)
                {
                    var extraData = GM.Instance.Get<GameSaveManager>().ExtraData;
                    GM.Instance.Get<AnalyticsEventManager>().MakeEvent("sng_level_achieved")
                        .Add("sng_attr_level", _entry.SortOrder)
                        .Add("ltv", extraData.Ltv)
                        .Add("interstitial_views", extraData.InterstitialViews)
                        .Add("rewarded_views", extraData.RewardedViews)
                        .Add("time", extraData.Time)
                        .Add("day", DateTime.UtcNow.ToLongDateString())
                        .SendEvent();

                    CheckMilestone(_entry);
                }
                
                GM.Instance.Get<PopupManager>().GetPopup(out PopupRefill refill);
                refill.Timer = 30f;
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
        
        private void OnMainMenu()
        {
            var mainGame = GM.Instance.Get<MainGameManager>();
            mainGame.TransitOut();
        }

        private void OnDoubleButton()
        {
            if (_received) return;

            GM.Instance.Get<UnityAdManager>().RequestAd(new AdRequest(AdRequestType.REWARD_AD, () =>
            {
                var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
                var curr = accessor.GetFromResources(Constants.COIN_RESOURCE) ?? 0;
                curr += _coinValue * 2;
                accessor.SetInResources(Constants.COIN_RESOURCE, curr, true);
                GM.Instance.Get<GameSaveManager>().SaveAll();
                _received = true;
                            
                GoToNextLevel();
            }, () =>
            {
                // Do nothing
                var popup = GM.Instance.Get<PopupManager>().GetPopup(out PopupBehaviourGeneric generic);
                generic.SetupAsNotify("Uh oh", "Reward ad is not available at the moment, try again later.");
                popup.Show();
            }));
            
            _doubleCoinButton.Comp.Interactable = false;
        }

        private bool _adButtonTimer = true;
        private void OnContinueButton()
        {
            _doubleCoinButton.Comp.Interactable = false;
            _continueButton.Comp.Interactable = false;

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
                var curr = accessor.GetFromResources(Constants.COIN_RESOURCE) ?? 0;
                curr += _coinValue;
                accessor.SetInResources(Constants.COIN_RESOURCE, curr, true);
                GM.Instance.Get<GameSaveManager>().SaveAll();
            }

            var canGoNext = mainGame.HasNextLevel(out var nextLevel);
            
            if (!canGoNext)
            {
                mainGame.TransitOut();
            }
            else
            {
                GM.Instance.Get<MainGameManager>().ForceClearLevel();
                GM.Instance.RequestPlayLevelWithValidation(nextLevel, ref _adButtonTimer);
            }
        }

        private void CheckMilestone(LevelEntry entry)
        {
            var eventName = entry.SortOrder switch
            {
                5 => "00",
                10 => "01",
                25 => "02",
                50 => "03",
                100 => "04",
                150 => "05",
                200 => "06",
                _ => null
            };

            if (eventName == null) return;

            eventName = $"mn_milestone_{eventName}";
            
            var extraData = GM.Instance.Get<GameSaveManager>().ExtraData;
            GM.Instance.Get<AnalyticsEventManager>().MakeEvent(eventName)
                .Add("ltv", extraData.Ltv)
                .Add("interstitial_views", extraData.InterstitialViews)
                .Add("rewarded_views", extraData.RewardedViews)
                .Add("time", extraData.Time)
                .SendEvent();
        }
    }
}