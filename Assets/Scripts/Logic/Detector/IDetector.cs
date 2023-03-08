using System;
using UnityEngine;

namespace Logic.Detector 
{
    public interface IDetector
    {
        public bool isTarget(Collision collision);

        public void onDetect(Action<Collision> callback);
    }

    public abstract class BaseDetector
    {
        protected Action<Collision> onDetectCallback;
    }
}