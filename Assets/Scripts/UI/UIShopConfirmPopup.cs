using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class UIShopConfirmPopup : UIBase
{
    private Button submitBtn;
    private Button cancelBtn;
    private Button minusBtn;
    private Button plusBtn;
    private Button maskBtn;
    private Button stackBtn;
    private InputField inputField;
    private Image itemIcon;
    private Text currencyName;
    private Text currencyCount;
    private Text itemName;
    private Text itemType;
    private Text description;
    private Slider slider;
    private int purchaseCount;
    private ShopConfirmData itemData;
    private Action<ShopConfirmData, int> onPurchase;

    private void Awake()
    {
        slider = transform.Find("Panel/Slider").GetComponent<Slider>();
        itemName = transform.Find("Panel/ItemName").GetComponent<Text>();
        itemType = transform.Find("Panel/ItemType").GetComponent<Text>();
        description = transform.Find("Panel/Description").GetComponent<Text>();
        // itemIcon = transform.Find("ItemIcon").GetComponent<Image>();
        currencyName = transform.Find("Panel/Currency/Name").GetComponent<Text>();
        currencyCount = transform.Find("Panel/Currency/Count").GetComponent<Text>();

        cancelBtn = transform.Find("Panel/CancelBtn").GetComponent<Button>();
        submitBtn = transform.Find("Panel/SubmitBtn").GetComponent<Button>();
        maskBtn = transform.Find("MaskBtn").GetComponent<Button>();
        plusBtn = transform.Find("Panel/Input/PlusBtn").GetComponent<Button>();
        minusBtn = transform.Find("Panel/Input/MinusBtn").GetComponent<Button>();
        stackBtn = transform.Find("Panel/QuickStack/StackBtn").GetComponent<Button>();
        inputField = transform.Find("Panel/Input/InputField").GetComponent<InputField>();

        slider.onValueChanged.AddListener(onSliderValueChange);
        inputField.onValueChanged.AddListener(onInputValueChange);
        cancelBtn.onClick.AddListener(cancelPurchase);
        submitBtn.onClick.AddListener(submitPurchase);
        maskBtn.onClick.AddListener(cancelPurchase);
        plusBtn.onClick.AddListener(onPlusPurchaseCount);
        minusBtn.onClick.AddListener(onMinusPurchaseCount);
        stackBtn.onClick.AddListener(onStackPurchase);
    }

    private void onPlusPurchaseCount()
    {
        if (purchaseCount + 1 <= slider.maxValue)
        {
            slider.value = purchaseCount + 1;
        }
    }

    private void onMinusPurchaseCount()
    {
        if (purchaseCount - 1 > 0)
        {
            slider.value = purchaseCount - 1;
        }
    }

    private void onStackPurchase()
    {
        if (itemData != null)
        {
            slider.value = Mathf.Min(slider.maxValue, (float)itemData.ItemStack?.item.StackCount);
        }
    }

    private void onSliderValueChange(float raw)
    {
        int value = Mathf.FloorToInt(raw + 0.5f);
        if (purchaseCount != value)
        {
            purchaseCount = value;
            inputField.text = value.ToString();
            updateCurrency();
        }
    }

    private void onInputValueChange(string raw)
    {
        int.TryParse(raw, out var value);
        var count = Mathf.Clamp(value, (int)slider.minValue, (int)slider.maxValue);
        if (count != value) {
            UnityEngine.Debug.Log($"{value} {count} {slider.minValue} {slider.maxValue}");
            inputField.text = count.ToString();
        }
        if (purchaseCount != count)
        {
            purchaseCount = count;
            slider.value = count;
            updateCurrency();
        }
    }

    private void updateCurrency()
    {
        currencyCount.text = (itemData.Price * purchaseCount).ToString();
    }

    private void submitPurchase()
    {
        onPurchase?.Invoke(itemData, purchaseCount);
        Hide();
    }

    private void cancelPurchase()
    {
        itemData = null;
        Hide();
    }

    public void ShowItem(string title, ShopConfirmData data, int maxCount, Action<ShopConfirmData, int> callback)
    {
        itemData = data;
        Item item = data.ItemStack.item;
        description.text = title;
        itemName.text = item.Name;
        // itemType.text = item.ItemTypeName();
        currencyName.text = data.Currency.Name;
        currencyCount.text = data.Price.ToString();
        slider.minValue = 0;
        slider.maxValue = maxCount;
        slider.value = 1;
        onPurchase = callback;
        Show();
    }
}

public class ShopConfirmData
{
    public Item Currency { get; set; }

    public int Price { get; set; }

    public ItemStack ItemStack { get; set; }


    public ShopConfirmData(ItemStack stack, Item currency, int price)
    {
        ItemStack = stack;
        Currency = currency;
        Price = price;
    }
}