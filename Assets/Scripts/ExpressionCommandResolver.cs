using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class ExpressionCommandResolver {
    private static bool resolverInitialized = false;

    public static Dictionary<string, ICommandResolver> resolvers = new Dictionary<string, ICommandResolver>();
    public static void Regist(string name, ICommandResolver resolver) {
        if (resolvers.ContainsKey(name)) {
            resolvers[name] = resolver;
        } else {
            resolvers.Add(name, resolver);
        }
    }

    public static ICommandResolver GetResolver(string name) {
        if (!resolvers.ContainsKey(name)) {
            UnityEngine.Debug.LogWarning($"Resolver Not Found {name} {resolvers.Count}");
            return null;
        } else {
            return resolvers[name];
        }
    }

    static void RegisterCommandResolver() {
        Assembly asm = typeof(CommandResolverHandler).Assembly;
        Type[] types = asm.GetExportedTypes().Where((Type type) => type.IsDefined(typeof(CommandResolverHandler), false)).ToArray();
        foreach(Type type in types) {
            string fieldName = ((CommandResolverHandler)type.GetCustomAttribute(typeof(CommandResolverHandler), false))?.FieldName;
            ICommandResolver resolver = Activator.CreateInstance(type) as ICommandResolver;
            if (resolver != null) {
                Regist(fieldName, resolver);
            }
        }        
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (!resolverInitialized)
        {
            RegisterCommandResolver();
            resolverInitialized = true;
        }
    }

}