using System.Threading.Tasks;
using Logic.Detector.Config;
using ModelContainer;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    [PrefabResource("Prefabs/ui/InteractTip")]
    public class InteractTip : UIBase
    {
        public Image icon;
        public Text keyCodeText;
        public Text tip;
        public long menuID;
        public KeyCode keyCode;

        public void UpdateMenu(long id)
        {
            var config = InteractMenuConfigCollection.Instance.GetConfig(id);
            tip.text = config.Name;
            keyCode = (KeyCode)config.keyCode;
            keyCodeText.text = keyCode.ToString();
            menuID = config.ID;
            icon.sprite = Resources.Load<Sprite>(config.IconPath);
        }
        
        
        public static async Task<InteractTip> Show(long menuID) {
            var panel = await UIManager.Instance.FindOrCreateAsync<InteractTip>() as InteractTip;
        
            if (!ReferenceEquals(panel, null))
            {
                panel.UpdateMenu(menuID);
            }

            return panel;
        }
    }
}