using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PurchaseButton : MonoBehaviour
    {
        [SerializeField] private string _itemId = "";
        
        [SerializeField] private CompWrapper<TextLocalizer> _nameText;
        [SerializeField] private CompWrapper<TextLocalizer> _descText;
        [SerializeField] private CompWrapper<TextLocalizer> _priceText;
        [SerializeField] private CompWrapper<UIButton> _buyButton;
        [SerializeField] private GOWrapper _boughtTag;
        [SerializeField] private GOWrapper _currencyIcon;

        private ProductEntry _entry;
        
        public string ItemId => _itemId;

        private void Awake()
        {
            if (_buyButton.NullableComp != null) _buyButton.Comp.OnClicked += OnBuy;
        }

        private void OnEnable()
        {
            SetProduct(_itemId);

            if (_entry is null || _entry.IsIAP) return;
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            if (accessor.HasData)
            {
                EvaluateBuyButton(accessor.GetFromResources(_entry.Id) ?? 0);
            }
            accessor.ResourcesChangedEvent += OnResourceChange;
        }
        
        private void OnDisable()
        {
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            accessor.ResourcesChangedEvent -= OnResourceChange;
        }
        
        private void OnResourceChange(object sender, (string key, bool isRemoved, int item) e)
        {
            if (e.key != _entry.Currency) return;
            EvaluateBuyButton(e.item);
        }

        private void EvaluateBuyButton(int currentValue)
        {
            if (_buyButton.NullableComp == null || _entry == null) return;
            _buyButton.Comp.Interactable = currentValue >= _entry.Price;
        }

        public void SetProduct(string id)
        {
            // TODO: Fix this, Add IAP
            try
            {
                _entry = GM.Instance.Get<GameDataManager>().GetProductEntry(id);
            }
            catch (Exception e)
            {
                // LogObj.Default.Error($"Cannot find product entry \"{id}\"");
            }

            if (_entry == null)
            {
                if (_boughtTag.NullableComp != null) _boughtTag.SetActive(false);
                if (_nameText.NullableComp != null) _nameText.Comp.Text = "N/A";
                if (_descText.NullableComp != null) _descText.Comp.Text = "N/A";
                if (_priceText.NullableComp != null) _priceText.Comp.Text = "???";
                return;
            }

            _itemId = id;

            var owned = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromOwnerships(id) ?? false;
            var ownership = !_entry.IsConsumable && owned;
            
            if (_boughtTag.NullableComp != null)
            {
                _boughtTag.SetActive(ownership);
            }
            
            if (_nameText.NullableComp != null) _nameText.Comp.Text = _entry.ListingName;
            if (_descText.NullableComp != null) _descText.Comp.Text = _entry.Description;
            if (_priceText.NullableComp != null) _priceText.Comp.Text  = _entry.Price.ToString();
            if (_currencyIcon.NullableComp != null) _currencyIcon.SetActive(true);
        }
        
        public void OnBuy()
        {
            if (_entry == null) return;
            
            PurchaseManager.Purchase(_entry);
        }
    }
}