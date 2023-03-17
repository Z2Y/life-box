using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Utils
{
    public class PrefabPool<T> where T : MonoBehaviour
    {
        private Transform _root;
        private Transform Root 
        {
            get {
                if (ReferenceEquals(_root, null))
                {
                    setUpDefaultRoot();
                }

                return _root;
            }
            set => _root = value;
        }

        private readonly ObjectPool<T> pool;
        private readonly HashSet<T> used = new();
        private readonly HashSet<T> unused = new();

        public PrefabPool()
        {
            pool = new ObjectPool<T>(
                createFunc: createFromLoader, 
                actionOnGet: onGetFromPool,
                actionOnRelease: onReturnToPool,
                actionOnDestroy: onDestroy,
                collectionCheck: false);

        }

        private void setUpDefaultRoot()
        {
            var gameObj = new GameObject($"{typeof(T).Name}Pool");
            gameObj.transform.SetParent(GameObject.Find("PrefabPool").transform);
            _root = gameObj.transform;
        }

        private T createFromLoader()
        {
            var obj = PrefabLoader<T>.Create(Root);
            if (obj == null)
            {
                throw new Exception("Create From Prefab Failed.");
            }

            return obj;
        }

        private void onGetFromPool(T obj)
        {
            used.Add(obj);
            unused.Remove(obj);
            obj.gameObject.SetActive(true);
            obj.transform.SetParent(Root);
        }

        private void onReturnToPool(T obj)
        {
            used.Remove(obj);
            unused.Add(obj);
            obj.transform.SetParent(Root);
            obj.gameObject.SetActive(false);
        }

        private void onDestroy(T obj)
        {
            Object.Destroy(obj);
        }

        public void SetTransform(Transform parent)
        {
            Root = parent;
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
                var obj = await PrefabLoader<T>.CreateAsync(Root);
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
            if (!unused.Contains(obj))
            {
                pool.Release(obj);
            }
            used.Remove(obj);
        }
        
        public void RecycleUsed()
        {
            var recycles = used.ToList();
            foreach (var item in recycles)
            {
                Return(item);
            }
        }

        public void Clear()
        {
            RecycleUsed();
            pool.Clear();
            unused.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
        
    }
    
    public class PrefabPool<T, T2> where T : MonoBehaviour, IOnPrefabLoaded<T2>
    {
        private Transform _root;
        private Transform Root 
        {
            get {
                if (ReferenceEquals(_root, null))
                {
                    setUpDefaultRoot();
                }

                return _root;
            }
            set => _root = value;
        }

        // private readonly ObjectPool<T> pool;

        private readonly Dictionary<T2, ObjectPool<T>> pool = new ();
        private readonly HashSet<KeyValuePair<T, T2>> used = new();
        private readonly HashSet<KeyValuePair<T, T2>> unused = new();


        private void setUpDefaultRoot()
        {
            var root = new GameObject($"{typeof(T).Name}Pool");
            root.transform.SetParent(GameObject.Find("PrefabPool").transform);
            _root = root.transform;
        }

        private ObjectPool<T> createPoolWithArg(T2 arg)
        {
            var poolWithArg = new ObjectPool<T>(
                createFunc: () => createFromLoader(arg), 
                actionOnGet: (obj) => onGetFromPool(obj, arg),
                actionOnRelease: (obj) => onReturnToPool(obj, arg),
                actionOnDestroy: onDestroy,
                collectionCheck: false);
            pool.Add(arg, poolWithArg);
            return poolWithArg;
        }

        private T createFromLoader(T2 arg)
        {
            var obj = PrefabLoader<T, T2>.Create(arg, Root);
            if (ReferenceEquals(obj, null))
            {
                throw new Exception("Create From Prefab Failed.");
            }

            return obj;
        }

        private void onGetFromPool(T obj, T2 arg)
        {
            var pair = new KeyValuePair<T, T2>(obj, arg);
            used.Add(pair);
            unused.Remove(pair);
            obj.gameObject.SetActive(true);
            obj.transform.SetParent(Root);
        }

        private void onReturnToPool(T obj, T2 arg)
        {
            var pair = new KeyValuePair<T, T2>(obj, arg);
            used.Remove(pair);
            unused.Add(pair);
            obj.transform.SetParent(Root);
            obj.gameObject.SetActive(false);
        }

        private static void onDestroy(T obj)
        {
            Object.Destroy(obj);
        }

        public void SetTransform(Transform parent)
        {
            Root = parent;
        }

        public T Get(T2 arg)
        {
            if (pool.TryGetValue(arg, out var poolWithArg))
            {
                return poolWithArg.Get();
            }
            else
            {
                return createPoolWithArg(arg).Get();
            }
        }

        public async Task<T> GetAsync(T2 arg)
        {
            if (!pool.TryGetValue(arg, out var poolWithArg))
            {
                poolWithArg = createPoolWithArg(arg);
            }

            if (poolWithArg.CountInactive <= 0)
            {
                var obj = await PrefabLoader<T, T2>.CreateAsync(arg, Root);
                if (obj == null)
                {
                    throw new Exception("Create From Prefab Failed.");
                }
                Return(obj, arg);
            }

            return poolWithArg.Get();
        }

        public void Return(T obj, T2 arg)
        {
            var pair = new KeyValuePair<T, T2>(obj, arg);
            if (!unused.Contains(pair))
            {
                if (pool.TryGetValue(arg, out var poolWithArg))
                {
                    poolWithArg.Release(obj);
                }
            }
            used.Remove(pair);
        }

        public void Clear()
        {
            RecycleUsed();
            foreach (var keyValuePair in pool)
            {
                keyValuePair.Value.Clear();
            }

            pool.Clear();
        }

        public void RecycleUsed()
        {
            var recycles = used.ToList();
            foreach (var item in recycles)
            {
                Return(item.Key, item.Value);
            }
        }

        public void Dispose()
        {
            Clear();
        }
        
    }
}