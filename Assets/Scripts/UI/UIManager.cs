using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private readonly Dictionary<Type, object> loadingTasks = new();
        private readonly Dictionary<int, UIBase> _lookup = new();
        public static UIManager Instance { get; private set; }

        public Transform worldRoot;
        public Transform screenRoot;

        private void Awake()
        {
            Instance ??= this;
            
            // collect all ui in direct child
            for (var i = 0; i < screenRoot.childCount; i++)
            {
                var ui = screenRoot.GetChild(i).GetComponent<UIBase>();
                if (ui != null && ui.gameObject.activeSelf)
                {
                    PushUI(ui);
                }
            }
        }

        private void OnDestroy()
        {
            _lookup.Clear();
            loadingTasks.Clear();
            Instance = null;
        }

        public bool PushUI(UIBase ui)
        {
            ui.gameObject.SetActive(true);
            return _lookup.TryAdd(ui.GetInstanceID(), ui);
        }

        public bool PopupUI()
        {
            return false;
        }

        public UIBase FindByName(string uiName)
        {
            return _lookup.Values.FirstOrDefault((ui => ui.gameObject.name == uiName));
        }
        
        public T FindByType<T>()
        {
            return _lookup.Values.OfType<T>().FirstOrDefault();
        }

        public UIBase Remove(int instanceID)
        {
            if (!_lookup.TryGetValue(instanceID, out var ui)) return null;
            _lookup.Remove(instanceID);
            
            return ui;
        }

        public bool Hide(int instanceID)
        {
            if (!_lookup.TryGetValue(instanceID, out var ui)) return false;
            
            ui.gameObject.SetActive(false);
            return true;          
        }

        public UIBase FindOrCreate<T>() where T : UIBase
        {
            var exist = _lookup.Values.OfType<T>().FirstOrDefault();
            if (exist != null)
            {
                return exist;
            }
            return UIFactory<T>.Create();
        }
        
        public async UniTask<UIBase> FindOrCreateAsync<T>(bool useActive = false) where T : UIBase
        {
            var uiType = typeof(T);
            var exist = _lookup.Values.OfType<T>().FirstOrDefault(ui => (useActive || !ui.gameObject.activeSelf));
            if (exist != null)
            {
                return exist;
            }

            if (useActive && loadingTasks.ContainsKey(uiType))
            {
                return await (UniTask<UIBase>)loadingTasks[uiType];
            }

            var loading = UIFactory<T>.CreateAsync().Preserve();
            loadingTasks.TryAdd(uiType, loading);
            var ui = await loading;
            loadingTasks.Remove(uiType);
            return ui;
        }
        
    }
}