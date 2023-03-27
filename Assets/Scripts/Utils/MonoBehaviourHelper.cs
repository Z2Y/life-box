using UnityEngine;

namespace Utils
{
    public static class MonoBehaviourHelper
    {
        public static T GetOrAddComponent<T>(this MonoBehaviour behaviour) where T : Component
        {
            return behaviour.GetComponent<T>() ?? behaviour.gameObject.AddComponent<T>();
        }
    }
}