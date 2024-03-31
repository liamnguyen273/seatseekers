using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(Toggle))]
    public class UIToggle : MonoBehaviour
    {
        [SerializeField] private CompWrapper<TextLocalizer> _label = "./Label";
        [SerializeField] private CompWrapper<Image> _checkMark = "./Icon/Checkmark";
        [SerializeField] private CompWrapper<CompPlayable> _clickOnPlayable = "./Playable";
        [SerializeField] private CompWrapper<CompPlayable> _clickOffPlayable = "./Playable";

        [SerializeField] private EventWrapper<bool> _valueChangedEvent;
        
        private Toggle _unityToggle;
        private Color _baseColor;
        
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

        public Sprite Checkmark
        {
            get => _checkMark.Comp.sprite;
            set
            {
                if (_checkMark != null)
                {
                    _checkMark.Comp.sprite = value;
                }
                else
                {
                    LogObj.Default.Warn($"Cannot set checkmark for toggle {name}, Icon Checkmark is null.");
                }
            }
        }
        
        public bool Interactable
        {
            get => UnityToggle.interactable;
            set => UnityToggle.interactable = value;
        }
        
        public Toggle UnityToggle
        {
            get
            {
                if (_unityToggle is null)
                {
                    _unityToggle = GetComponent<Toggle>();
                    _unityToggle.onValueChanged.AddListener(OnToggleValue);

                    _baseColor = _checkMark.Comp?.color ?? Color.white;
                }

                return _unityToggle;
            }
        }
        
        public bool ToggleValue
        {
            get => _unityToggle.isOn;
            set => _unityToggle.isOn = value;
        }

        public bool ToggleValueNoEvent
        {
            get => _unityToggle.isOn;
            set
            {
                _unityToggle.SetIsOnWithoutNotify(value);
                SetToggleObject(value);
            }
        }

        private void Awake()
        {
            var toggle = UnityToggle;
        }

        private void SetToggleObject(bool isOn)
        {
            if (_checkMark.Comp != null)
            {
                _checkMark.Comp.color = isOn ? _baseColor : Color.clear;
            }

            if (isOn)
            {
                _clickOnPlayable.Comp?.Play(null);
            }
            else
            {
                _clickOffPlayable.Comp?.Play(null);
            }
        }

        private void OnToggleValue(bool isOn)
        {
            SetToggleObject(isOn);
            _valueChangedEvent?.Invoke(isOn);
        }
    }
}
