using Cysharp.Threading.Tasks;
using ModelContainer;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    [PrefabResource("Prefabs/ui/InteractTip")]
    public class InteractTip : UIBase
    {
        private Canvas canvas;
        public Image icon;
        public Text keyCodeText;
        public Text tip;
        public KeyCode keyCode;
        public long menuID;
        public long targetID;

        private void Awake()
        {
            transform.SetParent(UIManager.Instance.worldRoot);
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
        }

        public void UpdateMenu(long id)
        {
            var config = InteractMenuConfigCollection.Instance.GetConfig(id);
            tip.text = config.Name;
            keyCode = (KeyCode)config.keyCode;
            keyCodeText.text = keyCode.ToString();
            menuID = config.ID;
            icon.sprite = Resources.Load<Sprite>(config.IconPath);
        }

        public void SetPosition(Vector3 position, long tID)
        {
            transform.position = position;
            this.targetID = tID;
        }

        public static async UniTask<InteractTip> Show(long menuID) {
            var panel = await UIManager.Instance.FindOrCreateAsync<InteractTip>() as InteractTip;
        
            if (!ReferenceEquals(panel, null))
            {
                panel.UpdateMenu(menuID);
                panel.Show();
            }

            return panel;
        }
    }
}