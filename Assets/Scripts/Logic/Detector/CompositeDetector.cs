using System;
using System.Linq;
using NPBehave;
using UnityEngine;

namespace Logic.Detector
{
    public class SelectorDetector : Selector, IDetector
    {
        private Action<IDetector, Collision> onDetectCallback;
        private Action<IDetector, Collision> onEndDetectCallback;

        public SelectorDetector(Action<IDetector, Collision> callback, params BaseDetector[] detectors) : base(detectors.OfType<Node>().ToArray())
        {
            onDetectCallback = callback;
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            base.DoChildStopped(child, result);
            if (result)
            {
                fireCallbackAsync(Blackboard.Get<Collision>("collision"));
            }
        }
        
        private async void fireCallbackAsync(Collision collision)
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

        public void Start(DetectPhase phase, Collision collision)
        {
            if (RootNode.CurrentState == State.INACTIVE)
            {
                RootNode.Blackboard.Set("phase", phase);
                RootNode.Blackboard.Set("collision", collision);
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
    }

    public class SequenceDetector : Sequence, IDetector
    {
        private Action<IDetector, Collision> onDetectCallback;
        private Action<IDetector, Collision> onEndDetectCallback;
        private int currentIndex = -1;
        
        public SequenceDetector(Action<IDetector, Collision> callback, params BaseDetector[] detectors) : base(detectors.OfType<Node>().ToArray())
        {
            onDetectCallback = callback;
        }
        
        protected override void DoChildStopped(Node child, bool result)
        {
            base.DoChildStopped(child, result);

            if (!result) return;
            if (++currentIndex >= Children.Length)
            {
                fireCallbackAsync(Blackboard.Get<Collision>("collision"));
            }
        }
        
        private async void fireCallbackAsync(Collision collision)
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

        public void Start(DetectPhase phase, Collision collision)
        {
            if (RootNode.CurrentState == State.INACTIVE)
            {
                RootNode.Blackboard.Set("collision", collision);
                RootNode.Blackboard.Set("phase", phase);
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
    }
}