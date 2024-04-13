using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;
using UnityEngine.Purchasing;

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
            var data = GM.Instance.Get<GameDataManager>();
            if (!data.Initialized) return;
            
            ResolveEntry(_itemId);
            RefreshProduct();
        }
        
        private void OnDisable()
        {
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            accessor.ResourcesChangedEvent -= OnResourceChange;
            UnityGM.Instance.Purchase.ProductMetadataAccessibleEvent -= OnGetMetadata;
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

        private void ForceSetBuyButton(bool interactable)
        {
            if (_buyButton.NullableComp == null || _entry == null) return;
            _buyButton.Comp.Interactable = interactable;
        }

        public void SetProduct(string id)
        {
            ResolveEntry(id);
            RefreshProduct();
        }

        private void ResolveEntry(string id)
        {
            _entry = GM.Instance.Get<GameDataManager>().GetProductEntry(id);
            if (_entry != null) _itemId = id;
        }

        private void RefreshProduct()
        {
            if (_entry == null)
            {
                if (_boughtTag.NullableComp != null) _boughtTag.SetActive(false);
                if (_nameText.NullableComp != null) _nameText.Comp.Text = "N/A";
                if (_descText.NullableComp != null) _descText.Comp.Text = "N/A";
                if (_priceText.NullableComp != null) _priceText.Comp.Text = "???";
                return;
            }

            if (_entry.IsIAP)
            {
                RefreshIAP();
            }
            else
            {
                RefreshNormal();
            }
        }

        private void RefreshIAP()
        {
            var owned = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromOwnerships(_itemId) ?? false;
            var ownership = !_entry.IsConsumable && owned;
            
            if (_nameText.NullableComp != null) _nameText.Comp.Text = _entry.ListingName;
            if (_descText.NullableComp != null) _descText.Comp.Text = _entry.Description;
            if (_priceText.NullableComp != null) _priceText.Comp.Text  = "...";
            if (_currencyIcon.NullableComp != null) _currencyIcon.SetActive(ownership);

            if (ownership)
            {
                if (_priceText.NullableComp != null) _priceText.Comp.Text  = "Bought!";
                ForceSetBuyButton(false);
            }
            else
            {
                if (UnityGM.Instance.Purchase.IapUsable)
                {
                    OnGetMetadata(null, EventArgs.Empty);
                }
                else
                {
                    ForceSetBuyButton(false);
                    UnityGM.Instance.Purchase.ProductMetadataAccessibleEvent += OnGetMetadata;
                }
            }
        }

        private void OnGetMetadata(object sender, EventArgs e)
        {
            var metadata = UnityGM.Instance.Purchase.GetProductMetaData(_itemId);
            if (_priceText.NullableComp != null) _priceText.Comp.Text  = $"{metadata.localizedPrice}";
        }

        private void RefreshNormal()
        {
            var owned = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromOwnerships(_itemId) ?? false;
            var ownership = !_entry.IsConsumable && owned;
            
            if (_boughtTag.NullableComp != null)
            {
                _boughtTag.SetActive(ownership);
            }
            
            if (_nameText.NullableComp != null) _nameText.Comp.Text = _entry.ListingName;
            if (_descText.NullableComp != null) _descText.Comp.Text = _entry.Description;
            if (_priceText.NullableComp != null) _priceText.Comp.Text  = _entry.Price.ToString();
            if (_currencyIcon.NullableComp != null) _currencyIcon.SetActive(true);
            
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            if (accessor.HasData)
            {
                EvaluateBuyButton(accessor.GetFromResources(_entry.Currency) ?? 0);
            }
            accessor.ResourcesChangedEvent += OnResourceChange;
        }
        
        public void OnBuy()
        {
            if (_entry == null) return;
            
            UnityGM.Instance.Purchase.Purchase(_entry);
        }
    }
}