using System;
using System.Collections.Generic;
using UnityEngine;

namespace Logic.Message.DefaultHandler
{
    public class MessageHandlerManager : MonoBehaviour
    {
        private static bool initialized;
        
        private static readonly Dictionary<Type, IMessageHandler> handlers = new();

        private void Awake()
        {
            foreach (var pair in handlers)
            {
                pair.Value.Enable();
            }
        }

        public static void EnableHandler(Type type)
        {
            if (handlers.TryGetValue(type, out var handler))
            {
                handler.Enable();
            }
        }

        public static void DisableHandler(Type type)
        {
            if (handlers.TryGetValue(type, out var handler))
            {
                handler.Disable();
            }
        }

        private static void Register(Type type, IMessageHandler handler) {
            if (handlers.ContainsKey(type)) {
                handlers[type] = handler;
            } else {
                handlers.Add(type, handler);
            }
        }
        private static void RegisterDefaultMessageHandler()
        {
            /*
            var asm = typeof(MessageHandlerManager).Assembly;
            var types = asm.GetExportedTypes().Where((type) => type.IsDefined(typeof(MessageDefaultHandler), false)).ToArray();
            foreach(var type in types) {
                if (type.GetInterface(nameof(IMessageHandler)) != null)
                {
                    Register(type, Activator.CreateInstance(type) as IMessageHandler);
                }
            }*/
            // Register by Manual
            Register(typeof(MainCharacterDeathHandler), new MainCharacterDeathHandler());
            Register(typeof(BattleCompleteHandler), new BattleCompleteHandler());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (initialized) return;
        
            RegisterDefaultMessageHandler();
            initialized = true;
        }
    }
}