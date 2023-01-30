using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class UIItemUsagePopup : UIBase
{
    private Button submitBtn;
    private Button cancelBtn;
    private Button useBtn;
    private Button discardBtn;
    private Button equipBtn;
    private Button maskBtn;
    private InputField inputField;
    private Image itemIcon;
    private Text itemName;
    private Text itemType;
    private Text description;
    private ItemStack itemData;
    private UIItemDiscardPopup discardPopup;

    private void Awake()
    {
        itemName = transform.Find("Panel/ItemName").GetComponent<Text>();
        itemType = transform.Find("Panel/ItemType").GetComponent<Text>();
        description = transform.Find("Panel/Description/Viewport/Content/Text").GetComponent<Text>();
        // itemIcon = transform.Find("ItemIcon").GetComponent<Image>();

        // cancelBtn = transform.Find("Panel/CancelBtn").GetComponent<Button>();
        maskBtn = transform.Find("MaskBtn").GetComponent<Button>();
        useBtn = transform.Find("Panel/Buttons/Use").GetComponent<Button>();
        equipBtn = transform.Find("Panel/Buttons/Equip").GetComponent<Button>();
        discardBtn = transform.Find("Panel/Buttons/Discard").GetComponent<Button>();
        discardPopup = transform.Find("DiscardPopup").GetComponent<UIItemDiscardPopup>();

        maskBtn.onClick.AddListener(cancel);
        useBtn.onClick.AddListener(use);
        equipBtn.onClick.AddListener(equip);
        discardBtn.onClick.AddListener(discard);
    }

    private void Start() {
        discardPopup?.Hide();
    }

    private void cancel()
    {
        itemData = null;
        Hide();
    }

    private void equip()
    {
        Hide();
    }

    private void use()
    {
        UsableItemStack usableStack = itemData.UsableStack();

        if (usableStack != null && usableStack.Usable())
        {
            usableStack.Use();
        }
        Hide();
    }

    private void discard()
    {
        discardPopup?.ShowItem(itemData, Hide);
    }

    public void ShowItem(ItemStack data)
    {
        itemData = data;
        UsableItemStack usableStack = itemData.UsableStack();

        if (usableStack != null) {
            description.text = usableStack.UsageDescription();
            useBtn.gameObject.SetActive(true);
        } else {
            useBtn.gameObject.SetActive(false);
            description.text = itemData.item.Description;
        }
        useBtn.interactable = usableStack != null && usableStack.Usable();
        itemName.text = itemData.item.Name;
        Show();
    }
}