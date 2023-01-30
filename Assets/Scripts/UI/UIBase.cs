using System;
using UnityEngine;

public abstract class UIBase : MonoBehaviour {
    
    public void Show() {
        this.gameObject.SetActive(true);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }

    public void Destroy() {
        GameObject.Destroy(this.gameObject);
    }
}