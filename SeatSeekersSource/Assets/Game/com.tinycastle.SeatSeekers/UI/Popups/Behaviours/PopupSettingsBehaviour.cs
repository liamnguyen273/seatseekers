using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupSettingsBehaviour : PopupBehaviour
    {
        [Header("Components")]
        [SerializeField] private CompWrapper<TextLocalizer> _version = "./Panel/Content/Version";
        // [SerializeField] private CompWrapper<UISwitch> _musicToggle = "./Panel/Content/MusicToggler";
        [SerializeField] private CompWrapper<UISwitch> _sfxToggle = "./Panel/Content/SFXToggler";
        [SerializeField] private CompWrapper<UISwitch> _vibrationToggle = "./Panel/Content/SFXToggler";
        
        [SerializeField] private CompWrapper<UIButton> _xButton;
        [SerializeField] private CompWrapper<UIButton> _quitButton = "./Panel/ButtonGroup/Button2";
        [SerializeField] private CompWrapper<UIButton> _okButton = "./Panel/ButtonGroup/Button2";
        
        [Header("Others")]
        [SerializeField] private CompWrapper<UIButton> _privacyButton = "./Panel/CenterGroup/ButtonPrivacy";
        [SerializeField] private CompWrapper<UIButton> _restoreButton = "./Panel/CenterGroup/ButtonRestore";
        [SerializeField] private CompWrapper<UIButton> _creditButton = "./Panel/CenterGroup/ButtonCredit";

        private void Awake()
        {
            _quitButton.Comp.OnClicked += OnQuitButton;
            _okButton.Comp.OnClicked += OnXButton;
            _xButton.Comp.OnClicked += OnXButton;
            // _musicToggle.Comp.ValueChangedEvent += OnMusicToggle;
            _sfxToggle.Comp.ValueChangedEvent += OnSfxToggle;
            _vibrationToggle.Comp.ValueChangedEvent += OnVibrationToggle;
            
            _privacyButton.Comp.OnClicked += OnPrivacyButton;
            _restoreButton.Comp.OnClicked += OnRestoreButton;
            _creditButton.Comp.OnClicked += OnCreditsButton;
            
            _quitButton.GameObject.SetActive(false);
        }

        protected override void InnateOnShowStart()
        {
            // _musicToggle.Comp.ToggleValueNoEvent = GM.Instance.Get<GameSaveManager>().GetPlayerPreference_MusicVolume() > 0;
            _sfxToggle.Comp.ToggleValueNoEvent = GM.Instance.Get<GameSaveManager>().GetPlayerPreference_SfxVolume() > 0;
            _vibrationToggle.Comp.ToggleValueNoEvent = GM.Instance.Get<GameSaveManager>().GetPlayerPreference_Vibration();

            _version.Comp.Text = $"Version {Application.version}";
            
            base.InnateOnShowStart();
        }

        protected override void InnateOnHideStart()
        {
            GM.Instance.Get<GameSaveManager>().SavePlayerPreference();
            base.InnateOnHideStart();
        }

        protected override void InnateOnHideEnd()
        {
            _quitButton.GameObject.SetActive(false);
            base.InnateOnHideEnd();
        }

        public void ShowQuitButton()
        {
            _quitButton.GameObject.SetActive(true);
        }

        public void OnQuitButton()
        {
            GM.Instance.Get<MainGameManager>().TransitOut();
        }

        private void OnXButton()
        {
            Popup.Hide();
        }
        
        private void OnRestoreButton()
        {
            GM.Instance.Get<PurchaseManager>().RestorePurchases();
        }

        private void OnPrivacyButton()
        {
            
        }
        
        private void OnCreditsButton()
        {
            GM.Instance.Get<PopupManager>().GetPopup("popup_credits")?.Show();
        }

        public void OnMusicToggle(bool value)
        {
            GM.Instance.Get<GameSaveManager>().SetPlayerPreference_MusicVolume(value ? 50 : 0);
            // AudioManager.MusicVolume = value ? 0.5f : 0f;
        }

        public void OnSfxToggle(bool value)
        {
            GM.Instance.Get<GameSaveManager>().SetPlayerPreference_MusicVolume(value ? 50 : 0);
            GM.Instance.Get<GameSaveManager>().SetPlayerPreference_SfxVolume(value ? 50 : 0);
            // AudioManager.SoundVolume = value ? 0.5f : 0f;
        }

        public void OnVibrationToggle(bool value)
        {
            GM.Instance.Get<GameSaveManager>().SetPlayerPreference_Vibration(value);
            // TODO
        }
    }
}