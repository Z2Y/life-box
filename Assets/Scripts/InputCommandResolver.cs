using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DefaultNamespace
{
    public class InputCommandResolver : MonoBehaviour
    {
        public static InputCommandResolver Instance { get; private set;}
        
        private static readonly Dictionary<KeyCode, ICommandResolver> Resolvers = new ();
        private void Awake()
        {
            Instance = this;
            RegisterCommandResolver();
        }

        private void Update()
        {
            if (Input.anyKeyDown)
            {
                var handlers = Resolvers.Where((pair) => Input.GetKeyDown(pair.Key)).ToList();
                handlers.ForEach(pair => pair.Value.Resolve("keydown", null, null));
            }

        }

        private void Register(KeyCode code, ICommandResolver resolver) {
            if (Resolvers.ContainsKey(code)) {
                Resolvers[code] = resolver;
            } else {
                Resolvers.Add(code, resolver);
            }
        }
        
        private void RegisterCommandResolver() {
            var asm = typeof(CommandResolverHandler).Assembly;
            var types = asm.GetExportedTypes().Where((type) => type.IsDefined(typeof(InputCommandResolverHandler), false)).ToArray();
            foreach(var type in types) {
                var keyCode = ((InputCommandResolverHandler)type.GetCustomAttribute(typeof(InputCommandResolverHandler), false))?.Code;
                if (keyCode != null && type.GetInterface(nameof(ICommandResolver)) != null)
                {
                    Register((KeyCode)keyCode, Activator.CreateInstance(type) as ICommandResolver);
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
}