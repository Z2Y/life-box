using System;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class UIBase : MonoBehaviour
{
    [SerializeField]
    protected Button closeBtn;

    public void Show() {
        UIManager.Instance.PushUI(this);
    }

    public void Hide() {
        if (!UIManager.Instance.Hide(GetInstanceID()))
        {
            gameObject.SetActive(false);
        }
    }
    
    public void OnClose(UnityAction onClose = null)
    {
        if (ReferenceEquals(closeBtn, null)) return;
        closeBtn.onClick.AddListener(Hide);
        if (onClose != null) {    
            closeBtn.onClick.AddListener(onClose);
        }
        closeBtn.onClick.AddListener(() =>
        {
            closeBtn.onClick.RemoveAllListeners();
        });

    }

    public void Destroy()
    {
        UIManager.Instance.Remove(GetInstanceID());
        GameObject.Destroy(gameObject);
    }
}