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
        [SerializeField] private CompWrapper<UISwitch> _musicToggle = "./Panel/Content/MusicToggler";
        [SerializeField] private CompWrapper<UISwitch> _sfxToggle = "./Panel/Content/SFXToggler";
        [SerializeField] private CompWrapper<UISwitch> _vibrationToggle = "./Panel/Content/SFXToggler";
        
        [SerializeField] private CompWrapper<UIButton> _xButton;
        [SerializeField] private CompWrapper<UIButton> _quitButton = "./Panel/ButtonGroup/Button2";
        [SerializeField] private CompWrapper<UIButton> _okButton = "./Panel/ButtonGroup/Button2";

        private void Awake()
        {
            _quitButton.Comp.OnClicked += OnQuitButton;
            _okButton.Comp.OnClicked += OnXButton;
            _xButton.Comp.OnClicked += OnXButton;
            _musicToggle.Comp.OnValueChanged += OnMusicToggle;
            _sfxToggle.Comp.OnValueChanged += OnSfxToggle;
            _vibrationToggle.Comp.OnValueChanged += OnVibrationToggle;
            
            _quitButton.GameObject.SetActive(false);
        }

        protected override void InnateOnShowStart()
        {
            _musicToggle.Comp.ToggleValueNoEvent = GM.Instance.Get<GameSaveManager>().GetPlayerPreference_MusicVolume() > 0;
            _sfxToggle.Comp.ToggleValueNoEvent = GM.Instance.Get<GameSaveManager>().GetPlayerPreference_MusicVolume() > 0;
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
            var loading = GM.Instance.Get<LoadingScreen>();
            var mainGame = GM.Instance.Get<MainGameManager>();
            var popupManager = GM.Instance.Get<PopupManager>();
            
            loading.RequestLoad(mainGame.Deactivate(),
                () =>
                {
                    popupManager.HideAllPopups();
                    popupManager.GetPopup<PopupMainMenu>().Show();
                }, null);
        }

        public void OnCreditsButton()
        {
            
        }

        private void OnXButton()
        {
            Popup.Hide();
        }

        public void OnMusicToggle(bool value)
        {
            GM.Instance.Get<GameSaveManager>().SetPlayerPreference_MusicVolume(value ? 50 : 0);
            // AudioManager.MusicVolume = value ? 0.5f : 0f;
        }

        public void OnSfxToggle(bool value)
        {
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