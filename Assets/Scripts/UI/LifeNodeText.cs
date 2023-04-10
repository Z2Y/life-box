using System;
using UnityEngine;
using UnityEngine.UI;

public class LifeNodeText : UIBase {
    private Text lifeText;
    public LifeNode node;

    private void Awake() {
        lifeText = GetComponentInChildren<Text>();
    }

    private void OnDisable() {
        node?.OnLifeNodeChange.RemoveListener(UpdateLifeText);
    }

    public void BindNode(LifeNode node) {   
        this.node = node;
        node.OnLifeNodeChange.AddListener(UpdateLifeText);
    }

    public void UpdateLifeText() {
        Debug.Log("Update Life Node");
        lifeText.text = $"{node?.Description}";

        LifeTextConsole.Instance?.UpdateScroller();
    }

}