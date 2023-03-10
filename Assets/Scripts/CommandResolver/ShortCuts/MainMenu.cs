using System;
using UnityEngine;

namespace ShortCuts
{
    [InputCommandResolverHandler(KeyCode.Escape)]
    public class MainMenuResolver : IInputCommandResolver
    {
        public void Resolve(KeyCode code)
        {
            throw new NotImplementedException();
        }
    }
}