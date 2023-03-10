using System;
using NPBehave;
using UnityEngine;

namespace Logic.Detector 
{
    public interface IDetector
    {
        public void Start(DetectPhase phase, Blackboard blackboard, Collision2D collision);
        public void onDetect(Action<IDetector, Collision2D> callback);
        public void onEndDetect(Action<IDetector, Collision2D> callback);

    }

    public enum DetectPhase
    {
        Enter,
        Exit
    }

    public abstract class BaseDetector : NPBehave.Task, IDetector
    {
        protected Action<IDetector, Collision2D> onDetectCallback;
        protected Action<IDetector, Collision2D> onEndDetectCallback;

        protected BaseDetector(string name) : base(name)
        {
        }
        
        protected override void DoStart()
        {
            Stopped(isTarget(Blackboard.Get<Collision2D>("collision")));
        }

        protected override void DoStop()
        {
            Stopped(false);
        }

        public abstract bool isTarget(Collision2D collision);

        public void Start(DetectPhase phase, Blackboard blackboard, Collision2D collision)
        {
            // this.Blackboard = blackboard;
            Blackboard = blackboard;
            if (currentState == State.INACTIVE)
            {
                blackboard.Set("collision", collision);
                blackboard.Set("phase", phase);
                Start();
            }
        }

        public void onDetect(Action<IDetector, Collision2D> callback)
        {
            onDetectCallback = callback;
        }

        public void onEndDetect(Action<IDetector, Collision2D> callback)
        {
            onEndDetectCallback = callback;
        }

        protected async void fireCallbackAsync(Collision2D collision)
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