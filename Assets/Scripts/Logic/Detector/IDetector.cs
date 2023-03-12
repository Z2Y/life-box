using System;
using NPBehave;
using UnityEngine;
using UnityEngine.Events;

namespace Logic.Detector 
{
    public interface IDetector
    {
        public void Start(DetectPhase phase, Blackboard blackboard, Collider2D collision);
        public void onDetect(UnityAction<IDetector, Collider2D> callback);
        public void onEndDetect(UnityAction<IDetector, Collider2D> callback);
        public void offEndDetect(UnityAction<IDetector, Collider2D> callback);
        public void offDetect(UnityAction<IDetector, Collider2D> callback);

    }

    public enum DetectPhase
    {
        Enter,
        Exit
    }

    public abstract class BaseDetector : Task, IDetector
    {
        protected UnityAction<IDetector, Collider2D> onDetectCallback;
        protected UnityAction<IDetector, Collider2D> onEndDetectCallback;
        private bool isRoot;

        protected BaseDetector(string name) : base(name)
        {
        }
        
        protected override void DoStart()
        {
            var collision = Blackboard.Get<Collider2D>("collision");
            var success = isTarget(collision);
            if (success)
            {
                fireCallbackAsync(collision);
            }
            Stopped(success);
            if (isRoot)
            {
                RootNode.Stop();
                RootNode.ChildStopped(this, success);
            }
        }

        protected abstract bool isTarget(Collider2D collision);

        public void Start(DetectPhase phase, Blackboard blackboard, Collider2D collision)
        {
            RootNode ??= new Root(blackboard, this);
            isRoot = true;
            if (RootNode.CurrentState == State.INACTIVE)
            {
                blackboard.Set("collision", collision);
                blackboard.Set("phase", phase);
                // Debug.Log($"Start Detect {this.GetType().Name} {collision.gameObject.name} {phase}");
                RootNode.Start();
            }
        }

        public void onDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onDetectCallback += callback;
        }

        public void onEndDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onEndDetectCallback += callback;
        }

        public void offEndDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onEndDetectCallback -= callback;
        }

        public void offDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onDetectCallback -= callback;
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