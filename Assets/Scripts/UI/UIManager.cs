using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private readonly LinkedList<UIBase> uiBases = new();

        private readonly Dictionary<int, UIBase> _lookup = new();

        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            Instance ??= this;

            // collect all ui in direct child
            for (var i = 0; i < transform.childCount; i++)
            {
                var ui = transform.GetChild(i).GetComponent<UIBase>();
                if (ui != null)
                {
                    PushUI(ui);
                }
            }
        }

        public bool PushUI(UIBase ui)
        {
            ui.Show();
            uiBases.AddFirst(ui);
            return _lookup.TryAdd(ui.GetInstanceID(), ui);
        }

        public bool PopupUI()
        {
            if (uiBases.Count <= 0) return false;
            
            var ui = uiBases.First();
            ui.Destroy();
            uiBases.RemoveFirst();
            return _lookup.Remove(ui.GetInstanceID());
        }

        public UIBase FindByName(string uiName)
        {
            return _lookup.Values.FirstOrDefault((ui => ui.gameObject.name == uiName));
        }
        
        public UIBase FindByType<T>(T uiType)
        {
            return _lookup.Values.FirstOrDefault((ui) => ui is T);
        }

        public UIBase Remove(int instanceID)
        {
            if (!_lookup.TryGetValue(instanceID, out var ui)) return null;
            
            uiBases.Remove(ui);
            return ui;
        }

        public UIBase Hide(int instanceID)
        {
            if (!_lookup.TryGetValue(instanceID, out var ui)) return null;
            
            ui.Hide();
            uiBases.Remove(ui);
            return ui;          
        }
        
    }
}