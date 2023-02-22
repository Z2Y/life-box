using System;
using Model;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UICurrencyText : UIBase {
    private Text currencyText;
    private Item currency;
    private void Awake() {
        currencyText = GetComponent<Text>();
    }

    public void SetCurrency(Item item) {
        currency = item;
        LifeEngine.Instance?.lifeData?.moneyInventory?.OnInventoryChange.AddListener(UpdateContent);
        UpdateContent();
    }

    private void OnDisable() {
        LifeEngine.Instance?.lifeData?.moneyInventory?.OnInventoryChange.RemoveListener(UpdateContent);
    }

    public void UpdateContent()
    {
        if (currency == null) return;
        var currencyStack = LifeEngine.Instance?.lifeData?.moneyInventory.GetStack(currency.ID);
        var currencyCount = currencyStack?.Count ?? 0;
        currencyText.text = $"{currency.Name} {currencyCount}";
    }
}