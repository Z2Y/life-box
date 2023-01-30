using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeTextConsole : UIBase {
    private Text lifeText;
    private ScrollRect scroller;
    private LifeNode current;
    private GameObject nodePrefab;
    private List<LifeNodeText> lifeNodes = new List<LifeNodeText>();

    private static LifeTextConsole _instance;

    public static LifeTextConsole Instance {
        get {
            return _instance;
        }
    }

    private void Awake() {
        _instance = this;
        nodePrefab = Resources.Load<GameObject>("Prefabs/LifeNode");
        lifeText = GetComponentInChildren<Text>();
        scroller = GetComponentInChildren<ScrollRect>();
    }

    private void Start() {
        ListenLifeChange();
    }

    private void OnDisable() {
        if (!LifeEngine.Instance) return;
        LifeEngine.Instance.AfterLifeChange -= UpdateLifeText;
    }

    private void ListenLifeChange() {
        LifeEngine.Instance.AfterLifeChange += UpdateLifeText;
    }

    private void UpdateLifeText() {
        LifeNode next = LifeEngine.Instance.lifeData.current;

        if (next == null) {
            return;
        }

        LifeNodeText uiLifeNode;
        if (lifeNodes.Count == 0 || lifeNodes[lifeNodes.Count - 1].node != next) {
            uiLifeNode = Instantiate(nodePrefab, scroller.content).GetComponent<LifeNodeText>();
            uiLifeNode.BindNode(next);
            lifeNodes.Add(uiLifeNode);
            uiLifeNode.UpdateLifeText();
        }
    }

    public void UpdateScroller() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroller.content);
        scroller.verticalNormalizedPosition = 0f;
    }

}