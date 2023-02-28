using System;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;

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
            
            var prefab = Resources.Load<GameObject>($"{prefabResourceDefine.Path}");

            if (prefab == null)
            {
                Debug.LogWarning($"Cant Find UI Prefab {tType.Name}");
                return null;
            }

            return GameObject.Instantiate(prefab, parent).GetComponent<T>();
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
            
            var request = Resources.LoadAsync<GameObject>($"{prefabResourceDefine.Path}");
            
            await YieldCoroutine.WaitForInstruction(request);
            
            if (request.asset == null)
            {
                return null;
            }

            var prefab = request.asset as GameObject;
                
            return GameObject.Instantiate(prefab, parent).GetComponent<T>();
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class PrefabResource : Attribute {
        public string Path {get; set;}
        public PrefabResource(string path) {
            Path = path;
        }
    }
}