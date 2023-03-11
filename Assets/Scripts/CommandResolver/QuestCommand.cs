
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logic.Quest;
using ModelContainer;
using UnityEngine;

[CommandResolverHandler("StartQuest")]
public class StartQuestCommand : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(args.First()));
        Debug.Log(quest);
        if (quest == null) return null;

        return await QuestManager.Instance.AddQuest(quest);
    }
}

[CommandResolverHandler("TerminateQuest")]
public class TerminateQuest : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(args.First()));
        if (quest == null) return null;

        return await QuestManager.Instance.TerminateQuest(quest);
    }
}

[CommandResolverHandler("CompleteQuest")]
public class CompleteQuest : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(args.First()));
        if (quest == null) return null;

        return await QuestManager.Instance.CompleteQuest(quest);
    } 
}

[CommandResolverHandler("IsQuestActive")]
public class IsQuestActive : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(args.First()));
        if (quest == null) return false;

        await this.Done();
        return QuestManager.Instance.isActive(quest.ID);
    }
}

[CommandResolverHandler("IsQuestComplete")]
public class IsQuestComplete : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(args.First()));
        if (quest == null) return false;

        await this.Done();
        return QuestManager.Instance.isCompleted(quest.ID);
    }
}

[CommandResolverHandler("IsQuestTerminated")]
public class IsQuestTerminated : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(args.First()));
        if (quest == null) return false;

        await this.Done();
        return QuestManager.Instance.isTerminated(quest.ID);
    }
}

[CommandResolverHandler("IsQuestStarted")]
public class IsQuestStarted : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var quest = QuestCollection.Instance.GetQuest(Convert.ToInt64(args.First()));
        if (quest == null) return false;

        await this.Done();
        return QuestManager.Instance.isStarted(quest.ID);
    }
}