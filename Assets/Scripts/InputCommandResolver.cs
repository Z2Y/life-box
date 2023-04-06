using System;
using System.Collections.Generic;
using System.Reflection;
using Cathei.LinqGen;
using UnityEngine;

public class InputCommandResolver : MonoBehaviour
{
    public static InputCommandResolver Instance { get; private set;}
    
    private static readonly Dictionary<KeyCode, IInputCommandResolver> Resolvers = new ();

    private static readonly HashSet<KeyCode> keyDisabled = new();
    private void Awake()
    {
        Instance = this;
        RegisterCommandResolver();
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            var handlers = Resolvers.Gen().Where((pair) => Input.GetKeyDown(pair.Key) && !keyDisabled.Contains(pair.Key));
            foreach (var pair in handlers)
            {
                pair.Value.Resolve(pair.Key);
            }
        }

    }

    public void DisableKeyCode(KeyCode code)
    {
        keyDisabled.Add(code);
    }

    public void EnableKeyCode(KeyCode code)
    {
        keyDisabled.Remove(code);
    }

    public void Register(KeyCode code, IInputCommandResolver resolver) {
        if (Resolvers.ContainsKey(code)) {
            Resolvers[code] = resolver;
        } else {
            Resolvers.Add(code, resolver);
        }
    }

    public void UnRegister(KeyCode code)
    {
        Resolvers.Remove(code);
        keyDisabled.Remove(code);
    }
    
    private void RegisterCommandResolver() {
        var asm = typeof(CommandResolverHandler).Assembly;
        var types = asm.GetExportedTypes().Gen().Where((type) => type.IsDefined(typeof(InputCommandResolverHandler), false));
        foreach(var type in types) {
            var keyCode = ((InputCommandResolverHandler)type.GetCustomAttribute(typeof(InputCommandResolverHandler), false))?.Code;
            if (keyCode != null && type.GetInterface(nameof(IInputCommandResolver)) != null)
            {
                Register((KeyCode)keyCode, Activator.CreateInstance(type) as IInputCommandResolver);
            }
        }        
    }
    
}


[AttributeUsage(AttributeTargets.Class)]
public class InputCommandResolverHandler : Attribute {
    public KeyCode Code {get; set;}
    public InputCommandResolverHandler(KeyCode code) {
        Code = code;
    }
}