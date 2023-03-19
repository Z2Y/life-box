using System;
using UnityEngine;

namespace Logic.Detector.Scriptable
{
    public abstract class ScriptableDetectorBase : ScriptableObject
    {
        public abstract IDetector GetDetector();
    }
    
    public class ScriptableDetector<T>: ScriptableDetectorBase where T : IDetector, new()
    {
        public override IDetector GetDetector()
        {
            return Activator.CreateInstance<T>();
        }
    }
}