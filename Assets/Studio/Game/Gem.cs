using System.Collections;
using System.Collections.Generic;
using Studio.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;
using MEC;

namespace Studio.Game
{
    [System.Flags]
    public enum Direction { None, Up, Right, Down, Left, Max };

    public enum GemType {
        None, Red, Blue, Yellow, Green, Orange, Purple, White, Max,
        TNT, RocketV, RocketH, Propeller, LightBall }
    [System.Flags]
    public enum GemEquipment { None };
    public enum Axis { Horizontal, Vetical };

    public class Gem : GoItem, IDragHandler, IPointerDownHandler, IPointerUpHandler, System.IEquatable<Gem>
    {
        public static implicit operator Point(Gem gem)
        {
            return new Point { x = gem.x, y = gem.y };
        }

        public readonly Subject<Gem> onPress = new Subject<Gem>();

        [System.NonSerialized, NaughtyAttributes.ShowNonSerializedField]
        public int x, y;

        public int key => (x << 16) | y;

        [SerializeField]
        private Image _image;
        public Image image => _image;

        private IGemVisualizer _visualizer;

        public void SetVisualizer(IGemVisualizer visualizer)
        {
            _visualizer = visualizer;
        }

        private RectTransform _rectTransform;

        public RectTransform rectTransform => _rectTransform;

        [System.NonSerialized]
        public float touchTime;

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

        public void SetTypeSilently(GemType type)
        {
            if (!IsLocked())
            {
                _type = type;
            }
        }

        [SerializeField]
        private GemEquipment _equipment;
        public GemEquipment equipment
        {
            get => _equipment;
        }

        [SerializeField]
        private FreeFall _freeFall = new FreeFall();
        [SerializeField]
        private ExplosionFlow _explosion = new ExplosionFlow();

        public override string ToString()
        {
            return $"Gem[{x},{y},{type}]";
        }

        // Start is called before the first frame update
        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _freeFall.SetOwner(this);
            _explosion.SetOwner(this);
        }

        public override void OnRent()
        {
            base.OnRent();

            x = y = -1;
            image.transform.localScale = new Vector2(1, 1);
        }

        public readonly Subject<Gem> onClick = new Subject<Gem>();

        public readonly Subject<PointerEventData> onDrag = new Subject<PointerEventData>();
        
        public void OnDrag(PointerEventData eventData)
        {
            if (ReferenceEquals(eventData.pointerDrag, gameObject))
                onDrag.OnNext(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            touchTime = Time.time;
            onPress.OnNext(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var diff = Time.time - touchTime;
            if (diff < 0.2f)
            {
                onClick.OnNext(this);
            }
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
        
        public void SetPosition(Point pt, bool immediately = true)
        {
            SetPosition(pt.x, pt.y, immediately);
        }

        public void SetPosition(int x, int y, bool immediately = true)
        {
            var old = Stage.Instance[this.x, this.y];
            if (old && old.GetInstanceID() == GetInstanceID())
                Stage.Instance[this.x, this.y] = null;
            Stage.Instance[x, y] = this;
            this.x = x;
            this.y = y;

            if (immediately)
            {
                rectTransform.anchoredPosition = GetAnchoredPosition(x, y);
            }
#if UNITY_EDITOR
            var str = $"Gem[{x},{y},{type}]";
            //SLogger.Log($"{name} => {str}");
            name = str;            
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

        public Tween MovePosition(Point p, System.Action action, float time = 0.3f)
        {
            return MovePosition(p.x, p.y, action, time);
        }

        public Tween MovePosition(int x, int y, System.Action action = null, float time = 0.3f)
        {
            if (IsLocked())
                return default;
            ++Stage.Instance.moveCount;
            var h = Lock();
            var dest = GetAnchoredPosition(x, y);

            var tween = rectTransform.DOAnchorPos(dest, time).OnKill(() =>
            {
                --Stage.Instance.moveCount;
                h.Dispose();
                if (action != null)
                    action();
                SetPosition(x, y);
            });
            return tween;
        }

        public bool CanMove() => _lock.count == 0 && IsFloor();

        public bool IsLocked() => _lock.count > 0;

        public System.IDisposable Lock() => _lock.Lock();

        [SerializeField]
        private CountingLock _lock = new CountingLock();

        public Subject<Point> onExplosion = new Subject<Point>();

        public float kExplosionTime = 0.3f;

        public void Join(GemInfo guest) =>
            _explosion.Join(guest);

        [System.Serializable]
        private class StackTrace
        {
            [SerializeField]
            private string _str;
            [SerializeField]
            private int _frame;
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public void Mark()
            {
                _str = System.Environment.StackTrace;
                _frame = Time.frameCount;
            }
        }
        [SerializeField]
        private StackTrace _disable;

        public bool Explode()
        {
            if (IsLocked() || y < 0)
                return false;

            var h = Lock();
            _explosion.Explode();
            var c = Explode_().RunCoroutine();
            c.OnDestroy(() => h.Dispose());
            
            return true;
        }

        private IEnumerator<float> Explode_()
        {
            var tween = image.transform.DOScale(0, kExplosionTime);
            while (tween.IsActive())
                yield return 0f;
            while (_explosion.IsRunning())
                yield return 0f;
            if (Equals(Stage.Instance[x, y]))
                Stage.Instance[x, y] = null;
            onExplosion.OnNext(this);
            Return();
        }

        [ContextMenu("Fall")]
        public bool Fall()
        {
            if (!isActiveAndEnabled)
            {
                return false;
            }
            if (IsLocked())
                return false;

            var h = Lock();
            
            _freeFall.Start(() =>
                {
                    h.Dispose();
                    Stage.Instance.MatchAndExplode();
                });
            //NotifyUpwards();
            
            return true;
        }

        public void NotifyUpwards()
        {
            var pt = (Point)this;
            for (var i = y - 1; i >= 0; --i)
            {
                pt.y = i;
                var g = (Gem)pt;
                if (g != null)
                    g.Fall();
            }
        }

        private void Refresh(GemType type)
        {
            if (_visualizer != null)
                _visualizer.SetType(this, type);
        }

        protected void OnValidate()
        {
            Refresh(_type);
        }

        protected void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            _freeFall.Stop();
            _disable.Mark();
        }

        public Gem GetNext(Direction dir)
        {
            var p = (Point)this;
            return (Gem)p.Move(dir);
        }

        public bool IsFloor()
        {
            var p = (Point)this;
            return p.IsFloor();
        }

        public bool IsFalling()
        {
            return _freeFall.keepWaiting;
        }

        public bool Equals(Gem other)
        {
            return other != null && GetInstanceID() == other.GetInstanceID();
        }
    }
}