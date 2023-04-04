using System;
using System.Collections.Generic;
using System.Threading;
using Controller;
using Cysharp.Threading.Tasks;
using Logic.Enemy.Scriptable;
using Logic.Message;
using Logic.Message.DefaultHandler;
using ModelContainer;
using StructLinq;
using UniTaskPubSub;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

[CommandResolverHandler("RealTimeBattle")]
public class BattleCommand : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var enemySpawner = ScriptableObject.CreateInstance(Convert.ToString(args[0])) as IEnemySpawner;

        var player = LifeEngine.Instance.MainCharacter;

        if (ReferenceEquals(enemySpawner, null))
        {
            return null;
        }
        
        var map = LifeEngine.Instance.Map;

        var placeContains = new PlaceContains() { position = player.transform.position };

        var place = map.ActivePlaces.ToStructEnumerable().FirstOrDefault(ref placeContains, x => x);

        if (ReferenceEquals(place, null))
        {
            return null;
        }
        
        place.GetOrAddComponent<UnAttachedObjectController>().AttachObject(enemySpawner as IDisposable);

        var cancelTokenSource = new CancellationTokenSource();

        enemySpawner.Spawn(Convert.ToString(args[1]),Convert.ToInt32(args[2]) , player.transform.position, Convert.ToSingle(args[3]), Convert.ToSingle(args[4]));

        enemySpawner.OnTerminate(() =>
        {
            cancelTokenSource.Cancel();
            Object.DestroyImmediate(enemySpawner as ScriptableObject, true);
        });

        var cancelled = await UniTask.WaitUntil(() => enemySpawner.CurrentAlive() <= 0, PlayerLoopTiming.Update, cancelTokenSource.Token).SuppressCancellationThrow();
        
        Debug.Log($"Battle End , IsCancelled: {cancelled}");

        return cancelled ? 0 : 1;
    }
}

[CommandResolverHandler("PlaceBattleComplete")]
public class PlaceBattleComplete : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var isReplace = Convert.ToInt64(args[0]);
        var eventID = Convert.ToInt64(args[1]);

        SimplePoolManager.ReturnUsedIf<CommandSnapshot>(
            (snap) => 
                snap.commandName == "PlaceBattleComplete" &&
                Convert.ToInt64(args[0]) == isReplace);
        
        var cancelTokenSource = new CancellationTokenSource();
        var controller = LifeEngine.Instance.Map.GetOrAddComponent<UnAttachedObjectController>();

        if (isReplace == 1)
        {
            MessageHandlerManager.DisableHandler(typeof(BattleCompleteHandler));
        }
        
        var disposable = AsyncMessageBus.Default.Subscribe<BattleComplete>(async (message) =>
        {
            Debug.Log($"Battle Completed ， Do Event {eventID}");
            try
            {
                
                await EventCollection.GetEvent(eventID).Trigger();
            }
            finally
            {
                cancelTokenSource.Cancel();
                cancelTokenSource.Dispose();
            }
        });

        var snapshot = SimplePoolManager.Get<CommandSnapshot>().Capture("PlaceBattleComplete", arg, args, env);
        snapshot.AddDisposeStuff(disposable, cancelTokenSource);
        snapshot.AddTo(cancelTokenSource.Token);
        snapshot.OnDispose(() =>
        {
            Debug.Log("ReEnable Default BattleCompleteHandler");
            MessageHandlerManager.EnableHandler(typeof(BattleCompleteHandler));
        });
        controller.AttachObject(snapshot);
        return await this.Done();
    }
}