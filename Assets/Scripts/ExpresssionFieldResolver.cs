using System;
using System.Reflection;
using System.Collections.Generic;
using Cathei.LinqGen;
using UnityEngine;

public static class ExpressionFieldResolver {
    private static bool resolverInitialized = false;
    private static readonly Dictionary<string, Func<object>> resolvers = new ();
    public static void Register(string name, Func<object> resolver) {
        if (resolvers.ContainsKey(name)) {
            resolvers[name] = resolver;
        } else {
            resolvers.Add(name, resolver);
        }
    }

    public static object Resolve(string name)
    {
        if (!resolvers.ContainsKey(name)) {
            // Debug.LogWarning($"Resolver Not Found {name} {resolvers.Count}");
            return null;
        }

        return resolvers[name].Invoke();
    }

    static void RegisterPropertyField() {
        var subProperties = Enum.GetValues(typeof(SubPropertyType));
        foreach (SubPropertyType propertyType in subProperties) {
            Register(propertyType.ToString(), () => LifeEngine.Instance.lifeData == null ? 0 : LifeEngine.Instance.lifeData.property.GetProperty(propertyType).value);
        }
    }

    static void RegisterFieldResolver() {
        var asm = typeof(FieldResolverHandler).Assembly;
        var types = asm.GetExportedTypes().Gen().Where((Type type) => type.IsDefined(typeof(FieldResolverHandler), false));
        foreach(var type in types) {
            var fieldName = ((FieldResolverHandler)type.GetCustomAttribute(typeof(FieldResolverHandler), false))?.FieldName;
            if (Activator.CreateInstance(type) is IFieldResolver resolver) {
                Register(fieldName, resolver.Resolve);
            }
        }        
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (resolverInitialized) return;
        
        RegisterPropertyField();
        RegisterFieldResolver();
        resolverInitialized = true;
    }

}