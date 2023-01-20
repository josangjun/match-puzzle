using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine.AddressableAssets;
using Studio.Common;

#pragma warning disable 649

namespace Studio.Framework
{
    public interface IPreloadView {}

    public class ViewStack : MonoBehaviour, IEnumerable<View>
    {
        [SerializeField]
        private float _id;
        public float Id => _id;
        
        [ContextMenu("Collect")]
        private void Collect()
        {
            _views.Clear();
            for (var i = transform.childCount-1; i >= 0; --i)
            {
                var child = transform.GetChild(i);
                if (!child) continue;
                var view = child.GetComponent<View>();
                if (!view) continue;
                _views.Add(view);
            }
        }

        [SerializeField]
        private List<View> _prefabs = new List<View>();

        [SerializeField]
        private List<SoftLink<GameObject>> _addressLinks = new List<SoftLink<GameObject>>();
        public List<SoftLink<GameObject>> AddressLinks => _addressLinks;

        [SerializeField]
        public bool suppressDefaultLayer;

        [ContextMenu("Sort")]
        private void Sort()
        {
            _prefabs.Sort((x, y) =>
            {
                return x.name.CompareTo(y.name);
            });

            _addressLinks.Sort((x, y) =>
            {
                var cmp = x.name.CompareTo(y.name);
                if (cmp == 0)
                    return x.path.CompareTo(y.path);
                return cmp;
            });
#if UNITY_EDITOR
            foreach (var link in _addressLinks)
            {
                var item = _prefabs.BinarySearchFirst(arg => arg.name.CompareTo(link.name));
                var path = UnityEditor.AssetDatabase.GetAssetPath(item);
                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                if (guid == link.guid)
                {
                    _prefabs.Remove(item);
                }
            }
#endif
        }

        private List<View> _views = new List<View>();

        public bool Escape()
        {
            var view = Peek();
            if (!view.IsActive())
            {
                return false;
            }

            view.Escape();
            return true;
        }

        public IAddressLink<GameObject> GetAddressLink(string name)
        {
            return _addressLinks.BinOrSeqSearch(arg => arg.name.CompareTo(name));
        }

        public View Push(GameObject prefab)
        {
            var view = prefab.GetComponent<View>();
            var instance = Service.GoPool.Rent(view, transform);
            Push(instance);
            return instance;
        }

        public void Push(View view)
        {
            if (!view)
                return;

            if (_views.Contains(view))
            {
                Debug.LogError($"already contains:{view.name}, {Id}");
                return;
            }
            view.OnFoucsChangedAsObservable().Subscribe(OnFocusHasChanged).AddTo(this);
            view.OnFoucsChangingAsObservable().Subscribe(OnFocusChanging).AddTo(this);
            _views.Add(view);
        }

        public View GetView(string name)
        {
            var view = _views.FirstOrDefault(arg => arg.name == name);
            if (view)
                return view;

            var prefab = _prefabs.BinOrSeqSearch(arg => arg.name.CompareTo(name));
            if (prefab)
            {
                view = PushPrefab(prefab);
                return view;
            }

            var link = _addressLinks.BinOrSeqSearch(arg => arg.name.CompareTo(name));
            if (link == null)
                return default;
            
            var asyncOp = link.LoadAssetAsync();
            asyncOp.WaitForCompletion();
            if (asyncOp.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                var result = asyncOp.Result;
                view = PushPrefab(result);
                return view;
            }
            return default;
        }

        public View Peek()
        {
            for (var i = transform.childCount-1; i >= 0; --i)
            {
                var child = transform.GetChild(i);
                var view = child.GetComponent<View>();
                if (view)
                    return view;
            }
            return default;
        }

        public void Remove(View view)
        {
            _views.Remove(view);
        }

#if UNITY_EDITOR
        [ContextMenu("VerifyAddressLink")]
        public void VerifyAddressLink()
        {
            foreach (var link in _addressLinks)
            {
                var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(link.path);
                var view = go.GetComponent<View>();
                var methods = view.GetType().GetMethods(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.DeclaredOnly |
                    System.Reflection.BindingFlags.Instance);
                var prepares = methods.Where(arg => arg.Name == "Prepare").ToArray();
                if (prepares.Length > 0)
                {
                    Debug.LogError(view.name);
                }
            }
        }
#endif

        // Start is called before the first frame update
        public IEnumerator Start()
        {
            while (!Service.ready)
                yield return null;
            _views.Clear();
            foreach (var arg in _prefabs)
            {
                if (arg is IPreloadView)
                    PushPrefab(arg);
            }
            
            Service.UIManager.Add(this);
        }

