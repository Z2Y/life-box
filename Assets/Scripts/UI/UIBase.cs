using System;
using UI;
using UnityEngine;

public abstract class UIBase : MonoBehaviour {

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Destroy()
    {
        UIManager.Instance.Remove(GetInstanceID());
        GameObject.Destroy(gameObject);
    }
}