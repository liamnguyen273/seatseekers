using System;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CompWrapper<TextLocalizer> _label = "./Label";
        [SerializeField] private CompWrapper<Image> _icon = "./Icon";
        [SerializeField] private CompWrapper<CompPlayable> _clickPlayable = "./Playable";
        
        [SerializeField] private EventWrapper _clickedEvent;

        private Button _unityButton;
        
        public LocalizableText Label
        {
            get => _label.Comp.CloneText();
            set
            {
                if (_label != null)
                {
                    _label.Comp.Text = value;
                }
                else
                {
                    LogObj.Default.Warn($"Cannot set label for button {name}, Label text is null.");
                }
            }
        }

        public Sprite Icon
        {
            get => _icon.Comp.sprite;
            set
            {
                if (_icon != null)
                {
                    _icon.Comp.sprite = value;
                }
                else
                {
                    LogObj.Default.Warn($"Cannot set icon for button {name}, Icon Image is null.");
                }
            }
        }
        
        public bool Interactable
        {
            get => UnityButton.interactable;
            set => UnityButton.interactable = value;
        }

        public Button UnityButton
        {
            get
            {
                if (_unityButton is null)
                {
                    _unityButton = GetComponent<Button>();
                    _unityButton.onClick.AddListener(OnUnityButtonClick);

                    if (_clickPlayable.Comp == null)
                    {
                        var comp = transform.GetComponentInChildren<CompPlayable>();
                        if (comp != null)
                        {
                            var path = comp.gameObject.RegeneratePathUpTo(gameObject);
                            _clickPlayable.Set(comp, path);
                        }
                        else
                        {
                            LogObj.Default.Warn("Cannot find any CompPlayable on the Button.");
                        }
                    }
                }

                return _unityButton;
            }
        }
        
        public Image IconImage => _icon;

        public EventWrapper OnClicked
        {
            get => _clickedEvent;
            set => _clickedEvent = value;
        }

        private void Awake()
        {
            var button = UnityButton;
        }

        private void OnUnityButtonClick()
        {
            // other things
            // AudioManager.PlaySound(_tapSound, gameObject.transform);

            _clickPlayable.Comp?.Play(null);
            _clickedEvent?.Invoke();
        }
    }
}