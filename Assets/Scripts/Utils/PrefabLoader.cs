using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils
{
    public static class PrefabLoader<T> where T : UnityEngine.Object
    {
        public static T Create(Transform parent)
        {
            var tType = typeof(T);
            if (!tType.IsDefined(typeof(PrefabResource)))
            {
                Debug.Log($"Type {tType.Name} is not supported.");
                return null;
            }

            var prefabResourceDefine = tType.GetCustomAttribute<PrefabResource>(); 
            
            var prefab = Resources.Load<GameObject>($"{prefabResourceDefine.Path()}");

            if (prefab == null)
            {
                Debug.LogWarning($"Cant Find Prefab {tType.Name}");
                return null;
            }

            return Object.Instantiate(prefab, parent).GetComponent<T>();
        }

        public static async Task<T> CreateAsync(Transform parent)
        {
            var tType = typeof(T);
            if (!tType.IsDefined(typeof(PrefabResource)))
            {
                Debug.Log($"Type {tType.Name} is not supported.");
                return null;
            }
            
            var prefabResourceDefine = tType.GetCustomAttribute<PrefabResource>();
            
            var request = Resources.LoadAsync<GameObject>($"{prefabResourceDefine.Path()}");
            
            await YieldCoroutine.WaitForInstruction(request);
            
            if (request.asset == null)
            {
                return null;
            }

            var prefab = request.asset as GameObject;
                
            return Object.Instantiate(prefab, parent).GetComponent<T>();
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class PrefabResource : Attribute
    {
        private readonly string _basePath;
        public string Path()
        {
            return _basePath;
        }

        public PrefabResource(string path) {
            _basePath = path;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class PrefabResourceWithArgs : PrefabResource
    {
        private string _basePath;

        public string Path(object args)
        {
            Debug.Log($"{_basePath} xxx {args}");
            return string.Format(_basePath, args);
        }

        public PrefabResourceWithArgs(string path) : base(path)
        {
            _basePath = path;
        }
    }
    
    public static class PrefabLoader<T, T2> where T : Object, IOnPrefabLoaded<T2>
    {
        public static T Create(T2 arg, Transform parent)
        {
            var tType = typeof(T);
            if (!tType.IsDefined(typeof(PrefabResourceWithArgs)))
            {
                Debug.Log($"Type {tType.Name} is not supported.");
                return null;
            }

            var prefabResourceDefine = tType.GetCustomAttribute<PrefabResourceWithArgs>(); 
            
            var prefab = Resources.Load<GameObject>($"{prefabResourceDefine.Path(arg)}");

            if (prefab == null)
            {
                Debug.LogWarning($"Cant Find Prefab {tType.Name}");
                return null;
            }

            return Object.Instantiate(prefab, parent).GetComponent<T>();
        }

        public static async Task<T> CreateAsync(T2 arg, Transform parent)
        {
            var tType = typeof(T);
            if (!tType.IsDefined(typeof(PrefabResourceWithArgs)))
            {
                Debug.Log($"Type {tType.Name} is not supported.");
                return null;
            }
            
            var prefabResourceDefine = tType.GetCustomAttribute<PrefabResourceWithArgs>();
            
            var request = Resources.LoadAsync<GameObject>($"{prefabResourceDefine.Path(arg)}");
            
            await YieldCoroutine.WaitForInstruction(request);
            
            if (request.asset == null)
            {
                return null;
            }

            var prefab = request.asset as GameObject;
                
            return Object.Instantiate(prefab, parent).GetComponent<T>();
        }
    }

    public interface IOnPrefabLoaded<T>
    {
        public void OnLoaded(T arg);
    }

}