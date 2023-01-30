using System;
using UnityEngine;
using UnityEngine.UI;

public class LifePlacePanel : UIBase {
    private Text text;

    void Awake() {
        text = transform.Find("Text").GetComponent<Text>();
    }

    void Start() {
        ListenLifeTime();
        Refresh();
    }

    void OnDisable() {
        if (LifeEngine.Instance != null) {
            LifeEngine.Instance.AfterLifeChange -= Refresh;
        }
    }

    void ListenLifeTime() {
        if (LifeEngine.Instance != null) {
            LifeEngine.Instance.AfterLifeChange += Refresh;
        }        
    }

    public void Refresh() {
        text.text = LifeEngine.Instance?.lifeData?.current.Next.Place.Name;
    }
}