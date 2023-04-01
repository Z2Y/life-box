using System;
using System.Linq;
using ModelContainer;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Controller;
using Cysharp.Threading.Tasks;
using Logic.Message;
using UniTaskPubSub;
using UnityEngine;
using Utils;
using Event = Model.Event;

[CommandResolverHandler("DoEvent")]
public class DoEventResolver : CommandResolver, IEventTrigger
{
    private Event currentEvent;
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        currentEvent = EventCollection.GetEvent(Convert.ToInt64(args[0]));
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
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var current = LifeEngine.Instance?.lifeData?.current;
        if (current == null) return null;
        await this.Done();
        return current.Events[current.ProcessedCount - 1].EffectResult;
    }
}

[CommandResolverHandler("Confirm")]
public class ConformResolver : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        return await Confirm(arg);
    }

    private async UniTask<int> Confirm(string description)
    {
        var tcs = new TaskCompletionSource<int>();
        var onConfirm = new Action(() => tcs.SetResult(0));
        var onCancel = new Action(() => tcs.SetResult(1));

        await ModalPanel.Show(description, onConfirm, onCancel);
        return await tcs.Task;
    }
}

[CommandResolverHandler("Confirmed")]
public class ConfirmedResolver : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
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
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
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
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var description = args[0] as string;
        var events = args.Skip(1).Select(Convert.ToInt64).ToArray();
        env["$Effect"] = "";
        var options = EventCollection.GetValidEvents(events).Select((evt) => evt.Description.InjectedExpression(env)).ToList();
        return await Select(description, options);
    }

    private UniTask<int> Select(string description, List<string> options)
    {
        var tcs = new UniTaskCompletionSource<int>();
        var onSelect = new Action<int>((idx) => tcs.TrySetResult(idx));

        SelectPanel.Show(description, options, onSelect).Coroutine();
        return tcs.Task;
    }
}

[CommandResolverHandler("ListenMapMessage")]
public class ListenMapMessage : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        // AsyncMessageBus.Default.Subscribe()
        return await this.Done();
    }
}

[CommandResolverHandler("ListenNPCMessage")]
public class ListenNPCMessage : CommandResolver
{
    private static MethodInfo sub = typeof(AsyncMessageBus).GetMethod("Subscribe");
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var messageName = Convert.ToString(args[0]);
        var characterID = Convert.ToInt64(args[1]);
        var messageType = Type.GetType(messageName);

        if (messageType == null || !messageType.IsSubclassOf(typeof(CharacterMessage)))
        {
            Debug.LogWarning($"Invalid Message to Listen {args[0]}");
            return null;
        }
        
        SimplePoolManager.ReturnUsedIf<CommandSnapshot>(
            (snap) => 
                snap.commandName == "ListenNPCMessage" && 
                Convert.ToString(snap.args[0]) == messageName &&
                Convert.ToInt64(snap.args[1]) == characterID);
        
        var cancelTokenSource = new CancellationTokenSource();
        var controller = LifeEngine.Instance.GetOrAddComponent<UnAttachedObjectController>();
        var disposable = AsyncMessageBus.Default.Subscribe<CharacterMessage>(async (message) =>
        {
            if (message.GetType() != messageType || message.characterID != characterID)
            {
                return;
            }

            try
            {
                await EventCollection.GetEvent(Convert.ToInt64(args[2])).Trigger();
            }
            finally
            {
                cancelTokenSource.Cancel();
                cancelTokenSource.Dispose();
            }
        });
        var snapshot = SimplePoolManager.Get<CommandSnapshot>().Capture("ListenNPCMessage", arg, args, env);
        snapshot.AddDisposeStuff(disposable, cancelTokenSource);
        snapshot.AddTo(cancelTokenSource.Token);
        controller.AttachObject(snapshot);
        return await this.Done();
    }
}