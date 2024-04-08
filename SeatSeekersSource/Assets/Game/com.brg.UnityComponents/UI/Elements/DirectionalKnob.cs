using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Utils = com.brg.UnityCommon.Utils;

// Adapted from LeTai.TrueShadow.Demo, original content provided below

namespace com.brg.UnityComponents
{

    [ExecuteAlways]
    public class DirectionalKnob : UIBehaviour, IDragHandler
    {
        [SerializeField] private GOWrapper _knobGraphic = "./Circle";
        [SerializeField] private float _min = 0;
        [SerializeField] private float _max = 1;
        [SerializeField] private float _value = .5f;

        public EventWrapper<float> KnobValueChangedEvent;

        private RectTransform _rectTransform;
        private Vector2 _zeroVector;

        protected override void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _value = Mathf.Clamp(_value, _min, _max);
            SetValue(_value);
        }
#endif

        public void SetValue(float newValue)
        {
            _value = newValue;

            _knobGraphic.Transform.localRotation = Quaternion.Euler(0, 0, 1 - _value * 360);

            KnobValueChangedEvent?.Invoke(_value);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var point
                )) return;
            
            var angle = Utils.Angle360(Vector2.down, point);
            SetValue(Mathf.InverseLerp(_min, _max, 1 - angle / 360f));
        }
    }
}

// namespace LeTai.TrueShadow.Demo
// {
//     [Serializable]
//     public class KnobValueChangedEvent : UnityEvent<float> { }
//
//     [ExecuteAlways]
//     public class DirectionalKnob : UIBehaviour, IDragHandler
//     {
//         public Transform knobGraphic;
//         public float     min   = 0;
//         public float     max   = 1;
//         public float     value = .5f;
//
//         public KnobValueChangedEvent knobValueChanged;
//
//         RectTransform rectTransform;
//         Vector2       zeroVector;
//
//
//         protected override void Start()
//         {
//             rectTransform = GetComponent<RectTransform>();
//         }
//
// #if UNITY_EDITOR
//         protected override void OnValidate()
//         {
//             base.OnValidate();
//
//             value = Mathf.Clamp(value, min, max);
//             SetValue(value);
//         }
// #endif
//
//         public void SetValue(float newValue)
//         {
//             value = newValue;
//
//             knobGraphic.localRotation = Quaternion.Euler(0, 0, 1 - value * 360);
//
//             knobValueChanged.Invoke(value);
//         }
//
//         public void OnDrag(PointerEventData eventData)
//         {
//             if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
//                     rectTransform,
//                     eventData.position,
//                     eventData.pressEventCamera,
//                     out var point
//                 ))
//             {
//                 var angle = Angle360(Vector2.down, point);
//                 SetValue(Mathf.InverseLerp(min, max, 1 - angle / 360f));
//             }
//         }
//     }
// }
