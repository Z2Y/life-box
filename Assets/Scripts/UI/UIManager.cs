using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {

        private readonly Dictionary<int, UIBase> _lookup = new();
        public static UIManager Instance { get; private set; }

        public Transform worldRoot;
        public Transform screenRoot;

        private void Awake()
        {
            Instance ??= this;

            worldRoot = transform.Find("World");
            screenRoot = transform.Find("Screen");
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
        
        public UIBase FindByType<T>()
        {
            return _lookup.Values.FirstOrDefault((ui) => ui is T);
        }

        public UIBase Remove(int instanceID)
        {
            if (!_lookup.TryGetValue(instanceID, out var ui)) return null;
            _lookup.Remove(instanceID);
            
            return ui;
        }

        public UIBase Hide(int instanceID)
        {
            if (!_lookup.TryGetValue(instanceID, out var ui)) return null;
            
            ui.gameObject.SetActive(false);
            return ui;          
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
        
        public async Task<UIBase> FindOrCreateAsync<T>(bool useActive = false) where T : UIBase
        {
            var exist = _lookup.Values.OfType<T>().FirstOrDefault(ui => (useActive || !ui.gameObject.activeSelf));
            if (exist != null)
            {
                return exist;
            }
            return await UIFactory<T>.CreateAsync();
        }
        
    }
}