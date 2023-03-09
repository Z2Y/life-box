using UnityEngine;

namespace Logic.Detector.Scriptable
{
    public abstract class ScriptableDetectorBase : ScriptableObject
    {
        public abstract IDetector GetDetector();
    }
    
    public class ScriptableDetector<T>: ScriptableDetectorBase where T : IDetector, new()
    {
        private readonly T detector = new ();

        public override IDetector GetDetector()
        {
            return detector;
        }
    }
}