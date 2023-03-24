using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using NPBehave;
using UnityEngine;
using UnityEngine.Events;

namespace Logic.Detector
{
    public class SelectorDetector : Selector, IDetector
    {
        private UnityAction<IDetector, Collider2D> onDetectCallback;
        private UnityAction<IDetector, Collider2D> onEndDetectCallback;
        private bool isRoot;
        public SelectorDetector(UnityAction<IDetector, Collider2D> callback, params BaseDetector[] detectors) : base(detectors.OfType<Node>().ToArray())
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
            if (isRoot && currentState == State.INACTIVE)
            {
                Debug.Log("Stop Root");
                RootNode.Stop();
                RootNode.ChildStopped(this, false);
            }
        }
        
        private async void fireCallbackAsync(Collider2D collision)
        {
            var phase = Blackboard.Get<DetectPhase>("phase");
            if (phase == DetectPhase.Enter)
            {
                if (onDetectCallback == null) return;
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                onDetectCallback?.Invoke(this, collision);
            }
            else
            {
                if (onEndDetectCallback == null) return;
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                onEndDetectCallback?.Invoke(this, collision);
            }
        }

        public void Start(DetectPhase phase, Blackboard blackboard, Collider2D collision)
        {
            RootNode ??= new Root(blackboard, this);
            isRoot = true;
            if (RootNode.CurrentState == State.INACTIVE)
            {
                blackboard.Set("collision", collision);
                blackboard.Set("phase", phase);
                RootNode.Start();
            }
        }
        
        protected override void DoStop()
        {
            Stopped(false);
            if (isRoot)
            {
                Debug.Log("Stop Root");
                RootNode.Stop();
                RootNode.ChildStopped(this, false);
            }
        }

        public void onDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onDetectCallback = callback;
        }
        
        public void onEndDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onEndDetectCallback = callback;
        }

        public void offEndDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onEndDetectCallback -= callback;
        }

        public void offDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onDetectCallback -= callback;
        }
    }

    public class SequenceDetector : Sequence, IDetector
    {
        private UnityAction<IDetector, Collider2D> onDetectCallback;
        private UnityAction<IDetector, Collider2D> onEndDetectCallback;
        private int currentIndex = -1;
        private bool isRoot;
        
        public SequenceDetector(UnityAction<IDetector, Collider2D> callback, params BaseDetector[] detectors) : base(detectors.OfType<Node>().ToArray())
        {
            onDetectCallback = callback;
        }
        
        protected override void DoChildStopped(Node child, bool result)
        {
            base.DoChildStopped(child, result);
            if (isRoot && currentState == State.INACTIVE)
            {
                Debug.Log("Stop Root");
                RootNode.Stop();
                RootNode.ChildStopped(this, false);
            }
            
            if (++currentIndex >= Children.Length && result)
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
            isRoot = true;
            if (RootNode.CurrentState == State.INACTIVE)
            {
                blackboard.Set("collision", collision);
                blackboard.Set("phase", phase);
                RootNode.Start();
            }
        }
        
        protected override void DoStop()
        {
            Stopped(false);
            if (isRoot)
            {
                Debug.Log("Stop Root");
                RootNode.Stop();
                RootNode.ChildStopped(this, false);
            }
        }

        public void onDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onDetectCallback = callback;
        }
        
        public void onEndDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onEndDetectCallback = callback;
        }
        
        public void offEndDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onEndDetectCallback -= callback;
        }

        public void offDetect(UnityAction<IDetector, Collider2D> callback)
        {
            onDetectCallback -= callback;
        }
    }
}