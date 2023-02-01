using System;
using System.Linq;
using Model;
using ModelContainer;
using System.Collections.Generic;
using System.Threading.Tasks;

[CommandResolverHandler("DoEvent")]
public class DoEventResolver : CommandResolver, IEventTrigger
{
    private Event currentEvent;
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        currentEvent = EventCollection.Instance.GetEvent(Convert.ToInt64(args[0]));
        await this.Trigger();
        currentEvent = null;
        return currentEvent;
    }

    public Event GetEvent()
    {
        return currentEvent;
    }
}

[CommandResolverHandler("LastEffectResult")]
public class LastEffectResult : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        LifeNode current = LifeEngine.Instance?.lifeData?.current;
        if (current == null) return null;
        await this.Done();
        return current.Events[current.ProcessedCount - 1].EffectResult;
    }
}

[CommandResolverHandler("Confirm")]
public class ComfirmResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        return await Confirm(arg);
    }

    private Task<int> Confirm(string description)
    {
        var tcs = new TaskCompletionSource<int>();
        var onConfirm = new Action(() => tcs.SetResult(0));
        var onCancel = new Action(() => tcs.SetResult(1));

        ModalPanel.Show(description, onConfirm, onCancel);
        return tcs.Task;
    }
}

[CommandResolverHandler("Confirmed")]
public class ConfirmedResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        if (args.Count < 2) return args.LastOrDefault();
        env.TryGetValue("$Effect", out var confirmed);
        if (confirmed == null) return args.LastOrDefault();
        await this.Done();
        return Convert.ToInt32(confirmed) == 0 ? args[0] : args[1];
    }    
}

[CommandResolverHandler("RandomEvent")]
public class RandomEventResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var events = args.Where((o, idx) => (idx % 2 == 0)).Select(Convert.ToInt64).ToArray();
        var weights = args.Where((o, idx) => (idx % 2 == 1)).Select(Convert.ToSingle).ToArray();
        await this.Done();
        return EventCollection.RandomEventIndex(events, weights);
    }
}

[CommandResolverHandler("SelectEvent")]
public class SelectEventResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var description = args[0] as string;
        var events = args.Skip(1).Select(Convert.ToInt64).ToArray();
        env["$Effect"] = "";
        var options = EventCollection.GetValidEvents(events).Select((evt) => evt.Description.InjectedExpression(env)).ToList();
        return await Select(description, options);
    }

    private Task<int> Select(string description, List<string> options)
    {
        var tcs = new TaskCompletionSource<int>();
        var onSelect = new Action<int>((idx) => tcs.SetResult(idx));

        SelectPanel.Show(description, options, onSelect);
        return tcs.Task;
    }
}