using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class ExpressionCommandResolver {
    private static bool resolverInitialized;

    private static readonly Dictionary<string, ICommandResolver> Resolvers = new ();
    
    private static void Register(string name, ICommandResolver resolver) {
        if (Resolvers.ContainsKey(name)) {
            Resolvers[name] = resolver;
        } else {
            Resolvers.Add(name, resolver);
        }
    }

    public static async UniTask<object> Resolve(string command, string arg, List<object> args,
        Dictionary<string, object> env)
    {
        if (!Resolvers.TryGetValue(command, out var resolver))
        {
            //Debug.LogWarning($"Resolver Not Found {command} {Resolvers.Count}");
            return null;
        }
        
        var result = await resolver.Resolve(arg, args, env);
        return result;

    }

    private static void RegisterCommandResolver() {
        var asm = typeof(CommandResolverHandler).Assembly;
        var types = asm.GetExportedTypes().Where((type) => type.IsDefined(typeof(CommandResolverHandler), false)).ToArray();
        foreach(var type in types) {
            var fieldName = ((CommandResolverHandler)type.GetCustomAttribute(typeof(CommandResolverHandler), false))?.FieldName;
            if (type.GetInterface(nameof(ICommandResolver)) != null)
            {
                Register(fieldName, Activator.CreateInstance(type) as ICommandResolver);
            }
        }        
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (resolverInitialized) return;
        
        RegisterCommandResolver();
        resolverInitialized = true;
    }

}