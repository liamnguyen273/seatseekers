using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Unity;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using com.tinycastle.StickerBooker;
using JSAM;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupMainMenu : PopupBehaviour
    {
        [SerializeField] private GOWrapper _levelPageHost = "./LevelPages";
        [SerializeField] private CompWrapper<UIButton> _buttonSettings;
        [SerializeField] private CompWrapper<UIButton> _buttonNoAds;   
        [SerializeField] private CompWrapper<UIButton> _buttonSettings2;
        [SerializeField] private CompWrapper<UIButton> _buttonNoAds2;
        [SerializeField] private CompWrapper<UIButton> _buttonAddEnergy;
        [SerializeField] private CompWrapper<UIButton> _buttonAddCoin;
        [SerializeField] private CompWrapper<UIButton> _buttonVip;
        [SerializeField] private CompWrapper<UIButton> _buttonSuper;
        [SerializeField] private CompWrapper<UIButton> _debugEnergyButton;
        [SerializeField] private CompWrapper<UIButton> _buttonLeaderboard;

        public List<LevelPage> LevelPages { get; private set; }
        public List<LevelItem> LevelItems { get; private set; }

        private void Awake()
        {
            _buttonSettings.Comp.OnClicked += OnButtonSettings;
            _buttonSettings2.Comp.OnClicked += OnButtonSettings;
            _buttonNoAds.Comp.OnClicked += OnButtonNoAds;
            _buttonNoAds2.Comp.OnClicked += OnButtonNoAds;
            _debugEnergyButton.Comp.OnClicked += OnDebugEnergyButton;
            _buttonVip.Comp.OnClicked += OnVipButton;
            _buttonSuper.Comp.OnClicked += OnSuperButton;
            _buttonLeaderboard.Comp.OnClicked += OnLeaderboardButton;
            
            LevelPages = _levelPageHost.GameObject.GetDirectOrderedChildComponents<LevelPage>().ToList();
            LevelItems = new();
            foreach (var page in LevelPages)    
            {
                page.InitItems();
                var items = page.LevelItems;
                LevelItems.AddRange(items);
            }
        }

        private void OnLeaderboardButton()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupBehaviourLeaderboard>(out var behaviour);
            popup.Show();
        }

        private void OnSuperButton()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup("popup_bundle_super");
            popup.Show();
        }

        private void OnVipButton()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup("popup_bundle_vip");
            popup.Show();
        }

        protected override void InnateOnShowStart()
        {
            var mgm = GM.Instance.Get<MainGameManager>().transform;
            AudioManager.StopMusic(AudioLibraryMusic.music_game, mgm);
            AudioManager.StopMusic(AudioLibraryMusic.BPMusic1, mgm);
            AudioManager.StopMusic(AudioLibraryMusic.BPMusic2, mgm);
            AudioManager.StopMusic(AudioLibraryMusic.BPMusic3, mgm);
            AudioManager.StopMusic(AudioLibraryMusic.BPMusic4, mgm);
            AudioManager.StopMusic(AudioLibraryMusic.music_mainmenu);
            AudioManager.PlayMusic(AudioLibraryMusic.music_mainmenu, transform);
            
            var levelNumber = 1;
            var levelId = $"level_{levelNumber}";
            var dataManager = GM.Instance.Get<GameDataManager>();
            var saveManager = GM.Instance.Get<GameSaveManager>();

            var levels = dataManager.GetAllLevelEntry();

            var currentBeaten = true;
            LevelItem currentItem = null;
            while (levels.TryGetValue(levelId, out var levelEntry) && levelNumber <= LevelItems.Count)
            {
                var completedLevel = saveManager.PlayerData.GetFromCompletedLevels(levelId) is true;
                var lastIsBeaten = currentBeaten;
                currentBeaten = completedLevel;
                
                var unlocked = currentBeaten || lastIsBeaten;
                var isCurrent = !currentBeaten && lastIsBeaten;

                var item = LevelItems[levelNumber - 1];
                item.SetGOActive(true);
                item.SetLevel(levelEntry, unlocked, isCurrent);
                
                if (isCurrent)
                {
                    currentItem = item;
                }

                
                levelNumber += 1;
                levelId = $"level_{levelNumber}";
            }

            while (levelNumber <= LevelItems.Count)
            {
                var item = LevelItems[levelNumber - 1];
                item.SetGOActive(false);
                ++levelNumber;
            }

            if (currentItem != null)
            {
                // TODO: Scroll
            }
        }
        
        private void OnButtonSettings()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupSettingsBehaviour>(out var behaviour);
            popup.Show();
        }

        private void OnButtonNoAds()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup("popup_noads");
            popup.Show();
        }

        private void OnDebugEnergyButton()
        {
            var energy = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromResources(Constants.ENERGY_RESOURCE) ?? 0;
            if (energy < Constants.MAX_ENERGY)
            {
                energy += 1;
            }
            GM.Instance.Get<GameSaveManager>().PlayerData.SetInResources(Constants.ENERGY_RESOURCE, energy, true);
            GM.Instance.Get<GameSaveManager>().PlayerData.WriteDataAsync();
        }

        public void OnButtonAddEnergy()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup(out PopupRefill popupRefill);
            popup.Show();
        }

        public void OnButtonAddCoin()
        {
            
        }
    }
}