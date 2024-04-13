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
        [SerializeField] private CompWrapper<BoosterButton>[] _boosters;

        private void Awake()
        {
            _buttonSettings.Comp.OnClicked += OnButtonSettings;
            _buttonNoAds.Comp.OnClicked += OnButtonNoAds;
        }

        public void OnLevel(int level)
        {
            foreach (var booster in _boosters)
            {
                booster.Comp.OnLevel(level);
                booster.Comp.ForceBoosterOff();
            }
        }

        public BoosterButton GetBoosterButton(string boosterName)
        {
            return boosterName switch
            {
                Constants.BOOSTER_FREEZE_RESOURCE => _boosters[0].Comp,
                Constants.BOOSTER_JUMP_RESOURCE => _boosters[1].Comp,
                Constants.BOOSTER_EXPAND_RESOURCE => _boosters[2].Comp,
                _ => null
            };
        }

        private void OnButtonSettings()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupSettingsBehaviour>(out var behaviour);
            behaviour.ShowQuitButton();
            popup.Show();
        }

        private void OnButtonNoAds()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup("popup_noads");
            popup.Show();
        }
    }
}