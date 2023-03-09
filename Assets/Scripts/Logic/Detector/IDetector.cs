using System;
using UnityEngine;

namespace Logic.Detector 
{
    public interface IDetector
    {
        public void Start(DetectPhase phase, Collision collision);
        public void onDetect(Action<IDetector, Collision> callback);
        public void onEndDetect(Action<IDetector, Collision> callback);

    }

    public enum DetectPhase
    {
        Enter,
        Exit
    }

    public abstract class BaseDetector : NPBehave.Task, IDetector
    {
        protected Action<IDetector, Collision> onDetectCallback;
        protected Action<IDetector, Collision> onEndDetectCallback;

        protected BaseDetector(string name) : base(name)
        {
        }
        
        protected override void DoStart()
        {
            Stopped(isTarget(Blackboard.Get<Collision>("collision")));
        }

        protected override void DoStop()
        {
            Stopped(false);
        }

        public abstract bool isTarget(Collision collision);

        public void Start(DetectPhase phase, Collision collision)
        {
            if (RootNode.CurrentState == State.INACTIVE)
            {
                Blackboard.Set("collision", collision);
                Blackboard.Set("phase", phase);
                RootNode.Start();
            }
        }

        public void onDetect(Action<IDetector, Collision> callback)
        {
            onDetectCallback = callback;
        }

        public void onEndDetect(Action<IDetector, Collision> callback)
        {
            onEndDetectCallback = callback;
        }

        protected async void fireCallbackAsync(Collision collision)
        {
            var phase = Blackboard.Get<DetectPhase>("phase");
            if (phase == DetectPhase.Enter)
            {
                if (onDetectCallback == null) return;
                // await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
                onDetectCallback?.Invoke(this, collision);
            }
            else
            {
                if (onEndDetectCallback == null) return;
                // await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
                onEndDetectCallback?.Invoke(this, collision);
            }
        }
    }
}