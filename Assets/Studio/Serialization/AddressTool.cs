using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Studio.Common
{
    public static class AddressTool
    {
        public static string GetPath(this AssetReferenceScene assetRef)
        {
            foreach (var l in Addressables.ResourceLocators)
            {
                IList<IResourceLocation> locations;
                if (l.Locate(assetRef.RuntimeKey, typeof(SceneInstance), out locations))
                {
                    if (locations.Count > 0)
                        return locations[0].PrimaryKey;
                }
            }
            return string.Empty;
        }

        public static bool IsExist(string address)
        {
            var asyncOp = Addressables.LoadResourceLocationsAsync(address);
            asyncOp.WaitForCompletion();
            var result = asyncOp.Result;
            Addressables.Release(asyncOp);
            return result != null && result.Count > 0;
        }

        public static async Task<bool> IsExistAsync(string address)
        {
            var asyncOp = Addressables.LoadResourceLocationsAsync(address);
            await asyncOp.Task;
            var result = asyncOp.Result;
            Addressables.Release(asyncOp);
            return result != null && result.Count > 0;
        }

        public static bool FindLoadedScene(this AssetReferenceScene assetRef, out Scene scene)
        {
            var path = assetRef.GetPath();
            return FindLoadedScene(path, out scene);
        }

        public static bool FindLoadedScene(string path, out Scene scene)
        {
            for (var i = 0; i < SceneManager.sceneCount; ++i)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.path == path)
                {
                    scene = s;
                    return true;
                }
            }
            scene = default;
            return false;
        }

        public static Scene GetLoadedScene(this AssetReferenceScene sceneRef)
        {
            if (sceneRef.OperationHandle.IsValid())
            {
                var sceneInstacne = (SceneInstance)sceneRef.OperationHandle.Result;
                return sceneInstacne.Scene;
            }
            return default;
        }

        public static bool IsLoaded(this AssetReferenceScene sceneRef)
        {
            var scene = sceneRef.GetLoadedScene();
            return scene.IsValid() && scene.isLoaded;
        }

        public static string GetAddress(this AssetReference assetRef, System.Type type)
        {
            var key = (string)assetRef.RuntimeKey;
            return AddressTool.GetAddress(key, type);
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
        
        public static bool HasAddress(string guid)
        {
            #if UNITY_EDITOR
            var type = System.AppDomain.CurrentDomain.GetAssemblies()
                .Where(arg => arg.FullName.StartsWith("Studio.Framework.Editor"))
                .SelectMany(arg => arg.GetExportedTypes())
                .Where(arg => {
                    return 
                    arg.FullName.StartsWith("Studio.Framework.") &&
                    arg.FullName.EndsWith(".EditorExtension"); })
                .FirstOrDefault();
            var methodInfo = type.GetMethod("HasAddress");
            return (bool)methodInfo.Invoke(null, new[] { guid });
            #else
            UnityEngine.Assertions.Assert.IsTrue(Application.isEditor);
            return false;
            #endif
        }

        private static System.Reflection.MethodInfo _MakeAddressFromGUID;
        public static void MakeAddressFromGUID(string guid)
        {
            #if UNITY_EDITOR
            if (_MakeAddressFromGUID == null)
            {
                var type = System.AppDomain.CurrentDomain.GetAssemblies()
                    .Where(arg => arg.FullName.StartsWith("Studio.Serialization.Editor"))
                    .SelectMany(arg => arg.GetExportedTypes())
                    .Where(arg =>
                        arg.FullName == "Studio.Common.EditorInternal.EditorExtension")
                    .FirstOrDefault();
                _MakeAddressFromGUID = type.GetMethod("MakeAddressFromGUID");
            }
            _MakeAddressFromGUID.Invoke(null, new[] { guid });
            #endif
        }

        public static void EnsureCleanOperation<T>(this T assetRef) where T : AssetReference
        {
            if (assetRef.IsValid())
                assetRef.ReleaseAsset();
        }

        public static async void GetOrLoad<T>(this IAddressLink<T> assetRef, System.Action<T> onComplete) where T : Object
        {
            var asset = await assetRef.GetOrLoadAsync();
            onComplete.Invoke(asset);
        }

        public static async Task<T> GetOrLoadAsync<T>(this IAddressLink<T> assetRef) where T : Object
        {
            if (assetRef.IsValid())
            {
                if (!assetRef.IsDone)
                    await assetRef.OperationHandle.Task;

                var asset = assetRef.Asset;
                if (asset)
                    return asset;
                
                assetRef.ReleaseAsset();
            }
            return await assetRef.LoadAssetAsync();
        }

        public static TaskAwaiter<T> GetAwaiter<T>(this AsyncOperationHandle<T> handle) where T : Object
        {
            try
            {
                return handle.Task.GetAwaiter();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
                return default;
            }
        }

        public static int GetRefCount<T>(this AsyncOperationHandle<T> handle)
        {
            var propInfo = handle.GetType().GetProperty("ReferenceCount",
                System.Reflection.BindingFlags.Instance|
                System.Reflection.BindingFlags.NonPublic);
            if (propInfo != null)
            {
                return (int)propInfo.GetValue(handle);
            }
            return 0;
        }
    }
}