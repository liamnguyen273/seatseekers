using com.brg.UnityCommon.Editor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.brg.UnityComponents
{
    public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {        
        [SerializeField] private CompWrapper<RectTransform> _draggableArea = "./";

        private RectTransform _cachedParentRect;
        private RectTransform _cachedSelfRect;
        private bool _isDragging = false;
        private Vector2 _dragPosOffset;

        private void Awake()
        {
            _cachedSelfRect = GetComponent<RectTransform>();
            _cachedParentRect = _cachedSelfRect.parent as RectTransform;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(_draggableArea, eventData.position, eventData.pressEventCamera))
            {
                var localPointerPos = Vector2.zero;
                var localPosition = (Vector2)_cachedSelfRect.localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_cachedParentRect, eventData.position, eventData.pressEventCamera, out localPointerPos);
                _dragPosOffset = localPosition - localPointerPos;
                _isDragging = true;
            }
            else
            {
                _isDragging = false;
            }            
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            var localPointerPos = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_cachedParentRect, eventData.position, eventData.pressEventCamera, out localPointerPos))
            {
                _cachedSelfRect.localPosition = localPointerPos + _dragPosOffset;
            }
        }
    }
}