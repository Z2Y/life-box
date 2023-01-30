using System;
using UnityEngine;
using UnityEngine.UI;

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

    public static void Show(string description, Action onOk) {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/BattleResultPanel");
        BattleResultPanel panel = GameObject.Instantiate(prefab).GetComponent<BattleResultPanel>();
        panel.SetDescription(description);
        panel.SetCallback(onOk);
    }
}