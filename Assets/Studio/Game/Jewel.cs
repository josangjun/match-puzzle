using System.Collections;
using System.Collections.Generic;
using Studio.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Studio.Game
{
    public enum Direction { None, Up, Right, Down, Left };

    public class Jewel : GoItem, IDragHandler, IPointerDownHandler
    {
        public readonly UniRx.Subject<Jewel> onPress = new UniRx.Subject<Jewel>();

        [System.NonSerialized]
        [NaughtyAttributes.ShowNonSerializedField]
        public int x, y;

        [SerializeField]
        private Image _image;
        public Image image => _image;

        public RectTransform rect => (RectTransform)transform;

        public override string ToString()
        {
            return $"Jewel[{x},{y}]";
        }

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
            onPress.OnNext(this);
        }

        public bool IsAdjacency(Jewel other)
        {
            var diffx = Mathf.Abs(x - other.x);
            var diffy = Mathf.Abs(y - other.y);

            return diffx + diffy == 1;
        }

        public static Vector2 Size { get; private set; }
        public static Vector2 Offset { get; private set; }

        public static void Init(Vector2 size, Vector2 offset)
        {
            Size = size;
            Offset = offset;
        }
        
        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;

            rect.anchoredPosition = new Vector2(x * Size.x - Offset.x, y * -Size.y + Offset.y);
        }
    }
}