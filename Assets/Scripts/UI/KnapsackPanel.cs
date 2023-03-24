using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Model;
using UI;
using UnityEngine.UI;
using Utils;

[PrefabResource("Prefabs/ui/KnapsackPanel")]
public class KnapsackPanel : UIBase
{
    private UIItemGridView itemGridView;
    private UICurrencyText currencyView;
    private ItemInventory knapsack;
    private List<ItemCellData> stackData;
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
    }

    private void Start() {
        usagePopup.Hide();
    }

    private void BindInventory(ItemInventory inventory)
    {
        if (knapsack != null)
        {
            knapsack.OnInventoryChange?.RemoveListener(UpdateByCurrentFilter);
        }
        knapsack = inventory;
        currencyView.SetCurrency(LifeEngine.Instance.lifeData?.moneyInventory?.DefaultMoneyItem);
        knapsack?.OnInventoryChange?.AddListener(UpdateByCurrentFilter);
        InitStackData();
    }

    private void InitStackData()
    {
        if (knapsack == null)
        {
            stackData.Clear();
        }
        else
        {
            stackData = Enumerable.Range(0, knapsack.Capacity).Select((idx) =>
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

        itemGridView.UpdateContents(stackData);
    }

    public void UpdateByCurrentFilter()
    {
        if (currentFilter == null) {
            UpdateItemGridView();
        } else {
            FilterItemByType(currentFilter.ItemType);
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
            stackData[i].ItemStack = knapsack.Stacks[i];
            stackData[i].Index = i;
        }
        itemGridView.UpdateContents(stackData);
    }

    public void FilterItemByType(ItemType type)
    {
        if (knapsack == null) return;
        List<int> stackIdx = knapsack.Stacks.Select((_, idx) => idx).Where((idx) => !knapsack.Stacks[idx].Empty && knapsack.Stacks[idx].item.ItemType == type).ToList();
        for (int i = 0; i < stackIdx.Count; i++)
        {
            stackData[i].ItemStack = knapsack.Stacks[stackIdx[i]];
            stackData[i].Index = stackIdx[i];
        }
        for (int i = stackIdx.Count; i < stackData.Count; i++)
        {
            stackData[i].ItemStack = new ItemStack();
        }
        itemGridView?.UpdateContents(stackData);
        currentFilter = filters.FirstOrDefault((filter) => filter.ItemType == type);
    }

    private void OnPointerExitCell(int index)
    {
        // 
    }

    private void OnPointerEnterCell(int index)
    {
        if (index >= stackData.Count)
        {
            return;
        }
        ItemStack stack = stackData[index].ItemStack;
        if (!stack.Empty)
        {
            // UIItemTooltip tooltip = UIHelper.Show<UIItemTooltip>(UIType.UIItemTooltip);
            // tooltip?.ShowItemTipAt(stack.TooltipContent(), Input.mousePosition);
        }
    }

    private void OnPointerClickCell(int index)
    {
        if (index >= stackData.Count)
        {
            return;
        }
        ItemStack stack = stackData[index].ItemStack;
        if (!stack.Empty)
        {
            UsableItemStack usableItemStack = stack.UsableStack();
            if (usableItemStack != null) {
                knapsack.ReplaceStack(stackData[index].Index, usableItemStack);
                usagePopup.ShowItem(usableItemStack);
            } else {
                usagePopup.ShowItem(stack);
            }
        }
    }

    public static async UniTask<KnapsackPanel> Show(ItemInventory inventory)
    {
        var panel = await UIManager.Instance.FindOrCreateAsync<KnapsackPanel>(true) as KnapsackPanel;

        if (!ReferenceEquals(panel, null))
        {
            panel.BindInventory(inventory);
            panel.Show();
        }

        return panel;
    }
}