using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifePropertyPanel : UIBase {

    private Dictionary<SubPropertyType, LifePropertyItem> items = new Dictionary<SubPropertyType, LifePropertyItem>();

    private GameObject itemPrefab;

    private void Start() {
        itemPrefab = Resources.Load<GameObject>("Prefabs/PropertyItem");
        
        LifeEngine.Instance.OnLifeStart += ListenPropertyChange;
    }

    private void OnDisable() {
        if (!LifeEngine.Instance) return;
        LifeEngine.Instance.OnLifeStart -= ListenPropertyChange;
    }

    public void ListenPropertyChange() {
        lifeProperty?.onPropertyChange.AddListener(UpdateItems);
        UpdateItems();
    }

    public void UpdateItems() {
        foreach (var property in lifeProperty.propertys)
        {
            if (items.ContainsKey(property.Key)) {
                items[property.Key].UpdateContent(property.Key.GetPropertyName(), property.Value.ToString());
            } else {
                LifePropertyItem newItem = GameObject.Instantiate(itemPrefab, transform).GetComponent<LifePropertyItem>();
                items.Add(property.Key, newItem);
                newItem.UpdateContent(property.Key.GetPropertyName(), property.Value.ToString());
            }
        }
    }

    private LifeProperty lifeProperty {
        get {
            return LifeEngine.Instance.lifeData?.property;
        }
    }
}