

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

[CommandResolverHandler("ObtainLifeAsset")]
public class ObtainLifeAsset : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        await this.Done();
        throw new NotImplementedException();
    }
}

[CommandResolverHandler("LostLifeAsset")]
public class LostLifeAsset : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        await this.Done();
        throw new NotImplementedException();
    }
}