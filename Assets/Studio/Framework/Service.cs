using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Studio.Framework;
using System.Linq;
using UniRx;
using MEC;
using Studio.Common;
using UnityEngine.ResourceManagement.AsyncOperations;
using Studio.Framework.SceneBuilding;

namespace Studio
{
    [DisallowMultipleComponent]
    public class Service : MonoBehaviour
    {
        public static bool ready { get; private set; }

        public static Service Instance 
        {
            get;
            private set;
        }

        [SerializeField]
        private SoftLink<ContentConfig> _updateContent;

        public static GameObject GameObject => Instance.gameObject;

        private readonly IDictionary<int, Object> pool = new SortedList<int, Object>();

        private readonly IDictionary<int, System.IServiceProvider> provider = new Dictionary<int, System.IServiceProvider>();

        public const string ScenePath = "Assets/Scenes/Service.unity";

        public static IEnumerator Init()
        {
            if (Instance)
            {
                yield break;
            }
            if (AlreadyExist() == false)
            {
                yield return Addressables.InitializeAsync();
                yield return SceneManager.LoadSceneAsync("Service", LoadSceneMode.Additive);
            }
            
            while (Instance == null)
            {
                yield return null;
            }

#if false
            Addressables.ResourceManager.ClearDiagnosticCallbacks();
            Addressables.ResourceManager.RegisterDiagnosticCallback(c =>
            {
                if (c.Type == UnityEngine.ResourceManagement.ResourceManager.DiagnosticEventType.AsyncOperationFail)
                {
                    var h = c.OperationHandle;
                    Debug.LogWarning($"[AsyncOperationFail] {h.DebugName}, Exception:{h.OperationException}");
                }
                else if (c.Type == UnityEngine.ResourceManagement.ResourceManager.DiagnosticEventType.AsyncOperationCreate)
                {
                    var h = c.OperationHandle;
                    Debug.LogWarning($"[AsyncOperationCreate] {h.DebugName}");
                }
                else if (c.Type == UnityEngine.ResourceManagement.ResourceManager.DiagnosticEventType.AsyncOperationComplete)
                {
                    var h = c.OperationHandle;
                    Debug.LogWarning($"[AsyncOperationComplete] {h.DebugName}");
                }
            });
#endif

            var loading = Instance.LoadUpdateContents();
            yield return CoroutineBridge.WaitInflation(Instance.gameObject.scene);
            yield return loading;
        }

        public Coroutine LoadUpdateContents()
        {
            return StartCoroutine(LoadUpdateContentsAsync());
        }

        private IEnumerator LoadUpdateContentsAsync()
        {
            var opList = new List<AsyncOperationHandle<ContentConfig>>();

            var asyncOp = _updateContent.LoadAssetAsync();
            asyncOp.Completed += h =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    AddAsset(h.Result);
                    foreach (var config in h.Result.list)
                        AddAsset(config);
                }
                else
                {
                    Debug.LogError($"LoadUpdateContents:{_updateContent.name}:{h.Status}");
                }
            };
            yield return asyncOp;
        }

        private static bool AlreadyExist()
        {
#if UNITY_EDITOR
            for (var i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.path == ScenePath)
                    return true;
            }
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Additive));
            return true;
#else
            return false;
#endif
        }

        private ScriptableObject[] _persistentContents = new ScriptableObject[0];

        public static void AddAsset(Object asset)
        {
            var dict = Instance.pool;
            var k = asset.GetType().GetHashCode();
            dict[k] = asset;
        }

        private static object GetAsset(int key)
        {
            Instance.pool.TryGetValue(key, out var instance);
            return instance;
        }

        public static T GetAsset<T>() where T : ScriptableObject
        {
#if UNITY_EDITOR
            if (Instance == null)
            {
                var findArg = $"t:{typeof(T).Name}";
                var path = UnityEditor.AssetDatabase.FindAssets(findArg)
                    .Select(UnityEditor.AssetDatabase.GUIDToAssetPath).FirstOrDefault();
                if (string.IsNullOrEmpty(path) == false)
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                return default(T);
            }
#endif
            var dict = Instance.pool;
            var obj = GetAsset(typeof(T).GetHashCode());
            if (obj != null)
                return obj as T;
            return default;
        }

        public static IEnumerable<T> GetAssets<T>() where T : class
        {
            return Instance.pool.Values.Select(arg => arg as T).Where(arg => arg != null);
        }

        public static void AddProvider(System.IServiceProvider provider)
        {
            var dict = Instance.provider;
            var hash = provider.GetType().GetHashCode();

            if (!dict.ContainsKey(hash))
            {
                dict.Add(hash, provider);
            }
        }

        public static T GetProvider<T>() where T : System.IServiceProvider
        {
            var dict = Instance ? Instance.provider : default;

            if (null != dict)
            {
                var hash = typeof(T).GetHashCode();

                if (dict.ContainsKey(hash))
                {
                    return (T)dict[hash];
                }
            }

            return default;
        }

        public static void Reboot(bool clear = true)
        {
            Timing.RunCoroutine(RebootAsync(clear));
        }

        private static IEnumerator<float> RebootAsync(bool clear)
        {
            if (!ready)
                yield break;
            ready = false;

            yield return Timing.WaitUntilDone(SceneManager.LoadSceneAsync("Reboot", LoadSceneMode.Additive));

            Instance.Clear();
            Destroy(Instance.gameObject);
            for (var i = SceneManager.sceneCount-1; i >= 0; --i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name != "Reboot" && (clear || scene.name != "Service"))
                {
                    yield return Timing.WaitUntilDone(SceneManager.UnloadSceneAsync(scene));
                }
            }

            yield return Timing.WaitUntilDone(Resources.UnloadUnusedAssets());

