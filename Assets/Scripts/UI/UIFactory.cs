using UnityEngine;

namespace UI
{
    public static class UIFactory<T> where T : UIBase
    {
        public static T Create()
        {
            var prefab = Resources.Load<GameObject>($"Prefabs/ui/{typeof(T).Name}");

            if (prefab == null)
            {
                Debug.LogWarning($"Cant Find UI Prefab {typeof(T).Name}");
                return null;
            }

            var uiObj = GameObject.Instantiate(prefab, UIManager.Instance.transform);
            var ui = uiObj.GetComponent<T>();
            if (ui == null)
            {
                Debug.LogWarning($"Cant Find UI Component in prefab {typeof(T).Name}");
                return null;
            }

            UIManager.Instance.PushUI(ui);
            return ui;
        }
    }
}