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

    public Task<int> Confirm(string description)
    {
        TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
        Action onConfirm = () =>
        {
            tcs.SetResult(0);
        };
        Action onCancel = () =>
        {
            tcs.SetResult(1);
        };
        ModalPanel.Show(description, onConfirm, onCancel);
        return tcs.Task;
    }
}

[CommandResolverHandler("Confirmed")]
public class ComfirmedResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        if (args.Count < 2) return args.LastOrDefault();
        object comfirmed;
        env.TryGetValue("$Effect", out comfirmed);
        if (comfirmed == null) return args.LastOrDefault();
        await this.Done();
        return Convert.ToInt32(comfirmed) == 0 ? args[0] : args[1];
    }    
}

[CommandResolverHandler("RandomEvent")]
public class RandomEventResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        long[] events = args.Where((o, idx) => (idx % 2 == 0)).Select((o) => Convert.ToInt64(o)).ToArray();
        float[] weights = args.Where((o, idx) => (idx % 2 == 1)).Select((o) => Convert.ToSingle(o)).ToArray();
        await this.Done();
        return ModelContainer.EventCollection.RandomEventIndex(events, weights);
    }
}

[CommandResolverHandler("SelectEvent")]
public class SelectEventResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        string description = args[0] as string;
        long[] events = args.Skip(1).Select((o) => Convert.ToInt64(o)).ToArray();
        env["$Effect"] = "";
        List<string> options = ModelContainer.EventCollection.GetValidEvents(events).Select((evt) => evt.Description.InjectedExpression(env)).ToList();
        return await Select(description, options);
    }

    public Task<int> Select(string description, List<string> options)
    {
        TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
        Action<int> onSelect = (idx) =>
        {
            tcs.SetResult(idx);
        };
        SelectPanel.Show(description, options, onSelect);
        return tcs.Task;
    }
}