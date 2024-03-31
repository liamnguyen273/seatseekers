using System;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public class PanelGeneric : MonoBehaviour
    {
        [Header("Title Group")]
        [SerializeField] private CompWrapper<TextLocalizer> _titleText = "./TitleGroup/TextHeading";
        [SerializeField] private CompWrapper<Image> _leftTitleIcon = "./TitleGroup/LeftIconGroup/Icon";
        [SerializeField] private CompWrapper<UIButton> _rightTitleButton = "./TitleGroup/RightButtonGroup/Button";

        [Header("Content Group")] 
        [SerializeField] private CompWrapper<Image> _contentImage = "./CenterGroup/ContentImage";
        [SerializeField] private CompWrapper<TextLocalizer> _contentText = "./CenterGroup/ContentText";
        
        [Header("Bottom Group")] 
        [SerializeField] private CompWrapper<UIButton>[] _bottomButtons;

        public PanelGeneric Renew()
        {
            _titleText.Comp.SetGOActive(false);
            _leftTitleIcon.Comp.SetGOActive(false);
            _rightTitleButton.Comp.SetGOActive(false);
            _rightTitleButton.Comp.OnClicked.ClearEvents();
            
            _contentImage.Comp.SetGOActive(false);
            _contentText.Comp.SetGOActive(false);
            foreach (var button in _bottomButtons)
            {
                button.Comp.SetGOActive(false);
                button.Comp.OnClicked.ClearEvents();
            }

            return this;
        }

        public PanelGeneric SetTitle(LocalizableText text)
        {
            _titleText.Comp.SetGOActive(true);
            _titleText.Comp.Text = text;
            return this;
        }
        
        public PanelGeneric SetLeftTitleIcon(Sprite sprite)
        {
            _leftTitleIcon.Comp.SetGOActive(true);
            _leftTitleIcon.Comp.sprite = sprite;
            return this;
        }        
        
        public PanelGeneric SetRightTitleButton(Sprite sprite, Action callback)
        {
            _rightTitleButton.Comp.SetGOActive(true);
            _rightTitleButton.Comp.Icon = sprite;
            _rightTitleButton.Comp.OnClicked += callback;
            return this;
        }      
        
        public PanelGeneric SetContentImage(Sprite sprite)
        {
            _contentImage.Comp.SetGOActive(true);
            _contentImage.Comp.sprite = sprite;
            return this;
        }        
        
        public PanelGeneric SetContentText(LocalizableText text)
        {
            _contentText.Comp.SetGOActive(true);
            _contentText.Comp.Text = text;
            return this;
        }        
        
        public PanelGeneric AddButton(LocalizableText label, Action callback)
        {
            var turnedOn = false;
            foreach (var button in _bottomButtons)
            {
                if (!button.GameObject.activeSelf)
                {
                    button.Comp.SetGOActive(true);
                    button.Comp.Label = label;
                    button.Comp.OnClicked += callback;
                    turnedOn = true;
                    break;
                }
            }

            if (!turnedOn)
            {
                LogObj.Default.Warn($"Panel {name} already has all buttons set. Cannot add another.");
            }
            
            return this;
        }
    }
}