using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public class PopupBehaviourAsk : PopupBehaviour
    {
        private Action _yesAction;
        private Action _noAction;

        [Header("Components")]
        [SerializeField] private CompWrapper<Image> _image = "./Panel/Content/Image";
        [SerializeField] private CompWrapper<UIButton> _yesButton = "./Panel/ButtonGroup/ButtonYes";
        [SerializeField] private CompWrapper<UIButton> _noButton = "./Panel/ButtonGroup/ButtonNo";
        [SerializeField] private CompWrapper<TextLocalizer> _title = "./Panel/TitleGroup/Title";
        [SerializeField] private CompWrapper<TextLocalizer> _content = "./Panel/Content/ContentText";

        public override IProgress Initialize()
        {
            _yesButton.Comp.OnClicked.FunctionalEvent += OnYesButton;
            _noButton.Comp.OnClicked.FunctionalEvent += OnNoButton;
            return base.Initialize();
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