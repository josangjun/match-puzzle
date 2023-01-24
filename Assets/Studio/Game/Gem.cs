using System.Collections;
using System.Collections.Generic;
using Studio.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;

namespace Studio.Game
{
    [System.Flags]
    public enum Direction { None, Up, Right, Down, Left };
    public enum GemType { None, Red, Orange, Yellow, Green, Blue, Purple, White, Max }
    public enum Axis { Horizontal, Vetical };

    public class Gem : GoItem, IDragHandler, IPointerDownHandler
    {
        public static implicit operator Point(Gem gem)
        {
            return new Point { x = gem.x, y = gem.y };
        }

        public readonly UniRx.Subject<Gem> onPress = new UniRx.Subject<Gem>();

        //[System.NonSerialized, NaughtyAttributes.ShowNonSerializedField]
        public int x, y;

        [SerializeField]
        private Image _image;
        public Image image => _image;

        [SerializeField]
        private GemVisualizer _visualizer;

        private RectTransform _rectTransform;

        public RectTransform rectTransform => _rectTransform;

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
        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        public override void OnRent()
        {
            base.OnRent();

            x = y = -1;
            image.transform.localScale = new Vector2(1, 1);
        }

        public readonly UniRx.Subject<PointerEventData> onDrag = new UniRx.Subject<PointerEventData>();
        
        public void OnDrag(PointerEventData eventData)
        {
            onDrag.OnNext(eventData);
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
        
        public void SetPosition(Point pt)
        {
            SetPosition(pt.x, pt.y);
        }

        public void SetPosition(int x, int y)
        {
            var old = Stage.Instance[this.x, this.y];
            if (old && old.GetInstanceID() == GetInstanceID())
                Stage.Instance[this.x, this.y] = null;

            this.x = x;
            this.y = y;

            rectTransform.anchoredPosition = GetAnchoredPosition(x, y);
            Stage.Instance[x, y] = this;
#if UNITY_EDITOR
            name = $"Gem[{x},{y},{type}]";
#endif
        }

        public static Vector2 GetAnchoredPosition(int x, int y)
        {
            return new Vector2(x * Size.x - Offset.x, y * -Size.y + Offset.y);
        }

        public static Point GetPoint(Vector2 pos)
        {
            var board = (RectTransform)Stage.Instance.gemFactory.transform;
            var rect = board.rect;
            var diffx = pos.x - rect.xMin;
            var diffy = Mathf.Round(rect.height / Size.y) - pos.y - rect.yMin;
            var x = Mathf.FloorToInt(diffx / Size.x);
            var y = Mathf.FloorToInt(diffy / Size.y);
            return new Point { x = x, y = y };
        }

        public Tween MovePosition(int x, int y, float time = 0.3f)
        {
            var h = _lock.Lock();
            var dest = GetAnchoredPosition(x, y);

            var tween = rectTransform.DOAnchorPos(dest, time).OnComplete(() =>
            {
                h.Dispose();
                SetPosition(x, y);
            });
            return tween;
        }

        public bool IsIdle() => _lock.count == 0;
        public bool IsLocked() => _lock.count > 0;

        public System.IDisposable Lock() => _lock.Lock();

        private CountingLock _lock = new CountingLock();

        public UniRx.Subject<Point> onExplosion = new Subject<Point>();

        public void Explode()
        {
            if (IsLocked())
                return;
            var h = _lock.Lock();
            image.transform.DOScale(0, 0.3f).OnComplete(() =>
            {
                Stage.Instance[x, y] = null;
                onExplosion.OnNext(this);
                h.Dispose();
                Return();
            });
        }

        private FreeFall _freeFall = new FreeFall();

        [ContextMenu("Fall")]
        public void Fall()
        {
            _freeFall.target = this;
            _freeFall.start = rectTransform.anchoredPosition;
            _freeFall.elapsed = 0f;
            
            var h = Lock();
            IEnumerator Tick_()
            {
                while (this && isActiveAndEnabled)
                {
                    if (!_freeFall.Tick())
                        break;
                    yield return null;
                }
                h.Dispose();
            }
            if (isActiveAndEnabled)
            {
                MainThreadDispatcher.StartUpdateMicroCoroutine(Tick_());
            }
        }

        private void Refresh(GemType type)
        {
            _visualizer.SetAttribute(this, type);
        }

        protected void OnValidate()
        {
            Refresh(_type);
        }

        public Gem GetNext(Direction dir)
        {
            var p = (Point)this;
            return (Gem)p.Move(dir);
        }
    }
}