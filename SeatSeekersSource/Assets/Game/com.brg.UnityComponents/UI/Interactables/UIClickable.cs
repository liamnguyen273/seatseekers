using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.brg.UnityComponents
{
    [Flags]
    public enum UIClickability
    {
        LEFT_BUTTON = 0,
        RIGHT_BUTTON = 1,
        MIDDLE_BUTTON = 2,
    }
    
    public class UIClickable : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private UIClickability _clickability;
        [SerializeField] private CompWrapper<RectTransform> _effectedArea;

        [SerializeField] private EventWrapper<PointerEventData, Vector2> _leftButtonClickEvent;
        [SerializeField] private EventWrapper<PointerEventData, Vector2> _rightButtonClickEvent;
        [SerializeField] private EventWrapper<PointerEventData, Vector2> _middleButtonClickEvent;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!EvaluateButtonClick(eventData.button)) return;
            
            ParseEventData(FromInput(eventData.button), eventData);
        }

        private void ParseEventData(UIClickability clickedButton, PointerEventData eventData)
        {
#if ENABLE_INPUT_SYSTEM
            var mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#elif ENABLE_LEGACY_INPUT_MANAGER
            var mousePosition = Input.mousePosition;
#endif
            
            // Get effected rect local position
            var localPosition = Vector2.zero;
            if (_effectedArea.Comp != null)
            {
                var rectTransform = _effectedArea.Comp;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, 
                    mousePosition,
                    eventData.enterEventCamera, 
                    out localPosition);
            }
            
            switch (clickedButton)
            {
                case UIClickability.LEFT_BUTTON:
                    _leftButtonClickEvent?.Invoke(eventData, localPosition);
                    break;
                case UIClickability.RIGHT_BUTTON:
                    _rightButtonClickEvent?.Invoke(eventData, localPosition);
                    break;
                case UIClickability.MIDDLE_BUTTON:
                    _middleButtonClickEvent?.Invoke(eventData, localPosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clickedButton), clickedButton, null);
            }
        }

        private bool EvaluateButtonClick(PointerEventData.InputButton inputButton)
        {
            return 
                (inputButton == PointerEventData.InputButton.Left && _clickability.HasFlag(UIClickability.LEFT_BUTTON))
                || (inputButton == PointerEventData.InputButton.Right && _clickability.HasFlag(UIClickability.RIGHT_BUTTON))
                || (inputButton == PointerEventData.InputButton.Middle && _clickability.HasFlag(UIClickability.MIDDLE_BUTTON));
        }

        private UIClickability FromInput(PointerEventData.InputButton button)
        {
            return button switch
            {
                PointerEventData.InputButton.Left => UIClickability.LEFT_BUTTON,
                PointerEventData.InputButton.Right => UIClickability.RIGHT_BUTTON,
                PointerEventData.InputButton.Middle => UIClickability.MIDDLE_BUTTON,
                _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
            };
        }
    }
}