using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class ExpressionFieldResolver {
    private static bool resolverInitialized = false;
    public static Dictionary<string, Func<object>> resolvers = new Dictionary<string, Func<object>>();
    public static void Regist(string name, Func<object> resolver) {
        if (resolvers.ContainsKey(name)) {
            resolvers[name] = resolver;
        } else {
            resolvers.Add(name, resolver);
        }
    }

    public static object Resolve(string name) {
        if (!resolvers.ContainsKey(name)) {
            // Debug.LogWarning($"Resolver Not Found {name} {resolvers.Count}");
            return null;
        } else {
            return resolvers[name].Invoke();
        }
    }

    static void RegisterPropertyField() {
        var subProperties = Enum.GetValues(typeof(SubPropertyType));
        foreach (SubPropertyType propertyType in subProperties) {
            Regist(propertyType.ToString(), () => {
                if (LifeEngine.Instance?.lifeData == null) { return 0; }
                return LifeEngine.Instance.lifeData.property.GetProperty(propertyType).value;
            });
        }
    }

    static void RegisterFieldResolver() {
        Assembly asm = typeof(FieldResolverHandler).Assembly;
        Type[] types = asm.GetExportedTypes().Where((Type type) => type.IsDefined(typeof(FieldResolverHandler), false)).ToArray();
        foreach(Type type in types) {
            string fieldName = ((FieldResolverHandler)type.GetCustomAttribute(typeof(FieldResolverHandler), false))?.FieldName;
            if (Activator.CreateInstance(type) is IFieldResolver resolver) {
                Regist(fieldName, resolver.Resolve);
            }
        }        
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (!resolverInitialized)
        {
            RegisterPropertyField();
            RegisterFieldResolver();
            resolverInitialized = true;
        }
    }

}