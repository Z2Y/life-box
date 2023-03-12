using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;

[PrefabResource("Prefabs/ui/KnapsackPanel")]
public class KnapsackPanel : UIBase
{
    private UIItemGridView itemGridView;
    private UICurrencyText currencyView;
    private ItemInventory knapsack;
    private List<ItemCellData> stackDatas;
    private UIItemTypeFilter[] filters;
    private UIItemTypeFilter currentFilter;
    private UIItemUsagePopup usagePopup;
    public bool ShowEmptyStack = true;
    public void Awake()
    {
        itemGridView = transform.Find("Panel/BagItemView").GetComponent<UIItemGridView>();
        currencyView = transform.Find("Panel/Currency").GetComponent<UICurrencyText>();
        knapsack = LifeEngine.Instance.lifeData?.knapsackInventory;
        closeBtn = transform.Find("Panel/CancelButton")?.GetComponent<Button>();
        filters = transform.Find("Panel/TypeTabs").GetComponentsInChildren<UIItemTypeFilter>();
        usagePopup = transform.Find("Panel/Usage").GetComponent<UIItemUsagePopup>();

        knapsack?.OnInventoryChange.AddListener(UpdateByCurrentFilter);
        itemGridView.OnPointerClickCell(OnPointerClickCell);
        itemGridView.OnPointerEnterCell(OnPointerEnterCell);
        itemGridView.OnPointerExitCell(OnPointerExitCell);
        
        OnClose(Hide);
    }

    private void Start() {
        usagePopup?.Hide();
    }

    private void BindInventory(ItemInventory inventory)
    {
        if (knapsack != null)
        {
            knapsack.OnInventoryChange?.RemoveListener(UpdateByCurrentFilter);
        }
        knapsack = inventory;
        currencyView?.SetCurrency(LifeEngine.Instance?.lifeData?.moneyInventory?.DefaultMoneyItem);
        knapsack?.OnInventoryChange?.AddListener(UpdateByCurrentFilter);
        InitStackData();
    }

    private void InitStackData()
    {
        if (knapsack == null)
        {
            stackDatas.Clear();
        }
        else
        {
            stackDatas = Enumerable.Range(0, knapsack.Capacity).Select((idx) =>
            {
                var data = new ItemCellData(idx);
                if (idx < knapsack.Stacks.Count)
                {
                    data.ItemStack = knapsack.Stacks[idx];
                }
                else if (ShowEmptyStack)
                {
                    data.ItemStack = new ItemStack();
                }
                return data;
            }).ToList();
        }

        itemGridView?.UpdateContents(stackDatas);
    }

    public void UpdateByCurrentFilter()
    {
        if (currentFilter == null) {
            UpdateItemGridView();
        } else {
            FilterItemwByType(currentFilter.ItemType);
        }
    }

    public void UpdateItemGridView()
    {
        if (knapsack == null)
        {
            return;
        }
        for (int i = 0; i < knapsack.Stacks.Count; i++)
        {
            stackDatas[i].ItemStack = knapsack.Stacks[i];
            stackDatas[i].Index = i;
        }
        itemGridView?.UpdateContents(stackDatas);
    }

    public void FilterItemwByType(ItemType type)
    {
        if (knapsack == null) return;
        List<int> stackIdx = knapsack.Stacks.Select((_, idx) => idx).Where((idx) => !knapsack.Stacks[idx].Empty && knapsack.Stacks[idx].item.ItemType == type).ToList();
        for (int i = 0; i < stackIdx.Count; i++)
        {
            stackDatas[i].ItemStack = knapsack.Stacks[stackIdx[i]];
            stackDatas[i].Index = stackIdx[i];
        }
        for (int i = stackIdx.Count; i < stackDatas.Count; i++)
        {
            stackDatas[i].ItemStack = new ItemStack();
        }
        itemGridView?.UpdateContents(stackDatas);
        currentFilter = filters.FirstOrDefault((filter) => filter.ItemType == type);
    }

    private void OnPointerExitCell(int index)
    {
        // 
    }

    private void OnPointerEnterCell(int index)
    {
        if (index >= stackDatas.Count)
        {
            return;
        }
        ItemStack stack = stackDatas[index].ItemStack;
        if (!stack.Empty)
        {
            // UIItemTooltip tooltip = UIHelper.Show<UIItemTooltip>(UIType.UIItemTooltip);
            // tooltip?.ShowItemTipAt(stack.TooltipContent(), Input.mousePosition);
        }
    }

    private void OnPointerClickCell(int index)
    {
        if (index >= stackDatas.Count)
        {
            return;
        }
        ItemStack stack = stackDatas[index].ItemStack;
        if (!stack.Empty)
        {
            UsableItemStack usableItemStack = stack.UsableStack();
            if (usableItemStack != null) {
                knapsack.ReplaceStack(stackDatas[index].Index, usableItemStack);
                usagePopup?.ShowItem(usableItemStack);
            } else {
                usagePopup?.ShowItem(stack);
            }
        }
    }

    public static async Task Show(ItemInventory inventory)
    {
        var panel = await UIManager.Instance.FindOrCreateAsync<KnapsackPanel>() as KnapsackPanel;

        if (panel != null)
        {
            panel.BindInventory(inventory);
        }
    }
}