using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class ExpressionCommandResolver {
    private static bool resolverInitialized;

    private static readonly Dictionary<string, Type> Resolvers = new Dictionary<string, Type>();
    
    private static void Register(string name, Type resolver) {
        if (Resolvers.ContainsKey(name)) {
            Resolvers[name] = resolver;
        } else {
            Resolvers.Add(name, resolver);
        }
    }

    public static async Task<object> Resolve(string command, string arg, List<object> args,
        Dictionary<string, object> env)
    {
        if (!Resolvers.TryGetValue(command, out var resolverType))
        {
            Debug.LogWarning($"Resolver Not Found {command} {Resolvers.Count}");
            return null;
        }

        // todo use object pool 
        if (!(Activator.CreateInstance(resolverType) is ICommandResolver resolver))
        {
            Debug.LogWarning($"Resolver {command} is Not Instance of ICommandResolver");
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
                Register(fieldName, type);
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