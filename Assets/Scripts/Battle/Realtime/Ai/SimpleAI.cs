using Controller;
using UnityEngine;
using NPBehave;

namespace Battle.Realtime.Ai
{
    public class SimpleAI : MonoBehaviour
    {
        private Blackboard ownBlackBoard;
        private Root behaviorTree;
        private FindDestRD destinationFinder;
        private int state = 0;

        private void Start()
        {
            ownBlackBoard = new Blackboard(null, UnityContext.GetClock());
            destinationFinder = new FindDestRD(10, transform.position );
            behaviorTree = createBehaviorTree();
            ownBlackBoard.Set("word_map", LifeEngine.Instance.Map);
            ownBlackBoard.Set("self_transform", transform);
            ownBlackBoard.Set("move_task", new NPCMoveTask());
            
#if UNITY_EDITOR
            var debugger = (Debugger)gameObject.AddComponent(typeof(Debugger));
            debugger.BehaviorTree = behaviorTree;
#endif
        }

        private Root createBehaviorTree()
        {
            return new Root(ownBlackBoard,
                new Service(0.1f, 0.5f, updateBlackBoard,new Selector(
                    new Condition(() => state == 0, new Sequence(
                        new FindPathTask(destinationFinder.GetResult()),
                        new PathMove(),
                        new WaitUntilStopped()
                    ))
                ))
            );
        }

        private void updateBlackBoard()
        {
        }
    }
}