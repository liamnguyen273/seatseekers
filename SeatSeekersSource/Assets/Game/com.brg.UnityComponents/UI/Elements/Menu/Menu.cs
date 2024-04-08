using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace com.brg.UnityComponents
{
    public interface IMenu
    {
        public bool IsShown { get; }
        public void Show();
        public void ShowImmediately();
        public void Hide();       
        public void HideImmediately();
        public void SelectOption(string id);
        public void SelectOption(string category, string id);    
        public void DeselectOption(string id);
        public void DeselectOption(string category, string id);
    }
    
    public abstract class Menu<TOption, TItem> : 
            MonoBehaviour,
            IPointerClickHandler,
            ISubmitHandler, 
            ICancelHandler, 
            IComparer<string>,
            IMenu
        where TItem : MenuItem<TOption> 
        where TOption : class, IMenuOption
    {
        private readonly Dictionary<string, TItem> _itemMap = new();
        private readonly Dictionary<string, TOption> _optionMap = new();
        
        private List<(TOption option, TItem item)> _sortedOptionsAndItems;

        private TOption _currentOption = null;

        protected List<(TOption option, TItem item)> SortedOptionsAndItems => _sortedOptionsAndItems;

        [Header("Events")] 
        [SerializeField] public EventWrapper<TOption> OptionSelectedEvent;
        [SerializeField] public EventWrapper<TOption> OptionDeselectedEvent;
        [SerializeField] public EventWrapper<TItem> ItemSelectedEvent;
        [SerializeField] public EventWrapper<TItem> ItemDeselectedEvent;

        public virtual bool IsShown => gameObject.activeSelf;

        protected virtual void Awake()
        {
            // Do nothing?
        }

        public void SelectOption(string id)
        {
            if (!FindOption(id, out var option))
            {
                LogObj.Default.Warn("Menu", $"Cannot set option with id \"{id}\" because it is not" +
                                            $"found in the menu.");
                return;
            }
            
            SetCurrentOption(option, true);
        }

        public void SelectOption(string category, string id)
        {
            if (!FindOption(category, id, out var option))
            {
                LogObj.Default.Warn("Menu", $"Cannot set option with id \"{id}\" in category \"{category}\"" +
                                            $"because it is not found in the menu.");
                return;
            }
            
            SetCurrentOption(option, true);
        }

        public void SelectWithoutNotify(string id)
        {
            if (!FindOption(id ,out var option))
            {
                LogObj.Default.Warn("Menu", $"Cannot set option with id \"{id}\" because it is not" +
                                            $"found in the menu.");
                return;
            }

            SetCurrentOption(option, false);
        }
        
        public void SelectWithoutNotify(string category, string id)
        {
            if (!FindOption(category, id, out var option))
            {
                LogObj.Default.Warn("Menu", $"Cannot set option with id \"{id}\" in category \"{category}\"" +
                                            $"because it is not found in the menu.");
                return;
            }
            
            SetCurrentOption(option, false);
        }
        
        public void DeselectOption(string id)
        {
            if (!GetAllowDeselection()) return;
            if (_currentOption != null && _currentOption.Id == id)
            {
                DeselectCurrent();
            }
        }        
        
        public void DeselectOption(string category, string id)
        {
            if (!GetAllowDeselection()) return;            
            if (_currentOption != null && 
                _currentOption.Category == category &&
                _currentOption.Id == id)
            {
                DeselectCurrent();
            }
        }

        public void AddOptions(List<TOption> optionList)
        {
            if (optionList is null || optionList.Count == 0) return;

            foreach (var option in optionList)
            {
                var menuId = option.GetIdInMenu();

                if (_optionMap.ContainsKey(menuId))
                {
                    LogObj.Default.Warn("Menu",
                        $"Option with id \"{option.Id}\" in category \"{option.Category}\" already exists in menu," +
                        $"will be overriden with new option of the same id.");
                }

                _optionMap[menuId] = option;
                var newItem = InstantiateItem();
                newItem.gameObject.name = $"Option ({menuId})";

                if (_itemMap.Remove(menuId, out var item))
                {
                    DestroyItem(item);
                }

                _itemMap[menuId] = newItem;
            }

            SortOptions();
            RebuildMenu();
        }

        public void ClearOptions()
        {
            _optionMap.Clear();
            var values = _itemMap.Values.ToList();
            _itemMap.Clear();
            foreach (var item in _itemMap.Values)
            {
                DestroyItem(item);
            }
            
            _sortedOptionsAndItems.Clear();
            RebuildMenu();
        }

        private void DeselectCurrent()
        {
            if (_currentOption != null)
            {
                var oldItem = _itemMap[_currentOption.GetIdInMenu()];
                oldItem.OnDeselectedInMenu();
                _currentOption = null;
            }
        }
        
        protected virtual void SetCurrentOption(TOption option, bool invokeCallbacks)
        {
            DeselectCurrent();
            
            _currentOption = option;
            var item = _itemMap[option.GetIdInMenu()];

            if (!invokeCallbacks) return;
            
            OptionSelectedEvent?.Invoke(option);
            ItemSelectedEvent?.Invoke(item);
            
            item.OnSelectedInMenu();
        }

        protected virtual void SortOptions()
        {
            var options = _optionMap.Values
                .OrderBy(x => x.Category)
                .ThenBy(x => x.DesiredOrder)
                .ThenBy(x => x.Id);

            _sortedOptionsAndItems = options.Select(option =>
            {
                var item = _itemMap[option.GetIdInMenu()];
                return (option, item);
            }).ToList();
        }

        protected abstract void RebuildMenu();
        protected abstract TItem InstantiateItem();
        protected abstract void DestroyItem(TItem item);
        protected abstract int CompareCategory(string x, string y);
        protected abstract bool GetAllowDeselection();
        public abstract void Show();
        public abstract void ShowImmediately();
        public abstract void Hide();
        public abstract void HideImmediately();

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Show();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Show();
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            Hide();
        }

        public int Compare(string x, string y)
        {
            return CompareCategory(x, y);
        }
        
        private bool FindOption(string id, out TOption option)
        {
            var index = _sortedOptionsAndItems.FindIndex(x => x.Item1.Id == id);
            if (index < 0)
            {
                option = null;
                return false;
            }
            
            option = _sortedOptionsAndItems[index].Item1;
            return true;
        }

        private bool FindOption(string category, string id, out TOption option)
        {
            var menuId = $"{category}_{id}";
            return _optionMap.TryGetValue(menuId, out option);
        }

        private class OptionComparer : Comparer<IMenuOption>
        {
            private static OptionComparer _default;
            public static OptionComparer Default => _default ??= new OptionComparer();

            private readonly Comparer<string> _stringComparer = Comparer<string>.Default;
            
            public override int Compare(IMenuOption x, IMenuOption y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return 1;
                if (y is null) return -1;

                var si = _stringComparer.Compare(x.Category, y.Category);
                if (si != 0) return si;
                var di = x.DesiredOrder - y.DesiredOrder;
                if (di != 0) return di;
                return _stringComparer.Compare(x.Id, y.Id);
            } 
        }
    }
}