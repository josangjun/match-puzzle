using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using Studio.Common;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Framework
{
    public interface IViewValidator
    {
        bool CanOpen(View view);
    }

    public class UIManager : MonoBehaviour
    {
        public void Start()
        {
            TextMeshProUtils.LoadTMPSettings();
        }

        public static string GetAddress(string key, System.Type type)
        {
            foreach (var l in Addressables.ResourceLocators)
            {
                IList<IResourceLocation> locs;
                if (l.Locate(key, type, out locs) && locs.Count > 0)
                    return locs[0].PrimaryKey;
            }
            return key;
        }

        public void GetSprite(string location, System.Action<Sprite> onLoaded)
        {
            var asyncOp = Addressables.LoadAssetAsync<Sprite>(location);
            asyncOp.Completed += handle => {
                if (handle.Result == null)
                    throw new KeyNotFoundException(location);
                onLoaded.Invoke(handle.Result);
            };
        }

#if UNITY_EDITOR || UNITY_ANDROID
        public void OnGUI()
        {
            var evt = Event.current;
            if (evt != null && evt.type == EventType.KeyUp)
            {
                if (evt.keyCode == KeyCode.Escape)
                {
                    for (var i = _viewStacks.Count - 1; i >= 0; --i)
                    {
                        var stack = _viewStacks.Values[i];
                        if (stack.Escape())
                            return;
                    }
                }
            }
        }
#endif

        private SortedList<float, ViewStack> _viewStacks = new SortedList<float, ViewStack>();

        public ViewStack GetStack(float id)
        {
            _viewStacks.TryGetValue(id, out var stack);
            return stack;
        }

        public IList<ViewStack> GetStacks() => _viewStacks.Values;

        public View GetTopMost(float id)
        {
            _viewStacks.TryGetValue(id, out var stack);
            return stack ? stack.Peek() : default;
        }

        public bool IsTop(View view, float id = 0)
        {
            var top = GetTopMost(id);
            if (!view || !view.isActiveAndEnabled || !top || !top.isActiveAndEnabled)
                return false;
            return top.GetInstanceID() == view.GetInstanceID();
        }

        public bool IsTop(string name, float id = 0)
        {
            var view = GetTopMost(id);
            return view != null && view.name == name;
        }

        public void Add(ViewStack stack)
        {
            if (!_viewStacks.TryGetValue(stack.Id, out var existing))
            {
                _viewStacks.Add(stack.Id, stack);
            }
            else if (!ReferenceEquals(existing, stack))
            {
                var id = stack.Id;
                var newPath = stack.gameObject.GetHierarchy();
                var oldPath = existing.gameObject.GetHierarchy();
                Debug.LogError($"viewStack.Id:{id} already exists. {newPath}, {oldPath}");
            }
        }

        public bool Remove(ViewStack stack)
        {
            if (_viewStacks.TryGetValue(stack.Id, out var existing))
            {
                if (ReferenceEquals(stack, existing))
                {
                    _viewStacks.Remove(stack.Id);
                    return true;
                }
            }
            return false;
        }

        public T AttachView<T>(T prefab, float stackId) where T : View
        {
            if (!_viewStacks.TryGetValue(stackId, out var stack))
                return default;
            
            var instance = Service.GoPool.Rent(prefab, stack.transform);
            stack.Push(instance);
            return instance;
        }

        public void GetAllViews(List<View> views)
        {
            foreach (var stack in _viewStacks.Values)
            {
                foreach (View view in stack)
                    views.Add(view);
            }
        }

        public void GetAllViewNames(List<string> viewNames)
        {
            foreach (var stack in _viewStacks.Values)
            {
                foreach (View view in stack)
                    viewNames.Add(view.name);
                foreach (var link in stack.AddressLinks)
                    viewNames.Add(link.name);
            }
        }

        public void GetViews<T>(string name, List<T> views, float stackId) where T : View
        {
            _viewStacks.TryGetValue(stackId, out var stack);

            if (stack == null)
                return;

            views.AddRange(stack.Where(arg => arg.name == name && arg is T).Cast<T>());
        }

        public T GetView<T>(float stackId = 0) where T : View
        {
            var view = (T)GetView(typeof(T).Name, stackId) as T;
            if (view == null)
            {
                var stack = GetStack(stackId);
                var hit = stack.FirstOrDefault(arg => arg is T);
                if (hit != null)
                    view = hit as T;
            }
            return view;
        }

        public View GetView(string name, float id)
        {
            _viewStacks.TryGetValue(id, out var stack);
            if (stack == null) return null;
            var view = stack.GetView(name);
            return view;
        }

        public View GetView(string name)
        {
            foreach (var stack in _viewStacks.Values)
            {
                var view = stack.GetView(name);
                if (view)
                    return view;
            }
            return null;
        }

        private AsyncOperationHandle GetViewAsset(string name)
        {
            var link = GetAddressLink(name);
            if (link.OperationHandle.IsValid())
            {
                return link.OperationHandle;
            }
            var handle = link.LoadAssetAsync();
            return handle;
        }

        private IAddressLink<GameObject> GetAddressLink(string name)
        {
            foreach (var stack in _viewStacks.Values)
            {
                var link = stack.GetAddressLink(name);
                if (link != null)
                    return link;
            }
            return null;
        }
        
        public bool IsActive(string name)
        {
            var view = GetView(name);
            return view.IsActive();
        }

        private List<IViewValidator> validators = new List<IViewValidator>();

        public bool AddValidator(IViewValidator validator)
        {
            if (validators.Contains(validator))
                return false;
            validators.Add(validator);
            return true;
        }

        public bool RemoveValidator(IViewValidator validator)
        {
            var c = validators.Count;
            for (var i = validators.Count - 1; i >= 0; --i)
            {
                var arg = validators[i];
                if (arg == null || ReferenceEquals(validator, arg))
                {
                    validators.RemoveAt(i);
                    continue;
                }
            }
            return validators.Count != c;
        }

        private bool CanOpen(View view)
        {
            for (var i = validators.Count - 1; i >= 0; --i)
            {
                var validator = validators[i];
                if (validator == null)
                {
                    validators.RemoveAt(i);
                    continue;
                }

                if (!validator.CanOpen(view))
                    return false;
            }
            return true;
        }

        private View OpenInternal(string name, ViewContext context, bool bypassValidation = false)
        {
            foreach (var stack in _viewStacks.Values)
            {
                var view = stack.GetView(name);
                if (!view)
                    continue;
                try
                {
                    if (!bypassValidation && !CanOpen(view))
                        continue;
                    view.Open(context);
                    return view;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.StackTrace + "\n" + e.Message);
                    view.CloseImmediate();
                }
            }
            return null;
        }

        public View EnforceOpen(string name, params object[] args)
        {
            return OpenInternal(name, new ViewContext(args), true);
        }

        public View Open(string name, params object[] args)
        {
            return OpenInternal(name, new ViewContext(args));
        }

        public async Task<View> OpenAsync(string name, params object[] args)
        {
            var view = GetView(name);
            if (view == null)
            {
                var handle = GetViewAsset(name);
                if (!handle.IsDone)
                    await handle.Task;
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    return null;
                var result = (GameObject)handle.Result;
                var prefab = result.GetComponent<View>();
                view = AttachView(prefab, 0);
                if (view == null)
                    return default;
            }
            
            view.Open(args);
            return view;
        }

        public IEnumerator OpenAsyncCRT(string name, object[] args, System.Action<View> onComplete)
        {
            var view = GetView(name);
            if (view == null)
            {
                var handle = GetViewAsset(name);
                yield return handle;
                var result = (GameObject)handle.Result;
                var prefab = result.GetComponent<View>();
                view = AttachView(prefab, 0);
                if (view == null)
                    yield break;
            }

            view.Open(args);
            onComplete?.Invoke(view);
        }

        public void Redraw()
        {
            foreach (var stack in _viewStacks.Values)
            {
                foreach (View view in stack)
                {
                    if (!view.isActiveAndEnabled)
                        break;
                    view.Redraw();
                }
            }
        }

        public void Close(string name)
        {
            foreach (var stack in _viewStacks.Values)
            {
                var view = stack.FirstOrDefault(arg => arg.name == name);
                if (view) view.Close();
            }
        }

        public View GetActiveTopView(System.Type[] excludeTypes = null)
        {
            foreach (var stack in _viewStacks.Values)
            {
                var view = stack.GetTop();
                if (view == null)
                    continue;
                if (excludeTypes == null || !excludeTypes.Contains(view.GetType()))
                    return view;
            }

            return null;
        }
    }
}