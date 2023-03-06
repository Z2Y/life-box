using UnityEngine;
using UnityEngine.Pool;

namespace Utils
{
    public class SimplePool<T> where T : class, new()
    {
        public static readonly ObjectPool<T> Pool = new (CreateFunc);
        

        protected SimplePool()
        {
        }

        private static T CreateFunc()
        {
            return new T();
        }

        public static T Get()
        {
            return Pool.Get();
        }

        public void Return(T element)
        {
            Pool.Release(element);
        }
    }
    
    
}