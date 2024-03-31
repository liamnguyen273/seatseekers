using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupMainMenu : PopupBehaviour
    {
        [SerializeField] private GOWrapper _levelPageHost = "./LevelPages";
        [SerializeField] private CompWrapper<UIButton> _buttonSettings;
        [SerializeField] private CompWrapper<UIButton> _buttonNoAds;

        public List<LevelPage> LevelPages { get; private set; }
        public List<LevelItem> LevelItems { get; private set; }

        private void Awake()
        {
            _buttonSettings.Comp.OnClicked += OnButtonSettings;
            _buttonNoAds.Comp.OnClicked += OnButtonNoAds;
            
            LevelPages = _levelPageHost.GameObject.GetDirectOrderedChildComponents<LevelPage>().ToList();
            LevelItems = new();
            foreach (var page in LevelPages)    
            {
                page.InitItems();
                var items = page.LevelItems;
                LevelItems.AddRange(items);
            }
        }

        protected override void InnateOnShowStart()
        {
            var levelNumber = 1;
            var levelId = $"level_{levelNumber}";
            var dataManager = GM.Instance.Get<GameDataManager>();
            var saveManager = GM.Instance.Get<GameSaveManager>();

            var levels = dataManager.GetAllLevelEntry();

            var currentBeaten = true;
            LevelItem currentItem = null;
            while (levels.TryGetValue(levelId, out var levelEntry) && levelNumber <= LevelItems.Count)
            {
                var completedLevel = saveManager.GetPlayerData_CompletedLevels(levelId);
                var lastIsBeaten = currentBeaten;
                currentBeaten = completedLevel != null && completedLevel.HasValue && completedLevel.Value.Completed;
                
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
            
        }
    }
}