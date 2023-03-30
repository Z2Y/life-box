using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Logic.Clan;

public class NPCRelationValue : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        await this.Done();

        return NPCRelationManager.Instance.GetNpcRelation(Convert.ToInt64(args[0]), Convert.ToInt64(args[1]))?.Relation;
        
    }
}

public class NPCRelationTitle : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        await this.Done();
        return NPCRelationManager.Instance.GetNpcRelation(Convert.ToInt64(args[0]), Convert.ToInt64(args[1]))?.RelationTitleID;
    }
}

public class UpdateClanNpcRelation : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        NPCRelationManager.Instance.UpdateNPCRelation(Convert.ToInt64(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]) );
        return await this.Done();
    }
}

public class UpdateClanNpcTitle : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        NPCRelationManager.Instance.UpdateNPCRelationTitle(Convert.ToInt64(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]) );
        return await this.Done();
    }
}
