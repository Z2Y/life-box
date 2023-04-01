using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class SimplePoolManager
    {
        private static readonly Dictionary<Type, SimplePool> pools = new();

        public static T Get<T>() where T : PoolObject
        {
            return createOrGetPool(typeof(T)).Get<T>();
        }

        public static void Return<T>(T element) where  T : PoolObject
        {
            createOrGetPool(typeof(T)).Return(element);
        }

        private static SimplePool createOrGetPool(Type type)
        {
            if (pools.TryGetValue(type, out var pool))
            {
                return pool;
            }

            pool = new SimplePool(type);
            pools.TryAdd(type, pool);
            return pool;
        }

        public static void ReturnUsedIf<T>(Predicate<T> match)
        {
            createOrGetPool(typeof(T)).ReturnUsedIf(match);
        }

        public static void ReleaseUnused()
        {
            foreach (var pool in pools.Values)
            {
                pool.ReleaseUnused();
            }
        }
    }

    public class SimplePoolAutoRelease : MonoBehaviour
    {
        private void Awake()
        {
            Application.lowMemory += onLowMemory;
        }

        private static void onLowMemory()
        {
            SimplePoolManager.ReleaseUnused();
        }
    }
}