        public void Prepare(params string[] resources)
        {
            foreach (var name in resources)
            {
                var view = _prefabs.BinOrSeqSearch(arg => arg.name.CompareTo(name));
                if (view != null)
                {
                    PushPrefab(view);
                    continue;
                }
                var link = _addressLinks.BinOrSeqSearch(arg => arg.name.CompareTo(name));
                if (link == null)
                    continue;
                
                var asyncOp = link.LoadAssetAsync();
                asyncOp.WaitForCompletion();
                var go = asyncOp.Result;
                PushPrefab(go);
            }
        }

        private View PushPrefab(GameObject go)
        {
            var prefab = go.GetComponent<View>();
            if (prefab == null)
                return null;
            return PushPrefab(prefab);
        }

        private View PushPrefab(View prefab)
        { 
            var view = Instantiate(prefab, transform);
            view.name = prefab.name;
            view.transform.SetAsFirstSibling();
            view.Prepare();
            view.gameObject.SetActive(false);
            Push(view);
            return view;
        }

        public void OnDestroy()
        {
            if (Service.Instance)
                Service.UIManager.Remove(this);

            foreach (var arg in _addressLinks)
                arg.ReleaseAsset();
        }

        public void Close()
        {
            if (_views.Count == 0)
                return;

            var first = _views[0];
            if (!first.gameObject.activeSelf)
                return;
            
            first.Close();
        }

        private static int CompareSiblingIndex(View lhs, View rhs)
        {
            return rhs.transform.GetSiblingIndex().CompareTo(lhs.transform.GetSiblingIndex());
        }

        public void OnFocusChanging(View view)
        {
            if (_views.Count < 2)
                return;

            var second = _views[1];
            if (second.isActiveAndEnabled)
                second.alpha = 1f;

            SortViews();

            ToggleMainCamera(true);
        }

        public void OnFocusHasChanged(View view)
        {
            if (_views.Count == 0)
                return;

            if (view.WillBeDestroyed())
            {
                Remove(view);
            }

            SortViews();

            var first = _views[0];
            
            Expose(first.stackCount);

            if (!first.isActiveAndEnabled)
            {
                var stacks = Service.UIManager.GetStacks();
                for (var i = stacks.Count - 1; i >= 0; --i)
                {
                    var stack = stacks[i];
                    if (stack.Id >= Id)
                        continue;
                    var successor = stack.Peek();
                    view.Resign(successor);
                }
            }
        }

        private void SortViews()
        {
            var oldFirst = _views[0];

            _views.Sort(CompareSiblingIndex);

            var first = _views[0];

            if (!ReferenceEquals(first, oldFirst))
            {
                oldFirst.OnFocus(false);
                first.OnFocus(true);
            }

            if (first.isDirty && first.isActiveAndEnabled)
            {
                first.Redraw();
                first.isDirty = false;
            }
        }

        private void Expose(int count)
        {
            var i = Mathf.Min(10, _views.Count - 1);
            for (; i >= count; --i)
            {
                var view = _views[i];
                if (view.IsActive())
                    view.Show(false);
            }

            var opaque = false;

            for (; i >= 0; --i)
            {
                var view = _views[i];
                if (view.IsActive())
                {
                    view.Show(true);
                    if (view.IsOpaque())
                        opaque = true;
                }
            }

            if (suppressDefaultLayer)
            {
                ToggleMainCamera(!opaque);
            }
        }

        public static void ToggleMainCamera(bool toggle)
        {
            const int DefaultMask = 1;
            var camera = Camera.main;
            if (camera == null)
                return;
            if (toggle)
                camera.cullingMask |= DefaultMask;
            else
                camera.cullingMask &= ~DefaultMask;
        }

        public View GetTop()
        {
            var last = _views.FirstOrDefault();
            return last && last.isActiveAndEnabled ? last : default;
        }

        public bool IsTopMost<T>() where T : View
        {
            var last = _views.FirstOrDefault();
            return last != null && last is T;
        }

        public bool IsTopMost(View view)
        {
            var last = _views.FirstOrDefault();
            return ReferenceEquals(last, view);
        }

        public void CloseTopMost()
        {
            var actvieList = _views.Where(arg => arg.isActiveAndEnabled).ToList();
            if (actvieList.Count > 1)
            {
                actvieList.Last().Close();
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _views.GetEnumerator();
        }

        IEnumerator<View> IEnumerable<View>.GetEnumerator()
        {
            return ((IEnumerable<View>)_views).GetEnumerator();
        }
    }
}