using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class ExpressionCommandResolver {
    private static bool resolverInitialized;

    private static readonly Dictionary<string, ICommandResolver> Resolvers = new Dictionary<string, ICommandResolver>();
    private static void Register(string name, ICommandResolver resolver) {
        if (Resolvers.ContainsKey(name)) {
            Resolvers[name] = resolver;
        } else {
            Resolvers.Add(name, resolver);
        }
    }

    public static ICommandResolver GetResolver(string name) {
        if (!Resolvers.ContainsKey(name)) {
            Debug.LogWarning($"Resolver Not Found {name} {Resolvers.Count}");
            return null;
        } else {
            return Resolvers[name];
        }
    }

    private static void RegisterCommandResolver() {
        Assembly asm = typeof(CommandResolverHandler).Assembly;
        Type[] types = asm.GetExportedTypes().Where((type) => type.IsDefined(typeof(CommandResolverHandler), false)).ToArray();
        foreach(Type type in types) {
            string fieldName = ((CommandResolverHandler)type.GetCustomAttribute(typeof(CommandResolverHandler), false))?.FieldName;
            if (Activator.CreateInstance(type) is ICommandResolver resolver) {
                Register(fieldName, resolver);
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