using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public class ClassicButtonMenuItem : MenuItem<ClassicMenuOption>
    {
        [SerializeField] private TextLocalizer _text;
        [SerializeField] private Image _iconImage;
        [SerializeField] private UIButton _button;
        
        public override void AttachOption(IMenu targetMenu, ClassicMenuOption option)
        {
            base.AttachOption(targetMenu, option);

            _iconImage.sprite = option.Icon;
            _text.Text = option.DisplayStringKey;
            _button.OnClicked += OnClick;
        }

        private void OnClick()
        {
            TargetMenu?.SelectOption(Option.Category, Option.Id);
        }
    }

    [Serializable]
    public class ClassicMenuOption : IMenuOption
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayStringKey;
        [SerializeField] private Sprite _icon;

        public string Category { get; set; }
        public int DesiredOrder { get; set; }

        public string Id
        {
            get => _id;
            set => _id = value;
        }
        
        public string DisplayStringKey => _displayStringKey;
        public Sprite Icon => _icon;

        public ClassicMenuOption(
            int desiredOrder, 
            string id,
            string category = "default", 
            string displayStringKey = "",
            Sprite icon = null)
        {
            Category = category;
            DesiredOrder = desiredOrder;
            _id = id;
            _displayStringKey = displayStringKey;
            _icon = icon;
        }
    }
}