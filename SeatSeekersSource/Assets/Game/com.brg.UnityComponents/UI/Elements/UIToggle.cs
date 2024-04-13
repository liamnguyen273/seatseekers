using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using JSAM;
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

        public EventWrapper<bool> ValueChangedEvent
        {
            get => _valueChangedEvent;
            set => _valueChangedEvent = value;
        }
        
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

        public ToggleGroup Group
        {
            get => UnityToggle.group;
            set => UnityToggle.group = value;
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
            if (_checkMark.NullableComp != null)
            {
                _checkMark.NullableComp.color = isOn ? _baseColor : Color.clear;
            }

            if (isOn)
            {
                _clickOnPlayable.NullableComp?.Play(null);
            }
            else
            {
                _clickOffPlayable.NullableComp?.Play(null);
            }
        }

        private void OnToggleValue(bool isOn)
        {
            AudioManager.PlaySound(AudioLibrarySounds.sfx_shift, gameObject.transform);
            
            SetToggleObject(isOn);
            _valueChangedEvent?.Invoke(isOn);
        }
    }
}
