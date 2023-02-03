using System;
using UnityEngine;
using UnityEngine.UI;

public class ModalPanel : UIBase {
    private Text description;
    private Button okButton;
    private Button cancelButton;

    private void Awake() {
        description = transform.Find("Panel/Description").GetComponent<Text>();
        okButton = transform.Find("Panel/Buttons/OkButton").GetComponent<Button>();
        cancelButton = transform.Find("Panel/Buttons/CancelButton").GetComponent<Button>();
        okButton.onClick.AddListener(this.Destroy);
        cancelButton.onClick.AddListener(this.Destroy);
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

    public static void Show(string description, Action onOk, Action onCancel) {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/ModalPanel");
        ModalPanel panel = GameObject.Instantiate(prefab).GetComponent<ModalPanel>();
        panel.SetDescription(description);
        panel.SetCallback(onOk, onCancel);
    }
}