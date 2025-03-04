using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using JSAM;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(Toggle))]
    public class UISwitch : MonoBehaviour
    {
        [SerializeField] private CompWrapper<TextLocalizer> _label = "./Label";
        [SerializeField] private GOWrapper _onObject = "./OffObject";
        [SerializeField] private GOWrapper _offObject = "./OnObject";
        [SerializeField] private CompWrapper<CompPlayable> _offPlayable = "./OffPlayable";
        [SerializeField] private CompWrapper<CompPlayable> _onPlayable = "./OnPlayable";

        [SerializeField] private EventWrapper<bool> _valueChangedEvent;
        
        private Toggle _unityToggle;
        
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
            _offObject.GameObject?.SetActive(!isOn);
            _onObject.GameObject?.SetActive(isOn);

            if (isOn)
            {
                _onPlayable.Comp?.Play(null);
            }
            else
            {
                _offPlayable.Comp?.Play(null);
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
