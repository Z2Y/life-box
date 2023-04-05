using System;
using System.Collections.Generic;
using Cathei.LinqGen;
using UnityEngine.Pool;

namespace Utils
{
    // Not Thread Safe
    public class SimplePool
    {
        private readonly ObjectPool<PoolObject> pool;

        private readonly Dictionary<long, PoolObject> used = new ();
        private readonly Dictionary<long, PoolObject> unused = new();

        private readonly Type objectType;

        public SimplePool(Type objectType)
        {
            this.objectType = objectType;
            pool = new ObjectPool<PoolObject>(CreateFunc, actionOnGet: ActionOnGet, actionOnRelease: ActionOnRelease, collectionCheck: false);
        }

        private PoolObject CreateFunc()
        {
            return Activator.CreateInstance(objectType) as PoolObject;
        }

        public T Get<T>() where T : PoolObject
        {
            return pool.Get() as T;
        }
        
        private void ActionOnGet(PoolObject obj)
        {
            used.TryAdd(obj.ID, obj);
            unused.Remove(obj.ID);
        }
        
        private void ActionOnRelease(PoolObject obj)
        {
            unused.TryAdd(obj.ID, obj);
            used.Remove(obj.ID);
        }

        public void Return(PoolObject element)
        {
            if (!unused.ContainsKey(element.ID))
            {
                pool.Release(element);
            }
            unused.TryAdd(element.ID, element);
            used.Remove(element.ID);
        }

        public void ReleaseUnused()
        {
            unused.Clear();
            pool.Clear();
        }

        public void ReturnUsedIf<T>(Predicate<T> match)
        {
            var toRelease = used.Gen().Where((pair) => pair.Value is T value && match(value)).ToArray();
            
            foreach (var item in toRelease)
            {
                item.Value.Dispose();
            }
        }

        public int Count => used.Count + unused.Count;

        public int ActiveCount => used.Count;

        public int InActiveCount => unused.Count;
    }

    public abstract class PoolObject
    {
        public readonly long ID;

        public virtual void Dispose()
        {
            SimplePoolManager.Return(this);
        }

        protected PoolObject()
        {
            ID = IDGenerator.GenerateInstanceId();
        }
    }
}