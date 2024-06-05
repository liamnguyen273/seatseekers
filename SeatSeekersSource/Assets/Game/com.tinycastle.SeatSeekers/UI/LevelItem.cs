using System;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class LevelItem : MonoBehaviour
    {
        [SerializeField] private CompWrapper<TextLocalizer> _levelNumber = "./";
        [SerializeField] private GOWrapper _levelUnlocked = "./";
        [SerializeField] private GOWrapper _levelLocked = "./";
        [SerializeField] private GOWrapper _currentLevel = "./";
        [SerializeField] private CompWrapper<UIButton> _button = "./";

        private LevelEntry _entry;
        private bool _isUnlocked;
        private bool _isCurrent;

        public LevelEntry Entry => _entry;

        private void Awake()
        {
            _button.Comp.OnClicked += OnButtonSelect;
        }
        
        private void OnDestroy()
        {
            _button.Comp.OnClicked -= OnButtonSelect;
        }

        public void SetLevel(LevelEntry levelEntry, bool isUnlocked, bool isCurrent)
        {
#if UNITY_EDITOR
            isUnlocked = true;
#endif
            
            _entry = levelEntry;
            _isCurrent = isCurrent;
            _isUnlocked = isUnlocked;
            
            _button.Comp.Interactable = isUnlocked;
            _levelUnlocked.GameObject.SetActive(isUnlocked);
            _levelLocked.GameObject.SetActive(!isUnlocked);
            _currentLevel.GameObject.SetActive(isCurrent);
            _levelNumber.Comp.Text = $"{_entry.SortOrder}";
        }

        private void OnButtonSelect()
        {
            if (!_isUnlocked || _entry == null) return;

            var aa = false;
            GM.Instance.RequestPlayLevelWithValidation(_entry, ref aa);
        }
    }
}