using System;
using UnityEngine;
using Model;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FancyScrollView;


public class UIItemViewCell : FancyGridViewCell<ItemCellData, ItemCellContext>, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
, IPointerDownHandler, IPointerUpHandler
{

    protected Image image;
    protected Text text;
    protected Text itemName;

    public override void Initialize()
    {
        image = transform.Find("Icon/Image").GetComponent<Image>();
        text = transform.Find("Icon/Text").GetComponent<Text>();
        itemName = transform.Find("Name/Text").GetComponent<Text>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Context.OnPointerEnterCell?.Invoke(Index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Context.OnPointerExitCell?.Invoke(Index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Context.OnPointerClickCell?.Invoke(Index);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public override void UpdateContent(ItemCellData itemData)
    {
        if (itemData.ItemStack != null)
        {
            if (!itemData.ItemStack.Empty)
            {
                // image.sprite = itemData.ItemStack.item.icon.sprite;
                text.text = itemData.ItemStack.Count.ToString();
                itemName.text = itemData.ItemStack.item.Name;
                image.gameObject.SetActive(true);
            }
            else
            {
                image.gameObject.SetActive(false);
                text.text = "";
                itemName.text = "";
            }
        }
    }
}

public class ItemCellContext : FancyGridViewContext
{
    public Action<int> OnPointerEnterCell;
    public Action<int> OnPointerExitCell;
    public Action<int> OnPointerClickCell;
}

public class ItemCellData
{
    public int Index { get; set; }

    public Item Currency { get; set; }

    public int Price { get; set; }

    public ItemStack ItemStack { get; set; }

    public ItemCellData(int index) => Index = index;
}
