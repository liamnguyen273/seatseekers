using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    public class PopupBehaviourGeneric : PopupBehaviour
    {
        [Header("Components")] [SerializeField] private CompWrapper<PanelGeneric> _panel = "./Panel";

        public PanelGeneric GetPanel()
        {
            return _panel.Comp.Renew();
        }

        public void SetupAsNotify(
            LocalizableText title, 
            LocalizableText content, 
            Sprite image = null, 
            LocalizableText? buttonLabel = null,
            Action onButtonCallback = null,
            bool buttonClosePanel = true)
        {
            var panel = GetPanel()
                .SetTitle(title)
                .SetContentText(content)
                .AddButton(buttonLabel ?? "OK", () =>
                {
                    onButtonCallback?.Invoke();
                    if (buttonClosePanel) Popup.Hide();
                });
            
            if (image != null) panel.SetContentImage(image);
        }
        
        public void SetupAsYesNoQuestion(
            LocalizableText title, 
            LocalizableText content,
            Sprite image = null, 
            LocalizableText? yesButtonLabel = null,
            LocalizableText? noButtonLabel = null,
            Action yesButtonCallback = null,
            bool yesClosePanel = true,
            Action noButtonCallback = null,
            bool noClosePanel = true)
        {
            var panel = GetPanel()
                .SetTitle(title)
                .SetContentText(content)
                .AddButton(yesButtonLabel ?? "Yes", () =>
                {
                    yesButtonCallback?.Invoke();
                    if (yesClosePanel) Popup.Hide();
                })
                .AddButton(noButtonLabel ?? "No", () =>
                {
                    noButtonCallback?.Invoke();
                    if (noClosePanel) Popup.Hide();
                });
            
            if (image != null) panel.SetContentImage(image);
        }
        
        protected override void InnateOnHideEnd()
        {
            _panel.Comp.Renew();
        }
    }
}