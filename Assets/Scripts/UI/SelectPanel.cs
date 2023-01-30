using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPanel : UIBase {
    private Text description;
    private ScrollRect scroll;
    private Button okButton;
    private Button cancelButton;
    private GameObject optionPrefab;

    void Awake() {
        description = transform.Find("Panel/Description").GetComponent<Text>();
        scroll = transform.Find("Panel/Scroll View").GetComponent<ScrollRect>();
        cancelButton = transform.Find("Panel/CancelButton").GetComponent<Button>();
        optionPrefab = scroll.transform.Find("Viewport/Content/OptionButton").gameObject;
        optionPrefab.SetActive(false);
        cancelButton.onClick.AddListener(this.Destroy);
        SetCancelable(false);
    }

    public void SetDescription(string text) {
        description.text = text;
    }

    public void SetOptions(List<string> options, Action<int> onSelect) {
        for (int i = 0; i < options.Count; i++)
        {
            string option = options[i];
            int index = i;
            Button optionBtn = GameObject.Instantiate(optionPrefab, scroll.content).GetComponent<Button>();
            optionBtn.GetComponentInChildren<Text>().text = option;
            optionBtn.onClick.AddListener(() => onSelect(index));
            optionBtn.onClick.AddListener(this.Destroy);
            optionBtn.gameObject.SetActive(true);
        }
    }

    public void SetCancelable(bool value, Action onCancel = null) {
        cancelButton.gameObject.SetActive(value);
        if (onCancel != null) {
            cancelButton.onClick.AddListener(() => onCancel());
        }
    }

    public static SelectPanel Show(string description, List<string> options, Action<int> onSelect) {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/SelectPanel");
        SelectPanel panel = GameObject.Instantiate(prefab).GetComponent<SelectPanel>();
        panel.SetDescription(description);
        panel.SetOptions(options, onSelect);
        return panel;
    }
}