using System;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine.UI;
using Utils;

[PrefabResource("Prefabs/ui/BattleResultPanel")]
public class BattleResultPanel : UIBase {
    
    private Text description;
    private Button okButton;

    void Awake() {
        description = transform.Find("Panel/Description").GetComponent<Text>();
        okButton = transform.Find("Panel/Buttons/OkButton").GetComponent<Button>();
        okButton.onClick.AddListener(this.Destroy);
    }

    public void SetDescription(string text) {
        description.text = text;
    }

    public void SetCallback(Action okCallback) {
        if (okCallback != null) {
            okButton.onClick.AddListener(() => okCallback());
        }
    }

    public static async UniTask<BattleResultPanel> Show(string description, Action onOk)
    {
        var panel = await UIManager.Instance.FindOrCreateAsync<BattleResultPanel>() as BattleResultPanel;
        panel?.SetDescription(description);
        panel?.SetCallback(onOk);
        return panel;
    }
}