using System;
using Model;
using ModelContainer;
using System.Collections.Generic;
using Cathei.LinqGen;
using Controller;
using Cysharp.Threading.Tasks;
using Utils.Collision;


[CommandResolverHandler("NearbyNPC")]
public class NearbyNPCCommand : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        if (!LifeEngine.Instance.isReady) return 0;
        await this.Done();

        var character = LifeEngine.Instance.MainCharacter;

        var nearByCharacters = character.gameObject.GetNearbyObjects();
        
        return nearByCharacters.Gen().
            Select((obj) => obj.GetComponent<NPCController>()).
            Where((obj) => !ReferenceEquals(obj, null)).
            Where((npc) => npc.character.IsState(args[0] as string)).Count();
    }
}

[CommandResolverHandler("CurrentPlace")]
public class CurrentPlaceResolver : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        await this.Done();
        return LifeEngine.Instance.lifeData?.current?.Place;
    }
}

[CommandResolverHandler("TopLevelPlace")]
public class CurrentCityResolver : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var current = LifeEngine.Instance.lifeData?.current?.Place;
        while (current is { Parent: > 0 })
        {
            current = PlaceCollection.GetPlace(current.Parent);
        }

        await this.Done();
        return current;
    }
}

[CommandResolverHandler("SelectTalkToNearby")]
public class SelectTalkToNearBy : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        if (!LifeEngine.Instance.isReady) return null;
        var talkCompleteSource = new UniTaskCompletionSource<long>();
        
        var character = LifeEngine.Instance.MainCharacter;

        var nearbyCharacters = character.gameObject.GetNearbyObjects().Gen()
            .Select((obj) => obj.GetComponent<NPCController>())
            .Where((obj) => !ReferenceEquals(obj, null))
            .Where((npc) => npc.character.IsTalkable()).ToArray();
        var names = nearbyCharacters.Gen().Select((npc) => npc.character.Name).ToArray();
        var selector = await SelectPanel.Show("选择想要交谈的人物", names, (idx) =>
        {
            OnTalk(nearbyCharacters[idx].character);
            talkCompleteSource.TrySetResult(nearbyCharacters[idx].character.ID);
        });
        selector.SetCancelable(true, () => talkCompleteSource.TrySetCanceled());
        return await talkCompleteSource.Task;
    }

    private void OnTalk(Character character)
    {
        var trigger = TalkTriggerContainer.GetTalkConfig(character.ID);
        if (trigger == null) return;
        trigger.GetEvent().Trigger().Coroutine();
        UnityEngine.Debug.Log($"Talk to {character.ID}");
    }
}

[CommandResolverHandler("SelectShopToNearby")]
public class SelectShopToNearby : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {

        if (!LifeEngine.Instance.isReady) return null;
        var shopCompleteSource = new UniTaskCompletionSource<ShopResult>();
        
        var character = LifeEngine.Instance.MainCharacter;

        var nearbyCharacters = character.gameObject.GetNearbyObjects().Gen()
            .Select((obj) => obj.GetComponent<NPCController>())
            .Where((obj) => !ReferenceEquals(obj, null))
            .Where((npc) => npc.character.IsShopable()).ToArray();

        void OnCancel() => shopCompleteSource.TrySetCanceled();
        void OnComplete(ShopResult result) => shopCompleteSource.TrySetResult(result);

        if (nearbyCharacters.Length == 0)
        {
            shopCompleteSource.TrySetCanceled();
        }
        else if (nearbyCharacters.Length == 1)
        {
            OnShop(nearbyCharacters[0].character, OnComplete, OnCancel);
        }
        else
        {
            var names = nearbyCharacters.Gen().Select((npc) => npc.character.Name).ToArray();
            var selector = await SelectPanel.Show("选择想要交谈的人物", names,
                (idx) => OnShop(nearbyCharacters[idx].character, OnComplete, OnCancel));
            selector.SetCancelable(true, OnCancel);
        }

        return await shopCompleteSource.Task;
    }

    private async void OnShop(Character character, Action<ShopResult> onComplete, Action OnCancel)
    {
        var configs = ShopConfigCollection.GetShopConfigsByCharacter(character.ID).Gen().Where((config) => config.isOpen).ToArray();

        if (configs.Length <= 0)
        {
            // todo  
        }
        else if (configs.Length == 1)
        {
            OpenShop(configs[0], onComplete);
        }
        else
        {
            var selector = await SelectPanel.Show("选择想要购买的物品", configs.Gen().Select((config) => config.Name).ToArray(),
                (idx) => OpenShop(configs[idx], onComplete));
            selector.SetCancelable(true, OnCancel);
        }
    }

    private void OpenShop(ShopConfig config, Action<ShopResult> onComplete)
    {
        async void onShop(ShopResult result)
        {
            var node = await ShopTrigger.Instance.WithConfig(config).Trigger();
            if (node != null && node.Event.EventType == (int)EventType.Shop)
            {
                result.Merge(node.EffectResult as ShopResult);
                node.SetEffectResult(result);
            }

            onComplete(result);
        }

        ShopPanel.Show(config, onShop).Coroutine();
    }
}