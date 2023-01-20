using UnityEngine;
using System.Collections.Generic;
using MEC;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace Studio.Framework
{
    [ExecuteInEditMode]
    public class GoEffect : GoItem
    {      
        public enum ScaleMode { Owner, Preserve };

        [SerializeField] private ScaleMode _scaleMode = ScaleMode.Owner;
        public ScaleMode scaleMode => _scaleMode;
        [SerializeField] protected string _anchor;
        public string anchor => _anchor;
        [SerializeField] private Vector3 _offset;
        public Vector3 offset { get => _offset; set => _offset = value; }
        [SerializeField] protected float _lifeTime = 3.0f;
        public float lifeTime { get => _lifeTime; set => _lifeTime = value; }
        public enum ForwardType { None, Owner, Direction }
        [SerializeField] public ForwardType forwardType = ForwardType.None;
        [SerializeField] protected bool _grounded = false;
        public enum Space { Local, World, Follow, Absolute, LocalStay, Rect }
        [SerializeField] private Space _space = Space.Local;
        public Space space => _space;
        
        [SerializeField] private List<TrailRenderer> _trails;

        [SerializeField] private bool _playOnce;
        [SerializeField] private bool _useParentLayer = true;

        public bool canStop => space == Space.Local || space == Space.Follow;

        public int stateHash { get; set; }
        private float _speed = 1f;
        public float speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        
        public GameObject owner { get; private set; }
        
        [SerializeField] private List<SubEffect> _subEffects = new List<SubEffect>();

        public int Count => _subEffects.Count;

        public SubEffect this [int i] => _subEffects[i];

        public T GetSubEffect<T>() where T : class
        {
            for (var i = 0; i < _subEffects.Count; ++i)
            {
                var ret = _subEffects[i] as T;
                if (ret != null)
                    return ret;
            }
            return null;
        }

        public virtual bool Play(GameObject owner)
        {
            if (_playOnce)
            {
                var ownerTm = owner.transform;
                for (var i = 0; i < ownerTm.childCount; ++i)
                {
                    var child = ownerTm.GetChild(i).GetComponent<GoEffect>();
                    if (child == null || child.name != name)
                        continue;
                    
                    Return();
                    return false;
                }
            }
            this.owner = owner;

            if (_useParentLayer && gameObject.layer != owner.layer)
                gameObject.SetLayer(owner.layer);

            PrepareTransform();
            ReserveReturn();

            foreach (var effect in _subEffects)
            {
                if (effect && effect.isActiveAndEnabled)
                {
                    effect.Preprocess(owner);
                }
            }

            foreach (var effect in _subEffects)
            {
                if (effect && effect.isActiveAndEnabled)
                    effect.Play(owner);
            }

            foreach (var trail in _trails)
            {
                trail.Clear();
            }
            
            return true;
        }

        public bool PlayOnLayer(GameObject owner, string sortLayerName)
        {
            foreach (var effect in _subEffects)
            {
                if (string.IsNullOrEmpty(sortLayerName) == false)
                    effect.sortLayerName = sortLayerName;
            }

            if (Play(owner) == false)
                return false;

            if (string.IsNullOrEmpty(sortLayerName) == false)
                SetSortLayer(sortLayerName);

            return true;
        }

        protected virtual void PrepareTransform()
        {
            DecideSpace(owner);

            DecideScale(owner);

            DecideForward(owner);

            if (_grounded)
                transform.Translate(0, -transform.position.y + offset.y, 0, UnityEngine.Space.World);
        }

        protected void DecideForward(GameObject owner)
        {
            if (owner == null)
                return;

            switch (forwardType)
            {
                case ForwardType.Direction:
                    {
                        var dir = owner.transform.forward; dir.y = 0;
                        transform.LookAt(transform.position + dir);
                    }
                    break;
                case ForwardType.Owner:
                    {
                        transform.localRotation = Quaternion.LookRotation(owner.transform.forward);
                    }
                    break;
                case ForwardType.None:
                    transform.localRotation = Quaternion.identity;
                    break;
            }
        }

        protected virtual void DecideScale(GameObject owner)
        {
            if (owner == null)
                return;

            switch (scaleMode)
            {
                case ScaleMode.Preserve:
                    break;
                case ScaleMode.Owner:
                    if (space == Space.Local)
                    {
                        transform.localScale = Vector3.one;
                    }
                    else if (space == Space.World || space == Space.Follow)
                    {
                        CalculateOwnerScale(owner);
                    }
                    break;
            }
        }

        protected void CalculateOwnerScale(GameObject owner)
        {
            if (owner == null)
            {
                return;
            }
            
            var parent = transform.parent;
            var scale = owner.transform.lossyScale;
            var parentScale = parent ? parent.lossyScale : Vector3.one;
            parentScale.x = Mathf.Max(parentScale.x, Mathf.Epsilon);
            parentScale.y = Mathf.Max(parentScale.y, Mathf.Epsilon);
            parentScale.z = Mathf.Max(parentScale.z, Mathf.Epsilon);
            transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
        }

        protected void DecideSpace(GameObject owner)
        {
            Transform ownerTm, parent = null;
            if (owner != null)
            {
                if (string.IsNullOrEmpty(anchor) == false)
                    parent = owner.transform.FindDescendant(anchor);
                if (!parent)
                    parent = owner.transform;
                ownerTm = owner.transform;
            }
            else
            {
                parent = transform;
                ownerTm = transform;
            }

            switch (space)
            {
                case Space.Local:
                    transform.SetParent(parent);
                    transform.localPosition = offset;
                    break;
                case Space.LocalStay:
                    transform.SetParent(parent, true);
                    transform.localPosition += offset;
                    break;
                case Space.World:
                    transform.SetParent(Service.GoPool.transform);
                    transform.position = parent.transform.position + (ownerTm.localRotation * offset);
                    break;
                case Space.Follow:
                    transform.position = parent.transform.position + (ownerTm.localRotation * offset);
                    Chase(parent);
                    break;
                case Space.Absolute:
                    transform.SetParent(Service.GoPool.transform);
                    transform.position = offset;
                    break;
                case Space.Rect:
                    transform.SetParent(parent);
                    ((RectTransform)transform).anchoredPosition = offset;
                    break;
                default: break;
            }
        }


        public void Add(SubEffect sub)
        {
            if (!sub || _subEffects.Contains(sub))
                return;
            _subEffects.Add(sub);
        }

        [ContextMenu("Collect")]
        protected virtual void Collect()
        {
            if (_subEffects.Count == 0)
                GetComponentsInChildren(true, _subEffects);
        }

        public void OnValidate()
        {
            SubEffect c;
            for (var i = 0; i < _subEffects.Count; ++i)
            {
                c = _subEffects[i];
                if (c == null)
                {
                    _subEffects.RemoveAt(i--);
                    continue;
                }
                for (var j = i + 1; j < _subEffects.Count; ++j)
                {
                    if (_subEffects[j] == c)
                        _subEffects.RemoveAt(j--);
                }
            }

            if (_trails != null)
            {
                for (var i = 0; i < _trails.Count; ++i)
                {
                    if (!_trails[i])
                        _trails.RemoveAt(i--);
                }
            }
        }

        private CoroutineHandle hChace;

        protected void Chase(Transform parent)
        {
            hChace = Timing.RunCoroutineSingleton(ChaceAsync(parent), hChace, Segment.LateUpdate, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> ChaceAsync(Transform parent)
        {
            while (parent && parent.gameObject.activeInHierarchy)
            {
                if (!(Object)owner || !parent)
                    break;
                transform.position = parent.position + (owner.transform.localRotation * offset);
                yield return Timing.WaitForOneFrame;
            }
            Service.GoPool.Return(this);
        }

        public override void OnRent()
        {
            if (hClock.IsValid && hClock.IsRunning)
                Timing.KillCoroutines(hClock);
            speed = 1f;
            stateHash = 0;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            foreach (var sub in _subEffects)
                sub.OnRent();
        }
        private bool _hasOwner = false;

        public virtual bool hasOwner
        {
            get { return space == Space.Local || space == Space.Follow || _hasOwner; }
            set { _hasOwner = value; }
        }

        public override void OnReturn()
        {
            base.OnReturn();
            if (lifeTime > 0f)
                hClock.IsRunning = false;
            owner = null;
            hasOwner = false;
            if (space == Space.Follow)
                hChace.IsRunning = false;
        }

        protected void ReserveReturn()
        {
            if (Mathf.Approximately(lifeTime, 0) == false)
            {
                hClock = Timing.RunCoroutineSingleton(
                    ClockLifeTime(lifeTime),
                    hClock,
                    Segment.Update,
                    SingletonBehavior.Overwrite);
            }
        }

        public float GetLifeTime()
        {
            return lifeTime;
        }

        public void SetLifeTime(float lifeTime)
        {
            this.lifeTime = lifeTime;

            if (hClock.IsValid && hClock.IsRunning)
                Timing.KillCoroutines(hClock);

            ReserveReturn();
        }

        private IEnumerator<float> ClockLifeTime(float lifeTime)
        {
            yield return Timing.WaitForSeconds(lifeTime);
            
            if (this)
                Service.GoPool.Return(this);
        }

        private CoroutineHandle hClock;

        public void SetSortLayer(string sortLayerName)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach(var renderer in renderers)
                renderer.sortingLayerID = SortingLayer.NameToID(sortLayerName);
        }
    }


    [System.Serializable]
    public class AssetReferenceEffect : AssetReferenceGameObject
    {
        public AssetReferenceEffect(string guid) : base(guid) {}

        public override bool ValidateAsset(string path)
        {
            if (!base.ValidateAsset(path))
                return false;

            #if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GoEffect>(path) != null;
            #else
            return true;
            #endif
        }
    }
}