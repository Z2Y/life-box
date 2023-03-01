using System;
using System.Linq;
using Model;
using ModelContainer;
using System.Collections.Generic;
using System.Threading.Tasks;


[CommandResolverHandler("NearbyNPC")]
public class NearbyNPCCommand : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var place = await ExpressionCommandResolver.Resolve("CurrentPlace", arg, args, env) as Place;

        if (place == null) return 0;
        await this.Done();
        return place.Characters.Values.Count((character) => character.IsState(args[0] as string));
    }
}

[CommandResolverHandler("CurrentPlace")]
public class CurrentPlaceResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        await this.Done();
        return LifeEngine.Instance.lifeData?.current?.Place;
    }
}

[CommandResolverHandler("TopLevelPlace")]
public class CurrentCityResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var current = LifeEngine.Instance.lifeData?.current?.Place;
        while (current is { Parent: > 0 })
        {
            current = PlaceCollection.Instance.GetPlace(current.Parent);
        }

        await this.Done();
        return current;
    }
}

[CommandResolverHandler("SelectTalkToNearby")]
public class SelectTalkToNearBy : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var place = await ExpressionCommandResolver.Resolve("CurrentPlace", arg, args, env) as Place;

        if (place == null) return null;
        var talkCompleteSource = new TaskCompletionSource<long>();
        var nearbyCharacters = place.Characters.Values.Where((character) => character.IsTalkable()).ToList();
        var names = nearbyCharacters.Select((character) => character.Name).ToList();
        var selector = await SelectPanel.Show("选择想要交谈的人物", names, (idx) =>
        {
            OnTalk(nearbyCharacters[idx]);
            talkCompleteSource.TrySetResult(nearbyCharacters[idx].ID);
        });
        selector.SetCancelable(true, () => talkCompleteSource.TrySetCanceled());
        return await talkCompleteSource.Task;
    }

    private void OnTalk(Character character)
    {
        var trigger = TalkTriggerContainer.Instance.GetTrigger(character.ID);
        if (trigger == null) return;
        trigger.Trigger().Coroutine();
        UnityEngine.Debug.Log($"Talk to {character.ID}");
    }
}

[CommandResolverHandler("SelectShopToNearby")]
public class SelectShopToNearby : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var place = await ExpressionCommandResolver.Resolve("CurrentPlace", arg, args, env) as Place;

        if (place == null) return null;
        var shopCompleteSource = new TaskCompletionSource<ShopResult>();
        var nearbyCharacters = place.Characters.Values.Where((character) => character.IsShopable()).ToList();
        void OnCancel() => shopCompleteSource.TrySetCanceled();
        void OnComplete(ShopResult result) => shopCompleteSource.TrySetResult(result);

        if (nearbyCharacters.Count == 0)
        {
            shopCompleteSource.SetCanceled();
        }
        else if (nearbyCharacters.Count == 1)
        {
            OnShop(nearbyCharacters[0], OnComplete, OnCancel);
        }
        else
        {
            var names = nearbyCharacters.Select((character) => character.Name).ToList();
            var selector = await SelectPanel.Show("选择想要交谈的人物", names,
                (idx) => OnShop(nearbyCharacters[idx], OnComplete, OnCancel));
            selector.SetCancelable(true, OnCancel);
        }

        return await shopCompleteSource.Task;
    }

    private async void OnShop(Character character, Action<ShopResult> onComplete, Action OnCancel)
    {
        var configs = ShopConfigCollection.Instance.GetShopConfigsByCharacter(character.ID)
            .Where((config) => config.isOpen).ToList();

        if (configs.Count <= 0)
        {
            // todo  
        }
        else if (configs.Count == 1)
        {
            OpenShop(configs[0], onComplete);
        }
        else
        {
            var selector = await SelectPanel.Show("选择想要购买的物品", configs.Select((config) => config.Name).ToList(),
                (idx) => OpenShop(configs[idx], onComplete));
            selector.SetCancelable(true, OnCancel);
        }
    }

    private void OpenShop(ShopConfig config, Action<ShopResult> onComplete)
    {
        async void onShop(ShopResult result)
        {
            var node = await ShopTrigger.Instance.WithConfig(config).Trigger();
            if (node != null && node.Event.EventType == EventType.Shop)
            {
                result.Merge(node.EffectResult as ShopResult);
                node.SetEffectResult(result);
            }

            onComplete(result);
        }

        ShopPanel.Show(config, onShop).Coroutine();
    }
}