using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Studio.Framework
{
    public class PointerEventHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent<PointerEventData> onBeginDragHandler;
        public UnityEvent<PointerEventData> onDragHandler;
        public UnityEvent<PointerEventData> onEndDragHandler;

        public UnityEvent<PointerEventData> onDownHandler;
        public UnityEvent<PointerEventData> onUpHandler;

        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDragHandler.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDragHandler.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDragHandler.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onDownHandler.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onUpHandler.Invoke(eventData);
        }
    }
}