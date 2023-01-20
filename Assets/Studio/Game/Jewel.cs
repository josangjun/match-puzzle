using System.Collections;
using System.Collections.Generic;
using Studio.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Studio.Game
{
    public enum Direction { None, Up, Right, Down, Left };

    public class Jewel : GoItem, IDragHandler, IPointerDownHandler
    {
        [System.NonSerialized]
        [NaughtyAttributes.ShowNonSerializedField]
        public int x, y;

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void OnRent()
        {
            base.OnRent();

            x = y = -1;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var diff = eventData.position - eventData.pressPosition;
            Debug.Log(diff.magnitude);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            
        }
    }
}