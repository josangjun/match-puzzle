using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Framework
{
    [DisallowMultipleComponent]
    public class GoPool : MonoBehaviour
    {
        private class Entity : Stack<GoItem>
        {
            public string RuntimeKey { get; set; }
        }

        private IDictionary<string, Entity> _entities = new SortedList<string, Entity>();

        private GoItem GetStock(string key)
        {
            GoItem item = default;
            var entity = GetEntity(key);

            if (entity.Count > 0)
            {
                item = entity.Pop();
            }
            return item;
        }

        public async Task<T> RentAsync<T>(string address, Transform parent = null) where T : GoItem
        {
            if (RentStockItem<T>(address, parent, out var item))
                return item;

            var asyncOp = Addressables.InstantiateAsync(address, new Vector3(0, 9999), Quaternion.identity, parent);

            await asyncOp.Task;

            if (asyncOp.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"Rent({asyncOp.Status}):{address}");
                return null;
            }

            var go = asyncOp.Result;
            item = go.GetComponent<T>();
            item.itemKey = address;
            OnRent(item);
            item.OnRent();

            return item;
        }

        public T Rent<T>(string address, Transform parent = null) where T : GoItem
        {
            if (RentStockItem<T>(address, parent, out var item))
                return item;

            var asyncOp = Addressables.InstantiateAsync(address, new Vector3(0, 0), Quaternion.identity, parent);

            asyncOp.WaitForCompletion();

            if (asyncOp.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"Rent({asyncOp.Status}):{address}");
                return null;
            }

            var go = asyncOp.Result;
            item = go.GetComponent<T>();
            item.itemKey = address;
            OnRent(item);
            item.OnRent();

            return item;
        }

        private bool RentStockItem<T>(string address, Transform parent, out T item) where T : GoItem
        {
            item = GetStock(address) as T;
            if (!item)
                return false;
            
            item.transform.SetParent(parent);
            item.gameObject.SetActive(true);
            OnRent(item);
            item.OnRent();

            return true;
        }

        public T Rent<T>(T asset, Transform parent = null) where T : GoItem
        {
            if (asset == null || !Service.ready)
                return null;
            if (parent == null)
                parent = transform;

            if (RentStockItem<T>(asset.itemKey, parent, out var item))
                return item;

            item = Instantiate(asset, parent);
            item.name = asset.name;
            item.itemKey = asset.itemKey;
            
            OnRent(item);
            item.OnRent();
            
            return item;
        }

        private void OnRent(GoItem item)
        {
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.transform.localRotation = Quaternion.identity;
        }

        public void Return(GoItem item)
        {
            item.OnReturn();
            if (string.IsNullOrEmpty(item.itemKey))
                return;
            var stack = GetEntity(item.itemKey);
            if (stack == null || !Service.ready)
            {
                item.Destroy();
                return;
            }
            if (stack.Contains(item))
            {
                return;
            }
            stack.Push(item);
            item.gameObject.SetActive(false);
            item.transform.SetParent(transform);
            item.transform.localPosition = new Vector3(0, 0, 0);
            item.transform.localRotation = Quaternion.identity;
        }

        private Entity GetEntity(string key)
        {
            Entity entity;
            if (!_entities.TryGetValue(key, out entity) || entity == null)
                entity = _entities[key] = new Entity();
            return entity;
        }

        public void Clear()
        {
            foreach (var kv in _entities)
            {
                foreach (var item in kv.Value)
                {
                    item.Destroy();
                }
            }
            _entities.Clear();
        }
    }
}