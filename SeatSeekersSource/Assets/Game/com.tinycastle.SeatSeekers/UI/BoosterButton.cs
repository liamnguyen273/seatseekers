using System;
using com.brg.Unity;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class BoosterButton : MonoBehaviour
    {
        [Header("Groups")]
        [SerializeField] private GOWrapper _lockedGroup = "./Lock";
        [SerializeField] private GOWrapper _countGroup = "./Count";
        [SerializeField] private GOWrapper _addGroup = "./AddButton";
        
        [Header("Comps")]
        [SerializeField] private CompWrapper<UIToggle> _button = "./";
        [SerializeField] private CompWrapper<UIButton> _addButton = "./AddButton";
        [SerializeField] private CompWrapper<TextLocalizer> _countText = "./Count/BoosterCount";

        [SerializeField] private string _boosterName;
        
        private static PlayerDataAccessor Accessor => GM.Instance.Get<GameSaveManager>().PlayerData;

        private int _cachedValue = 0;
        private bool _unlocked = false;

        private void Awake()
        {
            _button.Comp.ValueChangedEvent += OnValueChanged;
            _addButton.Comp.OnClicked += OnAddClick;
        }
        

        public void OnLevel(int level)
        {
            var unlocked = Accessor.CheckUnlockedBooster(level, _boosterName, out var shouldIntroduce);

            _unlocked = unlocked;
            
            _lockedGroup.NullableComp?.SetActive(!unlocked);
            _countGroup.NullableComp?.SetActive(unlocked);
            _addGroup.NullableComp?.SetActive(unlocked);
            _button.Comp.Interactable = unlocked;
            
            Refresh(Accessor.GetFromResources(_boosterName) ?? 0);

            if (!shouldIntroduce) return;
            var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupBoosterUnlock>(out var behaviour);
            behaviour.Setup(_boosterName);
            popup.Show();
        }

        public void ForceBoosterOff()
        {
            _button.Comp.ToggleValueNoEvent = false;
        }

        private void OnEnable()
        {
            Accessor.ResourcesChangedEvent += OnResourceChange;

            if (!Accessor.HasData) return;

            Refresh(Accessor.GetFromResources(_boosterName) ?? 0);
        }
        
        private void OnDisable()
        {
            Accessor.ResourcesChangedEvent -= OnResourceChange;
        }

        private void OnResourceChange(object sender, (string key, bool isRemoved, int item) e)
        {
            if (e.key != _boosterName) return;
            Refresh(e.item);
        }

        public void ToggleOn()
        {
            _button.Comp.ToggleValue = true;
        }

        public void ToggleOff()
        {
            _button.Comp.ToggleValue = false;
        }
        
        private void OnValueChanged(bool selected)
        {
            if (selected)
            {
                if (_cachedValue > 0)
                {
                    var used = GM.Instance.Get<MainGameManager>().OnBoosterOn(_boosterName);

                    if (used)
                    {
                        var value = Accessor.GetFromResources(_boosterName) ?? 0;
                        value = Math.Max(0, value - 1);
                        Accessor.SetInResources(_boosterName, value, true);
                        GM.Instance.Get<GameSaveManager>().SaveAll();
                    }
                    else
                    {
                        _button.Comp.ToggleValueNoEvent = false;
                    }
                }
                else
                {
                    _button.Comp.ToggleValueNoEvent = false;
                    OnAddClick();
                }
            }
            else
            {
                var allowed = GM.Instance.Get<MainGameManager>().OnBoosterOff(_boosterName);
                if (!allowed)
                {
                    _button.Comp.ToggleValueNoEvent = true;
                }
            }
        }
        
        private void OnAddClick()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupBoosterAdd>(out var behaviour);
            behaviour.Setup(_boosterName);
            popup.Show();
        }
        
        private void Refresh(int count)
        {
            _cachedValue = count;

            if (_unlocked)
            {
                _countGroup.SetActive(_unlocked && count > 0);
                _countText.Comp.Text = count.ToString();
            }
        }
    }
}