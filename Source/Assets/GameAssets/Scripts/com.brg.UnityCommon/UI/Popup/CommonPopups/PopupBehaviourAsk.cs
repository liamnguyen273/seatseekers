using System;
using com.brg.Common.Localization;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityCommon.UI
{
    public class PopupBehaviourAsk : UIPopupBehaviour
    {
        private Action? _yesAction;
        private Action? _noAction;

        [Header("Components")]
        [SerializeField] private CompWrapper<Image> _image = "./Panel/Content/Image";
        [SerializeField] private CompWrapper<UIButton> _yesButton = "./Panel/ButtonGroup/ButtonYes";
        [SerializeField] private CompWrapper<UIButton> _noButton = "./Panel/ButtonGroup/ButtonNo";
        [SerializeField] private CompWrapper<TextLocalizer> _title = "./Panel/TitleGroup/Title";
        [SerializeField] private CompWrapper<TextLocalizer> _content = "./Panel/Content/ContentText";

        internal override void Initialize()
        {
            base.Initialize();

            _yesButton.Comp.Event.FunctionalEvent += OnYesButton;
            _noButton.Comp.Event.FunctionalEvent += OnNoButton;
        }

        public PopupBehaviourAsk SetImage(Sprite image)
        {
            _image.Comp.sprite = image;
            return this;
        }

        public PopupBehaviourAsk SetQuestion(string title, string content, Action onYes, Action onNo)
        {
            _title.Comp.Text = title;
            _content.Comp.Text = content;
            _yesAction = onYes;
            _noAction = onNo;
            return this;
        }

        public void OnYesButton()
        {
            Popup.Hide();
            _yesAction?.Invoke();
        }

        public void OnNoButton()
        {
            Popup.Hide();
            _noAction?.Invoke();
        }
    }
}