#if UNITY_EDITOR
            UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();
#endif
            var startOp = SceneManager.LoadSceneAsync(1);
            yield return Timing.WaitUntilDone(startOp);

            yield return Timing.WaitForSeconds(0.3f);

            // fix:ArgumentException: Scene to unload is invalid
            var reboot = SceneManager.GetSceneByName("Reboot");
            if (reboot.IsValid())
                yield return Timing.WaitUntilDone(SceneManager.UnloadSceneAsync(reboot));
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        public static void DeactivateUselessCamera()
        {
            DeactivateUselessCamera(default);
        }
#endif

        public static void DeactivateUselessCamera(Scene scene)
        {
            var cameras = new Camera[Camera.allCamerasCount];
            var count = Camera.GetAllCameras(cameras);
            for (var i = 0; i < count; ++i)
            {
                var c = cameras[i];
                if ((scene.IsValid() && c.gameObject.scene.Equals(scene)) || c.CompareTag("EditorOnly"))
                {
                    var listener = c.GetComponent<AudioListener>();
                    if (listener)
                        DestroyImmediate(listener);
                    c.gameObject.SetActive(false);
                }
            }
        }

        public static void ActivateRoots(string sceneName, bool toggle)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            ActivateRoots(scene, toggle);
        }

        public static void ActivateRoots(Scene scene, bool toggle)
        {
            foreach (var go in scene.GetRootGameObjects())
                go.SetActive(toggle);
        }

        public static GoPool GoPool => Instance.goPool;
        
        public static UIManager UIManager => Instance.uiManager;

        private class NullMessageBroker : IMessageBroker
        {
            public void Publish<T>(T message) { }

            public System.IObservable<T> Receive<T>() { return new UniRx.Subject<T>(); }
        }

        public static IMessageBroker Message => ready ? Instance.message : new NullMessageBroker();

        public static ImageCache ImageCache => Instance.imageCache;
        
        public static Sound Sound => Instance.sound;
        
        public static Native Native => Instance.native;

        public static IAsyncMessageBroker AsyncMessage => Instance.asyncMessage;

        public static AddressCache Cache => Instance.cache;

        private GoPool goPool;
        private UIManager uiManager;
        private ImageCache imageCache;
        private IMessageBroker message;
        private IAsyncMessageBroker asyncMessage;
        private Sound sound;
        private Native native;
        private AddressCache cache;

        protected void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            ready = true;
            Instance = this;

            Davinci.SetFilePath(FileTool.GetResourceDir() + "/images/");
            FontConfig.Init("Assets/Configs/FontConfig.asset");
            //Davinci.ClearAllCachedFiles();
            goPool = gameObject.GetOrNewComponent<GoPool>();
            uiManager = gameObject.GetOrNewComponent<UIManager>();
            imageCache = gameObject.GetOrNewComponent<ImageCache>();
            sound = gameObject.GetOrNewComponent<Sound>();
            native = gameObject.GetOrNewComponent<Native>();
            cache = GetComponent<AddressCache>();

            message = new MessageBroker();
            asyncMessage = new AsyncMessageBroker();
            foreach (var asset in _persistentContents)
                AddAsset(asset);
        }

        protected void OnDestroy()
        {
            Debug.Log($"Service.OnDestroy");
            ready = false;
            Clear();
            
            if (message != null)
                ((System.IDisposable)message).Dispose();
            message = null;
            if (asyncMessage != null)
                ((System.IDisposable)asyncMessage).Dispose();
            asyncMessage = null;
            FontConfig.ReleaseAsset();
        }

        public bool Clear()
        {
            if (ReferenceEquals(Instance, this) == false)
                return false;
            
            StaticInstance.Purge();

            foreach (var instance in pool.Values)
            {
                var component = instance as Component;
                if (component)
                    Destroy(component);
            }

            _updateContent.ReleaseAsset();

            pool.Clear();
            provider.Clear();
            
            return true;
        }

        public static AsyncOperation Unload()
        {
            if (GameObject)
                return SceneManager.UnloadSceneAsync(GameObject.scene);
            return null;
        }

        public static bool applicationIsQuitting => Instance ? Instance._applicationIsQuitting : false;

        private bool _applicationIsQuitting;

        public void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
