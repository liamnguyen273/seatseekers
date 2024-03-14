using com.brg.Common.Localization;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityCommon.UI
{
    public class PopupBehaviourNotify : UIPopupBehaviour
    {
        [Header("Components")]
        [SerializeField] private CompWrapper<Image> _image = "./Panel/Content/Image";
        [SerializeField] private CompWrapper<TextLocalizer> _title = "./Panel/TitleGroup/Title";
        [SerializeField] private CompWrapper<TextLocalizer> _content = "./Panel/Content/ContentText";

        private LocalizableText _baseTitle;
        private LocalizableText _baseContent;

        internal override void Initialize()
        {
            base.Initialize();
            _baseTitle = _title.Comp.GetContent();
            _baseContent = _content.Comp.GetContent();
        }
        
        public PopupBehaviourNotify SetImage(Sprite image)
        {
            _image.Comp.sprite = image;
            return this;
        }
        
        public PopupBehaviourNotify SetContentText(string value)
        {
            _content.Comp.Text = value;
            return this;
        }

        public PopupBehaviourNotify SetTitleText(string value)
        {
            _title.Comp.Text = value;
            return this;
        }

        protected override void InnateOnHideEnd()
        {
            _title.Comp.Text = _baseTitle;
            _content.Comp.Text = _baseContent;
        }
    }
}