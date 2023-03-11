using System;
using UI;
using UnityEngine;

public abstract class UIBase : MonoBehaviour {

    public void Show() {
        UIManager.Instance.PushUI(this);
    }

    public void Hide() {
        if (!UIManager.Instance.Hide(GetInstanceID()))
        {
            gameObject.SetActive(false);
        }
    }

    public void Destroy()
    {
        UIManager.Instance.Remove(GetInstanceID());
        GameObject.Destroy(gameObject);
    }
}