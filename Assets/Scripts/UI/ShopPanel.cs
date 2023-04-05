using System;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using Model;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;

[PrefabResource("Prefabs/ui/ShopPanel")]
public class ShopPanel : UIBase
{
    private Text description;
    private ScrollRect shopItemScroll;
    private ScrollRect bagItemScroll;
    private Button okButton;
    private Button cancelButton;
    private GameObject itemPrefab;
    private ShopInventory shop;
    private ItemInventory bag;
    private UIShopConfirmPopup confirmPopup;
    private UICurrencyText currencyText;
    private ShopConfig Config;
    private ShopResult Result;
    private Action<ShopResult> onShop;

    void Awake()
    {
        transform.SetParent(UIManager.Instance.worldRoot);
        description = transform.Find("Panel/Description").GetComponent<Text>();
        shopItemScroll = transform.Find("Panel/ShopItemView").GetComponent<ScrollRect>();
        bagItemScroll = transform.Find("Panel/BagItemView").GetComponent<ScrollRect>();
        cancelButton = transform.Find("Panel/CancelButton").GetComponent<Button>();
        confirmPopup = transform.Find("ConfirmPopup").GetComponent<UIShopConfirmPopup>();
        currencyText = transform.Find("Panel/Currency").GetComponent<UICurrencyText>();
        itemPrefab = Resources.Load<GameObject>("Prefabs/ShopItem");
        cancelButton.onClick.AddListener(this.Destroy);
        SetCancelable(true, onClose);
    }

