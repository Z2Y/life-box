
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelContainer;

[CommandResolverHandler("StartQuest")]
public class StartQuestCommand : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(arg));
        if (quest == null) return null;
        // todo check if quest is in progress
        // todo add to player's quest list
        return await ExpressionCommandResolver.Resolve("DoEvent",quest.startEventID.ToString(),
            new List<object> { quest.startEventID }, env);
    }
}

[CommandResolverHandler("TerminateQuest")]
public class TerminateQuest : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(arg));
        if (quest == null) return null;
        // todo check if quest is not started
        return await this.Done();
    }
}

[CommandResolverHandler("CompleteQuest")]
public class CompleteQuest : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(arg));
        if (quest == null) return null;
        // todo check if quest is not completed
        // todo give bonus to player
        return await this.Done();
    } 
}