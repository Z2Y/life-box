
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

[CommandResolverHandler("DelaySeconds")]
public class DelayCommand : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        await YieldCoroutine.WaitForSeconds(Convert.ToSingle(args[0]));
        return null;
    }
}