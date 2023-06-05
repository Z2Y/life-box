using System;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine.UI;
using Utils;

[PrefabResource("Prefabs/ui/ModalPanel")]
public class ModalPanel : UIBase {
    private Text description;
    private Button okButton;
    private Button cancelButton;

    private void Awake() {
        description = transform.Find("Panel/Description").GetComponent<Text>();
        okButton = transform.Find("Panel/Buttons/OkButton").GetComponent<Button>();
        cancelButton = transform.Find("Panel/Buttons/CancelButton").GetComponent<Button>();
        okButton.onClick.AddListener(Destroy);
        cancelButton.onClick.AddListener(Destroy);
    }

    public void SetDescription(string text) {
        description.text = text;
    }

    public void SetCallback(Action okCallback, Action cancelCallback) {
        if (okCallback != null) {
            okButton.onClick.AddListener(() => okCallback());
        }
        if (cancelButton != null) {
            cancelButton.onClick.AddListener(() => cancelCallback());
        }
    }

    public static async UniTask<ModalPanel> Show(string description, Action onOk, Action onCancel) {
        var panel = await UIManager.Instance.FindOrCreateAsync<ModalPanel>() as ModalPanel;
        panel.SetDescription(description);
        panel.SetCallback(onOk, onCancel);
        return panel;
    }
}