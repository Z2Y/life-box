using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[AttributeUsage(AttributeTargets.Class)]
public class CommandResolverHandler : Attribute {
    public string FieldName {get; set;}
    public CommandResolverHandler(string fieldName) {
        FieldName = fieldName;
    }
}

public interface ICommandResolver {
    UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env);
}

public interface IInputCommandResolver
{
    void Resolve(KeyCode code);
}

public abstract class CommandResolver : ICommandResolver {

    public GameCommandEvent onBeforeCommand;
    public GameCommandEvent onAfterCommand;

    public async UniTask<object> Run(string arg, List<object> args, Dictionary<string, object> env) {
        onBeforeCommand?.Invoke();
        object result = await Resolve(arg, args, env);
        onAfterCommand?.Invoke();
        return result;
    }
    public abstract UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env);
}

public static class CommandHelper {
    private static readonly UniTaskCompletionSource<object> doneSource;

    static CommandHelper() {
        doneSource = new UniTaskCompletionSource<object>();
        doneSource.TrySetResult(null);
    }
    public static UniTask<object> Done(this CommandResolver resolver) {
        return doneSource.Task;
    }
    
    public static void Coroutine(this UniTask task) {
        
    }

    public static void Coroutine<T>(this UniTask<T> task)
    {
        
    }

}

public class GameCommandEvent : UnityEvent {}