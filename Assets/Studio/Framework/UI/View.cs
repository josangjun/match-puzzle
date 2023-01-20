using UnityEngine;
using DG.Tweening;
using System.Linq;
using System.Threading.Tasks;
using UniRx;

#pragma warning disable 649

namespace Studio.Framework
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class View : GoItem
    {
        public object RuntimeKey { get; set; } = string.Empty;
        [SerializeField]
        protected CanvasGroup _canvasGroup;
        [SerializeField]
        protected Vector2 _animTime = new Vector2(0.5f, 0.5f);
        public float inTime => _animTime.x;
        public float outTime => _animTime.y;

        [SerializeField] [Range(1, 5)]
        private int _stackCount = 2;

        [SerializeField]
        private bool _destroyOnClose;

        public int stackCount => _stackCount;

        public virtual bool IsOpaque() => _stackCount == 1 && !isFading;

        public bool isFading { get; private set; }

        protected Subject<View> onClose = new Subject<View>();

        public System.IObservable<View> OnCloseAsObservable() => onClose.Take(1);

        protected bool pendingClose = false;

        public virtual bool IsActiveSelf()
        {
            return isActiveAndEnabled;
        }

        public bool interactable
        {
            get => _canvasGroup.interactable;
            set => _canvasGroup.interactable = value;
        }

        public void SetDestroyOnClose(bool toggle)
        {
            _destroyOnClose = toggle;
        }

        public bool WillBeDestroyed()
        {
            return _destroyOnClose && !isActiveAndEnabled;
        }

        public float alpha
        {
            get => _canvasGroup.alpha;
            set
            {
                _canvasGroup.alpha = value;
                // Debug.Log($"View:{name}, {value}");
            }
        }

        public System.IDisposable SetAlpha(float alpha)
        {
            var h = new AlphaHandle(this, this.alpha);
            this.alpha = alpha;
            return h;
        }

        private struct AlphaHandle : System.IDisposable
        {
            private float _value;
            private View _view;

            public AlphaHandle(View view, float val)
            {
                _value = val;
                _view = view;
            }

            public void Dispose()
            {
                _view.alpha = _value;
            }
        }

        private int _nameHash = 0;

        public int nameHash
        {
            get
            {
                if (_nameHash == 0)
                    _nameHash = Animator.StringToHash(name);
                return _nameHash;
            }
        }

        public RectTransform rectTransform => (RectTransform)transform;

        public virtual void Prepare() {}

        public void Show(bool toggle)
        {
            if (isFading)
                return;
            alpha = toggle ? 1f : 0f;
            interactable = toggle;
        }

        public void Open<T>(T arg) 
        {
            if (!typeof(T).IsSerializable)
                throw new System.InvalidOperationException();
            var json = JsonUtility.ToJson(arg);
            var context = new ViewContext(json);
            Open(context);
        }

        public System.IObservable<View> OnFoucsChangedAsObservable() => onFocusChanged;
        public System.IObservable<View> OnFoucsChangingAsObservable() => onFocusChanging;

        protected readonly Subject<View> onFocusChanging = new Subject<View>();
        protected readonly Subject<View> onFocusChanged = new Subject<View>();

        public void Open(ViewContext context)
        {
            transform.SetAsLastSibling();
            isFading = true;
            gameObject.SetActive(true);
            onFocusChanging.OnNext(this);
            interactable = true;
            OnDraw(context);
            _ = FadeInDefault();
        }

        public void Open(params object[] args)
        {
            var context = args.Length == 0 ? ViewContext.Default : new ViewContext(args);
            Open(context);
        }

        protected virtual void OnDraw(ViewContext context) {}

        protected virtual void OnFadeFinished()
        {
            onFocusChanged.OnNext(this);
            _canvasGroup.blocksRaycasts = true;
        }

        public float finalAlpha { get; set; } = 0f;

        protected async Task FadeInDefault()
        {
            await FadeIn();
            isFading = false;
            if (this)
                OnFadeFinished();
        }

        protected virtual async Task FadeIn()
        {
            _canvasGroup.alpha = finalAlpha;
            var tween = _canvasGroup.DOFade(1f, inTime);
            tween.timeScale = 1f;
            await tween;
        }

        public virtual void Escape()
        {
            Close();
        }

        public virtual void Close()
        {
            if (!gameObject.activeSelf)
                return;

            if (pendingClose)
                return;

            pendingClose = true;
            isFading = true;

            onFocusChanging.OnNext(this);
            
            _ = FadeOut(OnCloseFadeOutComplete);

            onClose.OnNext(this);
        }

        protected virtual async Task FadeOut(System.Action onComplete)
        {
            var tween = _canvasGroup
                .DOFade(finalAlpha, outTime);
            tween.timeScale = 1f;
            await tween;
            onComplete?.Invoke();
        }

        private bool isDestroyed;
        protected virtual void OnDestroy()
        {
            isDestroyed = true;
        }

        protected virtual void OnCloseFadeOutComplete()
        {
            pendingClose = false;
            CloseImmediate();
        } 

        public virtual void CloseImmediate()
        {
            if (isDestroyed) { return; }
            if (_destroyOnClose)
            {
                transform.SetParent(null);
                Destroy(gameObject);
            }
            else
            {
                transform.SetAsFirstSibling();
                interactable = true;
                gameObject.SetActive(false);
            }
            isFading = false;
            onFocusChanged.OnNext(this);
        }

        public virtual void OnFocus(bool toggle)
        {

        }

        public virtual void Redraw()
        {
            
        }

        public virtual void RefreshCapture()
        {
            var a = alpha;
            alpha = 0f;
            foreach (Transform child in transform)
            {
                var ci = child.GetComponent<CaptureImage>();
                if (ci == null) continue;
                ci.Refresh();
                break;
            }
            alpha = a;
        }

        public virtual void Resign(View successor)
        {
            if (successor == null)
                return;
            successor.RefreshCapture();
        }

        public bool isDirty { get; set; }

        public void MakeSiblingsDirty()
        {
            var parent = transform.parent;
            var index = transform.GetSiblingIndex();
            for (var i = index - 1; i >= 0; --i)
            {
                var child = parent.GetChild(i);
                var view = child.GetComponent<View>();
                if (view == null)
                    continue;
                if (view.isActiveAndEnabled == false)
                    break;
                view.isDirty = true;
            }
        }

        public void CloseSiblings(bool immediate = true)
        {
            var parent = transform.parent;
            for (var i = parent.childCount - 1; i >= 0; --i)
            {
                var sibling = parent.GetChild(i);
                var view = sibling.GetComponent<View>();
                if (view.GetInstanceID() == GetInstanceID())
                    continue;
                if (view.isActiveAndEnabled == false)
                    break;
                if (immediate)
                    view.CloseImmediate();
                else
                    view.Close();
            }
        }

        protected virtual void OnValidate()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public static class ViewExtensions
    {
        public static bool IsActive(this View view)
        {
            return view && view.IsActiveSelf();
        }
    }
}