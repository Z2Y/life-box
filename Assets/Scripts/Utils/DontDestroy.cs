using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class DontDestroy : MonoBehaviour
    {
        public bool uniq;

        private static readonly Dictionary<string, bool> instances = new();
        private void Awake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                UnityEditor.SceneVisibilityManager.instance.Show(gameObject, false);
#endif
            if (uniq && instances.ContainsKey(gameObject.name))
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                instances.TryAdd(gameObject.name, true);
            }

        }
    }
}