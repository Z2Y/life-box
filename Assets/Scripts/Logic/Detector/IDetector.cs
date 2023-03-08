using UnityEngine;

namespace Logic.Detector 
{
    public interface IDetector
    {
        public bool isTarget(Collision collision);
    }
}