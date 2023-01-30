using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class UIItemDiscardPopup : UIBase
{
    private Button submitBtn;
    private Button cancelBtn;
    private Button minusBtn;
    private Button plusBtn;
    private Button maskBtn;
    private InputField inputField;
    private Text itemName;
    private Slider slider;
    private ItemStack itemData;
    private int discardCount;
    private Action onComplete;

    private void Awake()
    {
        itemName = transform.Find("Panel/ItemName").GetComponent<Text>();

        slider = transform.Find("Panel/Slider").GetComponent<Slider>();
        maskBtn = transform.Find("MaskBtn").GetComponent<Button>();
        submitBtn = transform.Find("Panel/SubmitBtn").GetComponent<Button>();
        cancelBtn = transform.Find("Panel/CancelBtn").GetComponent<Button>();
        plusBtn = transform.Find("Panel/Input/PlusBtn").GetComponent<Button>();
        minusBtn = transform.Find("Panel/Input/MinusBtn").GetComponent<Button>();
        inputField = transform.Find("Panel/Input/InputField").GetComponent<InputField>();

        maskBtn.onClick.AddListener(cancel);
        cancelBtn.onClick.AddListener(cancel);
        submitBtn.onClick.AddListener(discard);

        slider.onValueChanged.AddListener(onSliderValueChange);
        inputField.onValueChanged.AddListener(onInputValueChange);

        minusBtn.onClick.AddListener(onMinusDiscardCount);
        plusBtn.onClick.AddListener(onPlusDiscardCount);
    }

    private void cancel()
    {
        itemData = null;
        Hide();
        onComplete?.Invoke();
    }

    private void discard()
    {
        itemData?.DiscardItem(discardCount);
        Hide();
        onComplete?.Invoke();
    }

    private void onPlusDiscardCount()
    {
        if (discardCount + 1 <= slider.maxValue)
        {
            slider.value = discardCount + 1;
        }
    }

    private void onMinusDiscardCount()
    {
        if (discardCount - 1 > 0)
        {
            slider.value = discardCount - 1;
        }
    }

    private void onSliderValueChange(float raw)
    {
        int value = Mathf.FloorToInt(raw + 0.5f);
        if (discardCount != value)
        {
            discardCount = value;
            inputField.text = value.ToString();
        }
    }

    private void onInputValueChange(string raw)
    {
        int value = int.Parse(raw);
        if (discardCount != value)
        {
            discardCount = value;
            slider.value = value;
        }
    }

    public void ShowItem(ItemStack data, Action callback)
    {
        itemData = data;

        itemName.text = itemData.item.Name;
        slider.minValue = 1;
        slider.maxValue = data.Count;
        slider.value = 1;
        onComplete = callback;
        Show();
    }
}