using System;
using Model;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIShopItem : UIBase, IPointerClickHandler {
    public Text nameText;
    public Text descriptionText;
    private ShopConfirmData data;
    private Action<ShopConfirmData> onItemClick;

    private void Awake() {
        nameText = transform.Find("Name").GetComponent<Text>();
        descriptionText = transform.Find("Description").GetComponent<Text>();
    }

    public void SetItem(ShopConfirmData itemData) {
        data = itemData;
        nameText.text = data.ItemStack.item.Name;
        descriptionText.text = $"数量:{data.ItemStack.Count}    价格: {data.Price}{data.Currency.Name}";
    }

    public void OnItemClick(Action<ShopConfirmData> callback) {
        onItemClick = callback;
    }

    public void OnPointerClick(PointerEventData e) {
        onItemClick?.Invoke(data);
    }
}