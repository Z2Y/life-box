using System;
using NPBehave;
using UnityEngine;

namespace Logic.Detector 
{
    public interface IDetector
    {
        public void Start(DetectPhase phase, Blackboard blackboard, Collider2D collision);
        public void onDetect(Action<IDetector, Collider2D> callback);
        public void onEndDetect(Action<IDetector, Collider2D> callback);

    }

    public enum DetectPhase
    {
        Enter,
        Exit
    }

    public abstract class BaseDetector : NPBehave.Task, IDetector
    {
        protected Action<IDetector, Collider2D> onDetectCallback;
        protected Action<IDetector, Collider2D> onEndDetectCallback;

        protected BaseDetector(string name) : base(name)
        {
        }
        
        protected override void DoStart()
        {
            Stopped(isTarget(Blackboard.Get<Collider2D>("collision")));
        }

        protected override void DoStop()
        {
            Stopped(false);
        }

        protected abstract bool isTarget(Collider2D collision);

        public void Start(DetectPhase phase, Blackboard blackboard, Collider2D collision)
        {
            // this.Blackboard = blackboard;
            RootNode ??= new Root(blackboard, this);
            if (currentState == State.INACTIVE)
            {
                blackboard.Set("collision", collision);
                blackboard.Set("phase", phase);
                RootNode.Start();
            }
        }

        public void onDetect(Action<IDetector, Collider2D> callback)
        {
            onDetectCallback = callback;
        }

        public void onEndDetect(Action<IDetector, Collider2D> callback)
        {
            onEndDetectCallback = callback;
        }

        protected async void fireCallbackAsync(Collider2D collision)
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