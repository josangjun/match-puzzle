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
    public enum GemType { None, Red, Orange, Yellow, Green, Blue, Purple, White }

    public class Gem : GoItem, IDragHandler, IPointerDownHandler
    {
        public readonly UniRx.Subject<Gem> onPress = new UniRx.Subject<Gem>();

        [System.NonSerialized, NaughtyAttributes.ShowNonSerializedField]
        public int x, y;

        [SerializeField]
        private Image _image;
        public Image image => _image;

        [SerializeField]
        private GemVisualizer _visualizer;

        public RectTransform rect => (RectTransform)transform;

        [SerializeField]
        private GemType _type = GemType.None;
        public GemType type
        {
            get => _type;
            set
            {
                _type = value;
                Refresh(_type);
            }
        }

        public override string ToString()
        {
            return $"Gem[{type},{x},{y}]";
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

        public bool IsAdjacency(Gem other)
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

        private void Refresh(GemType type)
        {
            _visualizer.SetAttribute(this, type);
        }

        protected void OnValidate()
        {
            Refresh(_type);
        }
    }
}