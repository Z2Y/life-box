using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultNamespace;
using UnityEngine;

namespace ShortCuts
{
    [InputCommandResolverHandler(KeyCode.Escape)]
    public class MainMenuResolver : CommandResolver
    {
        public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
        {
            throw new NotImplementedException();
        }
    }
}