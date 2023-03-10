using System;
using System.Collections.Generic;
using Controller;
using Logic.Detector;
using UnityEngine;

namespace ShortCuts
{
    public class InteractMenuHandler : IInputCommandResolver
    {
        private readonly string command;
        private readonly NPCInteractController controller;
        private readonly Dictionary<string, object> env = new();

        public InteractMenuHandler(NPCInteractController controller, string command)
        {
            this.command = command;
            this.controller = controller;
        }

        public void Resolve(KeyCode code)
        {
            env["activeDetectors"] = controller.activeDetectors;
            Debug.Log(command);
            Debug.Log(env["activeDetectors"]);
            command.ExecuteExpressionAsync(env).Coroutine();
        }
    }
}