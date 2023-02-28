using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Utils
{
    public class PrefabPool<T> where T : MonoBehaviour
    {
        private Transform defaultRoot;

        private readonly ObjectPool<T> pool;

        PrefabPool()
        {
            pool = new (
                createFunc: createFromLoader, 
                actionOnGet: onGetFromPool,
                actionOnRelease: onReturnToPool,
                actionOnDestroy: onDestroy,
                collectionCheck: false);
        }

        private T createFromLoader()
        {
            var obj = PrefabLoader<T>.Create(defaultRoot);
            if (obj == null)
            {
                throw new Exception("Create From Prefab Failed.");
            }

            return obj;
        }

        private void onGetFromPool(T obj)
        {
            obj.gameObject.SetActive(true);
            obj.transform.SetParent(defaultRoot);
        }

        private void onReturnToPool(T obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void onDestroy(T obj)
        {
            Object.Destroy(obj);
        }

        public void SetTransform(Transform parent)
        {
            defaultRoot = parent;
        }

        public T Get()
        {
            var obj = pool.Get();
            return obj;
        }

        public async Task<T> GetAsync()
        {
            if (pool.CountInactive <= 0)
            {
                var obj = await PrefabLoader<T>.CreateAsync(defaultRoot);
                if (obj == null)
                {
                    throw new Exception("Create From Prefab Failed.");
                }
                Return(obj);
            }

            return Get();
        }

        public void Return(T obj)
        {
            pool.Release(obj);
        }

        public void Clear()
        {
            pool.Clear();
        }

        public void Dispose()
        {
            pool.Dispose();
        }
        
    }
}