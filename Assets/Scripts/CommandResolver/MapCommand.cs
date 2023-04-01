using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Logic.Map;
using UnityEngine;

[CommandResolverHandler("MapJump")]
public class MapJumpCommand : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        MapGate.JumpTo(Convert.ToInt64(args[0]), new Vector2(Convert.ToSingle(args[1]), Convert.ToSingle(args[2])));
        return await this.Done();
    }
}