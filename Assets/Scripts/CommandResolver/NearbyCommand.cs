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
        Place place = await ExpressionCommandResolver.Resolve("CurrentPlace", arg, args, env) as Place;

        if (place == null) return 0;
        string state = args[0] as string;
        await this.Done();
        return place.Characters.Values.Count((character) => character.IsState(state));
    }
}

[CommandResolverHandler("CurrentPlace")]
public class CurrentPlaceResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        await this.Done();
        return LifeEngine.Instance?.lifeData?.current?.Place;
    }
}

[CommandResolverHandler("TopLevelPlace")]
public class CurrentCityResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        Place current = LifeEngine.Instance?.lifeData?.current?.Place;
        while (current != null && current.Parent > 0)
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
    private TaskCompletionSource<long> talkCompleteSource;
    private List<Character> nearbyCharacters = new();
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        Place place = await ExpressionCommandResolver.Resolve("CurrentPlace", arg, args, env) as Place;

        if (place == null) return null;
        talkCompleteSource = new TaskCompletionSource<long>();
        nearbyCharacters = place.Characters.Values.Where((character) => character.IsTalkable()).ToList();
        List<string> names = nearbyCharacters.Select((character) => character.Name).ToList();
        SelectPanel.Show("选择想要交谈的人物", names, OnTalk).SetCancelable(true, OnCancel);
        return await talkCompleteSource.Task;
    }

    private void OnTalk(int index)
    {
        Character character = nearbyCharacters[index];
        TalkTrigger trigger = TalkTriggerContainer.Instance.GetTrigger(character.ID);
        talkCompleteSource.SetResult(character.ID);
        if (trigger == null) return;
        trigger.Trigger().Coroutine();
        UnityEngine.Debug.Log($"Talk to {character.ID}");
    }

    private void OnCancel()
    {
        talkCompleteSource.TrySetCanceled();
    }
}

[CommandResolverHandler("SelectShopToNearby")]
public class SelectShopToNearby : CommandResolver
{
    private TaskCompletionSource<ShopResult> shopCompleteSource;
    private List<Character> nearbyCharacters = new();
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        Place place = await ExpressionCommandResolver.Resolve("CurrentPlace", arg, args, env) as Place;

        if (place == null) return null;
        shopCompleteSource = new TaskCompletionSource<ShopResult>();
        nearbyCharacters = place.Characters.Values.Where((character) => character.IsShopable()).ToList();
        if (nearbyCharacters.Count == 0)
        {
            shopCompleteSource.SetCanceled();
        }
        else if (nearbyCharacters.Count == 1)
        {
            OnShop(0);
        }
        else
        {
            List<string> names = nearbyCharacters.Select((character) => character.Name).ToList();
            SelectPanel.Show("选择想要交谈的人物", names, OnShop).SetCancelable(true, OnCancel);
        }
        return await shopCompleteSource.Task;
    }

    private void OnShop(int index)
    {
        Character character = nearbyCharacters[index];
        List<ShopConfig> configs = ShopConfigCollection.Instance.GetShopConfigsByCharacter(character.ID).Where((config) => config.isOpen).ToList();

        if (configs.Count <= 0)
        {
            // todo  
        }
        else if (configs.Count == 1)
        {
            OpenShop(configs[0]);
        }
        else
        {
            SelectPanel.Show("选择想要购买的物品", configs.Select((config) => config.Name).ToList(), (idx) => OpenShop(configs[idx])).SetCancelable(true, OnCancel);
        }
    }

    private void OpenShop(ShopConfig config)
    {
        async void onShop(ShopResult result)
        {
            var node = await ShopTrigger.Instance.WithConfig(config).Trigger();
            if (node != null && node.Event.EventType == EventType.Shop)
            {
                result.Merge(node.EffectResult as ShopResult);
                node.SetEffectResult(result);
            }

            shopCompleteSource.TrySetResult(result);
        }

        ShopPanel.Show(config, onShop);
    }

    private void OnCancel()
    {
        shopCompleteSource.TrySetCanceled();
    }
}