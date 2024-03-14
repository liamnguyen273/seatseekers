using JSAM;
using com.brg.Common.Localization;
using com.brg.UnityCommon.Editor;
using com.brg.UnityCommon.Player;
using Lean.Gui;
using UnityEngine;

namespace com.brg.UnityCommon.UI
{
    public class PopupBehaviourSettings : UIPopupBehaviour
    {
        [Header("Components")]
        [SerializeField] private CompWrapper<TextLocalizer> _version = "./Panel/Content/Version";
        [SerializeField] private CompWrapper<LeanToggle> _musicToggle = "./Panel/Content/MusicToggler";
        [SerializeField] private CompWrapper<LeanToggle> _sfxToggle = "./Panel/Content/SFXToggler";
        // [SerializeField] private LeanToggle _vibrationToggle;
        [SerializeField] private CompWrapper<UIButton> _quitButton = "./Panel/ButtonGroup/Button2";

        private PlayerPreference _cachedPref;
        
        protected override void InnateOnShowStart()
        {
            var pref = GM.Instance.Player.GetPreference();
            _cachedPref = pref;

            _musicToggle.Comp.Set(pref.MusicVolume > 0);
            _sfxToggle.Comp.Set(pref.SfxVolume > 0);
            // _vibrationToggle.Set(pref.Vibration);

            _version.Comp.Text = $"Version {Application.version}";
            
            base.InnateOnShowStart();
        }

        protected override void InnateOnHideStart()
        {
            GM.Instance.Player.SetPreference(_cachedPref);
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
            Popup.Hide();
            // GM.Instance.RequestGoToMenu();
        }

        public void OnCreditsButton()
        {
            GM.Instance.Popups.GetPopup(PopupNames.CREDITS).Show();
        }

        public void OnMusicToggle(bool value)
        {
            _cachedPref.MusicVolume = value ? 50 : 0;
            AudioManager.MusicVolume = _cachedPref.MusicVolume > 0 ? 0.5f : 0f;
        }

        public void OnSfxToggle(bool value)
        {
            _cachedPref.SfxVolume = value ? 50 : 0;
            AudioManager.SoundVolume = _cachedPref.SfxVolume > 0 ? 0.5f : 0f;
        }

        public void OnVibrationToggle(bool value)
        {
            _cachedPref.Vibration = value;
        }
    }
}