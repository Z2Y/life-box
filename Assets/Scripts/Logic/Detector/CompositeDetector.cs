using System;
using System.Linq;
using NPBehave;
using UnityEngine;

namespace Logic.Detector
{
    public class SelectorDetector : Selector, IDetector
    {
        private Action<IDetector, Collider2D> onDetectCallback;
        private Action<IDetector, Collider2D> onEndDetectCallback;

        public SelectorDetector(Action<IDetector, Collider2D> callback, params BaseDetector[] detectors) : base(detectors.OfType<Node>().ToArray())
        {
            onDetectCallback = callback;
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            base.DoChildStopped(child, result);
            if (result)
            {
                fireCallbackAsync(Blackboard.Get<Collider2D>("collision"));
            }
        }
        
        private async void fireCallbackAsync(Collider2D collision)
        {
            var phase = Blackboard.Get<DetectPhase>("phase");
            if (phase == DetectPhase.Enter)
            {
                if (onDetectCallback == null) return;
                await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
                onDetectCallback?.Invoke(this, collision);
            }
            else
            {
                if (onEndDetectCallback == null) return;
                await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
                onEndDetectCallback?.Invoke(this, collision);
            }
        }

        public void Start(DetectPhase phase, Blackboard blackboard, Collider2D collision)
        {
            RootNode ??= new Root(blackboard, this);
            if (currentState == State.INACTIVE)
            {
                Blackboard.Set("phase", phase);
                Blackboard.Set("collision", collision);
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
    }

    public class SequenceDetector : Sequence, IDetector
    {
        private Action<IDetector, Collider2D> onDetectCallback;
        private Action<IDetector, Collider2D> onEndDetectCallback;
        private int currentIndex = -1;
        
        public SequenceDetector(Action<IDetector, Collider2D> callback, params BaseDetector[] detectors) : base(detectors.OfType<Node>().ToArray())
        {
            onDetectCallback = callback;
        }
        
        protected override void DoChildStopped(Node child, bool result)
        {
            base.DoChildStopped(child, result);

            if (!result) return;
            if (++currentIndex >= Children.Length)
            {
                fireCallbackAsync(Blackboard.Get<Collider2D>("collision"));
            }
        }
        
        private async void fireCallbackAsync(Collider2D collision)
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

        public void Start(DetectPhase phase, Blackboard blackboard, Collider2D collision)
        {
            RootNode ??= new Root(blackboard, this);
            if (currentState == State.INACTIVE)
            {
                Blackboard.Set("collision", collision);
                Blackboard.Set("phase", phase);
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
    }
}