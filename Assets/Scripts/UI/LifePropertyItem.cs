using System;

using UnityEngine;
using UnityEngine.UI;

public class LifePropertyItem : UIBase {

    public SubPropertyType propertyType;
    public Text nameText;
    public Text valueText;

    private void Awake() {
        nameText = transform.Find("Name").GetComponent<Text>();
        valueText = transform.Find("Value").GetComponent<Text>();
    }

    public void UpdateContent(string title, string value) {
        nameText.text = title;
        valueText.text = value;
    }
}