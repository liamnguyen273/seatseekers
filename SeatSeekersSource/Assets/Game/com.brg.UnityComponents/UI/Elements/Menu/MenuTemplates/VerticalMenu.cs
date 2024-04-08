using System.Collections.Generic;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public class VerticalMenu<TOption, TItem> : Menu<TOption, TItem>
        where TOption : class, IMenuOption
        where TItem : MenuItem<TOption>
    {
        [Header("Panel Components")]
        [SerializeField] protected CompWrapper<RectTransform> _panel = "./Panel";
        [SerializeField] protected CompWrapper<RectTransform> _resizePanel = "./Panel";
        [SerializeField] private CompWrapper<HorizontalOrVerticalLayoutGroup> _layoutGroup = "./Panel";
        [SerializeField] private CompWrapper<RectTransform> _contentHost = "./Panel";
        [SerializeField] private GOWrapper _template = "./Panel/ItemTemplate";
        [SerializeField] private GOWrapper _categoryDivider = "./Panel/CategoryDividerTemplate";
        
        [Header("Options")] 
        [SerializeField] private List<TOption> _defaultOptions = new();

        private readonly List<GameObject> _dividers = new();

        protected override void Awake()
        {
            var i = 0;
            foreach (var option in _defaultOptions)
            {
                option.Category = "default";
                option.DesiredOrder = i;
                ++i;
            }
            AddOptions(_defaultOptions);
            
            base.Awake();
        }

        protected override void RebuildMenu()
        {
            // Clear all dividers
            foreach (var divider in _dividers)
            {
               Destroy(divider); 
            }
            _dividers.Clear();
            
            _template.GameObject.SetActive(false);
            _categoryDivider.GameObject.SetActive(false);
            
            string currentCategory = null;
            var siblingIndex = 0;
            foreach (var (option, item) in SortedOptionsAndItems)
            {
                var category = option.Category;

                if (_categoryDivider.NullableComp != null && currentCategory != null && category != currentCategory)
                {
                    // Add divider
                    var divider = Instantiate(_categoryDivider.GameObject, _contentHost);
                    divider.transform.SetSiblingIndex(siblingIndex);
                    divider.SetActive(true);
                    _dividers.Add(divider);
                    ++siblingIndex;
                }

                item.AttachOption(this, option);
                item.transform.SetSiblingIndex(siblingIndex);
                item.gameObject.SetActive(true);
                ++siblingIndex;

                currentCategory = category;
            }

            if (gameObject.activeInHierarchy)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup.Comp.GetComponent<RectTransform>());
            }
        }

        protected override TItem InstantiateItem()
        {
            var go = Instantiate(_template.GameObject, _contentHost);
            return go.GetComponent<TItem>();
        }

        protected override void DestroyItem(TItem item)
        {
            GameObject go = item.gameObject;
            go.SetActive(false);
            Destroy(go);
        }

        protected override int CompareCategory(string x, string y)
        {
            return Comparer<string>.Default.Compare(x, y);
        }

        protected override bool GetAllowDeselection()
        {
            return false;
        }

        public override void Show()
        {
            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup.Comp.GetComponent<RectTransform>());

            if (_resizePanel.NullableComp != null)
            {
                var layoutTransform = _layoutGroup.Comp.GetComponent<RectTransform>();
                var selfTransform = _resizePanel.Comp.GetComponent<RectTransform>();
                selfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layoutTransform.rect.height); 
            }
        }

        public override void ShowImmediately()
        {
            // Same as Show(), since there's no animation.
            Show();
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        public override void HideImmediately()
        {
            // Same as Hide(), since there's no animation.
            Hide();
        }
    }
}