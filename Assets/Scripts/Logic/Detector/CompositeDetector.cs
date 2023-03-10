using System;
using System.Linq;
using NPBehave;
using UnityEngine;

namespace Logic.Detector
{
    public class SelectorDetector : Selector, IDetector
    {
        private Action<IDetector, Collision2D> onDetectCallback;
        private Action<IDetector, Collision2D> onEndDetectCallback;

        public SelectorDetector(Action<IDetector, Collision2D> callback, params BaseDetector[] detectors) : base(detectors.OfType<Node>().ToArray())
        {
            onDetectCallback = callback;
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            base.DoChildStopped(child, result);
            if (result)
            {
                fireCallbackAsync(Blackboard.Get<Collision2D>("collision"));
            }
        }
        
        private async void fireCallbackAsync(Collision2D collision)
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

        public void Start(DetectPhase phase, Blackboard blackboard, Collision2D collision)
        {
            Blackboard = blackboard;
            if (currentState == State.INACTIVE)
            {
                Blackboard.Set("phase", phase);
                Blackboard.Set("collision", collision);
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
    }

    public class SequenceDetector : Sequence, IDetector
    {
        private Action<IDetector, Collision2D> onDetectCallback;
        private Action<IDetector, Collision2D> onEndDetectCallback;
        private int currentIndex = -1;
        
        public SequenceDetector(Action<IDetector, Collision2D> callback, params BaseDetector[] detectors) : base(detectors.OfType<Node>().ToArray())
        {
            onDetectCallback = callback;
        }
        
        protected override void DoChildStopped(Node child, bool result)
        {
            base.DoChildStopped(child, result);

            if (!result) return;
            if (++currentIndex >= Children.Length)
            {
                fireCallbackAsync(Blackboard.Get<Collision2D>("collision"));
            }
        }
        
        private async void fireCallbackAsync(Collision2D collision)
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

        public void Start(DetectPhase phase, Blackboard blackboard, Collision2D collision)
        {
            Blackboard = blackboard;
            if (currentState == State.INACTIVE)
            {
                Blackboard.Set("collision", collision);
                Blackboard.Set("phase", phase);
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
    }
}