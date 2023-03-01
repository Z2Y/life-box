using System;
using UI;
using UnityEngine;

public abstract class UIBase : MonoBehaviour {

    public void Show() {
        UIManager.Instance.PushUI(this);
    }

    public void Hide() {
        UIManager.Instance.Hide(GetInstanceID());
    }

    public void Destroy()
    {
        UIManager.Instance.Remove(GetInstanceID());
        GameObject.Destroy(gameObject);
    }
}