    public void SetConfig(ShopConfig config, Action<ShopResult> callback)
    {
        Config = config;
        try
        {
            description.text = $"购买 {config.Name}";
            shop = ShopInventoryCollection.Instance.GetInventory(config);
            shop.RefreshShopItem(LifeEngine.Instance.lifeTime.Time);
            bag = LifeEngine.Instance.lifeData.knapsackInventory;
            currencyText?.SetCurrency(shop.Currency);
            UpdateShopView();
            UpdateBagView();
            shop.OnInventoryChange.AddListener(UpdateShopView);
            bag.OnInventoryChange.AddListener(UpdateBagView);
            Result = new ShopResult(config);
            onShop = callback;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public void UpdateShopView()
    {
        var stacks = shop.Stacks.Gen().Where(itemStack => !itemStack.Empty).ToArray();
        for (int i = 0; i < stacks.Length; i++)
        {
            UIShopItem uIShopItem;
            if (i < shopItemScroll.content.childCount)
            {
                uIShopItem = shopItemScroll.content.GetChild(i).GetComponent<UIShopItem>();
            }
            else
            {
                uIShopItem = Instantiate(itemPrefab, shopItemScroll.content).GetComponent<UIShopItem>();
            }

            uIShopItem.gameObject.SetActive(true);
            ShopItemStack itemStack = (ShopItemStack)stacks[i];
            uIShopItem.SetItem(new ShopConfirmData(itemStack, shop.Currency, itemStack.Price));
            uIShopItem.OnItemClick(onConfirmSell);
        }

        for (int i = stacks.Length; i < shopItemScroll.content.childCount; i++)
        {
            shopItemScroll.content.GetChild(i).gameObject.SetActive(false);
        }

        shopItemScroll.transform.Find("EmptyText")?.gameObject.SetActive(shopItemScroll.content.childCount == 0);
    }

    private void UpdateBagView()
    {
        var stacks = bag.Stacks
            .Gen().Where(itemStack => !itemStack.Empty && shop.GetRecycle(itemStack.item.ID) > 0).ToArray();

        for (int i = 0; i < stacks.Length; i++)
        {
            UIShopItem uIShopItem;
            if (i < bagItemScroll.content.childCount)
            {
                uIShopItem = bagItemScroll.content.GetChild(i).GetComponent<UIShopItem>();
            }
            else
            {
                uIShopItem = Instantiate(itemPrefab, bagItemScroll.content).GetComponent<UIShopItem>();
            }

            int recycle = shop.GetRecycle(stacks[i].item.ID);
            uIShopItem.SetItem(new ShopConfirmData(stacks[i], shop.Currency, recycle));
            uIShopItem.OnItemClick(onConfirmRecycle);
            uIShopItem.gameObject.SetActive(true);
        }

        for (int i = stacks.Length; i < bagItemScroll.content.childCount; i++)
        {
            bagItemScroll.content.GetChild(i).gameObject.SetActive(false);
        }

        bagItemScroll.transform.Find("EmptyText")?.gameObject.SetActive(bagItemScroll.content.childCount == 0);
    }

    private void onConfirmSell(ShopConfirmData data)
    {
        var currencyStack = LifeEngine.Instance.lifeData?.moneyInventory.GetStack(data.Currency.ID);
        int currencyCount = currencyStack?.Count ?? 0;
        if (data.Price <= 0)
        {
            confirmPopup.ShowItem("确认购买物品", data, data.ItemStack.Count, onSell);
        }
        else
        {
            int maxPurchaseCount = Math.Min((int)Mathf.Floor(currencyCount / data.Price), data.ItemStack.Count);
            confirmPopup.ShowItem("确认购买物品", data, maxPurchaseCount, onSell);
        }
    }

    private void onSell(ShopConfirmData data, int count)
    {

        int currencyCount = data.Price * count;
        var currencyStack = LifeEngine.Instance.lifeData?.moneyInventory.GetStack(data.Currency.ID);

        if (currencyStack == null || currencyStack.Count < currencyCount)
        {
            Debug.LogWarning("货币不够购买物品");
            return;
        }

        if (bag.StoreItem(data.ItemStack.item, count))
        {
            currencyStack.DiscardItem(currencyCount);
            Result.AddSell(data, count);
            data.ItemStack.DiscardItem(count);
        }
        else
        {
            Debug.LogWarning("角色没有足够空间");
        }
    }

    private void onConfirmRecycle(ShopConfirmData data)
    {
        confirmPopup.ShowItem("确认出售物品", data, data.ItemStack.Count, onRecycle);
    }

    private void onRecycle(ShopConfirmData data, int count)
    {

        int currencyCount = data.Price * count;
        var currencyInventory = LifeEngine.Instance.lifeData?.moneyInventory;

        if (shop.StoreItem(data.ItemStack.item, count))
        {
            currencyInventory.StoreItem(data.Currency, currencyCount);
            Result.AddRecycle(data, count);
            data.ItemStack.DiscardItem(count);
        }
        else
        {
            Debug.LogWarning("商店无法回收该商品");
        }
    }

    private void onClose()
    {
        shop?.OnInventoryChange.RemoveListener(UpdateShopView);
        bag?.OnInventoryChange.RemoveListener(UpdateBagView);
        if (Result is { Empty: false })
        {
            onShop?.Invoke(Result);
        }
    }

    public void SetCancelable(bool value, Action onCancel = null)
    {
        cancelButton.gameObject.SetActive(value);
        if (onCancel != null)
        {
            cancelButton.onClick.AddListener(() => onCancel());
        }
    }

    public static async UniTask<ShopPanel> Show(ShopConfig config, Action<ShopResult> onShop)
    {
        var panel = await UIManager.Instance.FindOrCreateAsync<ShopPanel>() as ShopPanel;
        panel?.SetConfig(config, onShop);
        return panel;
    }
}

public class ShopResult
{
    public readonly ShopInventory Sells;

    public readonly ShopInventory Recycles;

    public ShopResult(ShopConfig config)
    {
        Sells = new ShopInventory(config);
        Recycles = new ShopInventory(config);
    }

    public void AddRecycle(ShopConfirmData data, int count)
    {
        Recycles.StoreItem(data.ItemStack.item, count, data.Price, 0);
    }

    public void AddSell(ShopConfirmData data, int count)
    {
        Sells.StoreItem(data.ItemStack.item, count, data.Price, 0);
    }

    public bool Merge(ShopResult other)
    {
        if (other == null || other.Sells.Config != Sells.Config) return false;
        foreach (var stack in other.Sells.Stacks)
        {
            ShopItemStack shopStack = stack as ShopItemStack;
            Sells.StoreItem(shopStack.item, shopStack.Count, shopStack.Price, 0);
        }
        foreach (var stack in other.Recycles.Stacks)
        {
            ShopItemStack shopStack = stack as ShopItemStack;
            Recycles.StoreItem(shopStack.item, shopStack.Count, shopStack.Price, 0);
        }
        return true;
    }

    public int CurrencyCount
    {
        get
        {
            var payed = Sells.Stacks.Gen().Select((stack) => ((ShopItemStack)stack).Price * stack.Count).Sum();
            var recycled = Recycles.Stacks.Gen().Select((stack) => ((ShopItemStack)stack).Price * stack.Count).Sum();
            return payed - recycled;
        }
    }

    public bool Empty => Sells.Empty && Recycles.Empty;

    public override string ToString()
    {
        var sells = string.Join(" ", Sells.Stacks.Gen().Select(stack => $"【{stack.item.Name} x {stack.Count}】"));
        var recycles = string.Join(" ", Recycles.Stacks.Gen().Select(stack => $"【{stack.item.Name} x {stack.Count}】"));
        var count = CurrencyCount;
        return $"{(Sells.Empty ? "" : "购买")} {sells} {(Recycles.Empty ? "" : "卖出")} {recycles} {(count > 0 ? "失去" : "获得")} 【{Sells.Currency.Name}】 x {Math.Abs(count)}";
    }
}