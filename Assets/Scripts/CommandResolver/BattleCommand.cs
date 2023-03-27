using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Controller;
using Cysharp.Threading.Tasks;
using Logic.Enemy.Scriptable;
using UnityEngine;
using Utils;

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

        var place = map.ActivePlaces.FirstOrDefault((place) => place.bounds.Contains(player.transform.position));

        if (ReferenceEquals(place, null))
        {
            return null;
        }
        
        place.GetOrAddComponent<UnAttachedObjectController>().AttachObject((ScriptableObject)enemySpawner);

        var cancelTokenSource = new CancellationTokenSource();

        enemySpawner.Spawn(Convert.ToString(args[1]),Convert.ToInt32(args[2]) , player.transform.position, Convert.ToSingle(args[3]), Convert.ToSingle(args[4]));

        enemySpawner.OnTerminate(() => cancelTokenSource.Cancel());

        var cancelled = await UniTask.WaitUntil(() => enemySpawner.CurrentAlive() <= 0, PlayerLoopTiming.Update, cancelTokenSource.Token).SuppressCancellationThrow();
        
        Debug.Log($"Battle End , IsCancelled: {cancelled}");

        return cancelled ? 0 : 1;
    }
}