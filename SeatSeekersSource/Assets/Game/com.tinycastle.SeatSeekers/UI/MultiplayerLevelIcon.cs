using System;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class MultiplayerLevelIcon : MonoBehaviour
    {
        [SerializeField] private GOWrapper _lockIcon = "./LockIcon";
        [SerializeField] private GOWrapper _priceGroup = "./PriceGroup";
        [SerializeField] private CompWrapper<UIButton> _button = "./";
        [SerializeField] private CompWrapper<TextLocalizer> _priceText = "./Layout/PriceText";
        [SerializeField] private int _price;
        [SerializeField] private int _intensity;
        [SerializeField] private int _levelThreshold;
        
        private void Awake()
        {
            if (_button.NullableComp != null) _button.Comp.OnClicked += OnPlay;

            _priceText.Comp.Text = _price.ToString();
        }

        private void OnEnable()
        {
            var data = GM.Instance.Get<GameDataManager>();
            if (!data.Initialized) return;
            
            var unlocked = CheckUnlocked();
            _lockIcon.SetActive(!unlocked);
            _priceGroup.SetActive(unlocked);
            
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            OnResourceChange(null, (Constants.COIN_RESOURCE, false, accessor.GetFromResources(Constants.COIN_RESOURCE) ?? 0));
            accessor.ResourcesChangedEvent += OnResourceChange;

            _priceText.Comp.Text = _price.ToString();
        }
        
        private void OnDisable()
        {
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            accessor.ResourcesChangedEvent -= OnResourceChange;
        }

        private void OnResourceChange(object sender, (string key, bool isRemoved, int item) e)
        {
            var total = e.item;

            _button.Comp.Interactable = total >= _price && CheckUnlocked();
        }

        private void OnPlay()
        {
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            var coin = accessor.GetFromResources(Constants.COIN_RESOURCE) ?? 0;
            
            if (coin < _price) return;
            
            coin -= _price;
            
            accessor.SetInResources(Constants.COIN_RESOURCE, coin, true);
            accessor.WriteDataAsync();
            
            GM.Instance.RequestPlayMultiplayer(_intensity);
        }

        private bool CheckUnlocked()
        {
            // return true;
            var player = GM.Instance.Get<GameSaveManager>().PlayerData;
            return player.GetFromCompletedLevels($"level_{_levelThreshold}") is true;
        }
    }
}