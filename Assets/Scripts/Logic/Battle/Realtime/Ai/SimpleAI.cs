using Controller;
using Logic.Battle.Realtime.Ai.Battle.Realtime.Ai;
using NPBehave;
using UnityEngine;

namespace Logic.Battle.Realtime.Ai
{
    public class SimpleAI : MonoBehaviour
    {
        private Blackboard ownBlackBoard;
        private Root behaviorTree;
        private FindDestRD destinationFinder;
        private FindEnemy enemyFinder;
        private NPCMoveTask moveTask;
        private bool markStart;

        private void Start()
        {
            ownBlackBoard = new Blackboard(null, UnityContext.GetClock());
            destinationFinder = new FindDestRD(10, transform.position );
            enemyFinder = new FindEnemy(10, LayerMask.GetMask("Default"));
            behaviorTree = createBehaviorTree();
            ownBlackBoard.Set("self_transform", transform);
            ownBlackBoard.Set("move_task", moveTask = new NPCMoveTask());
            moveTask.rigidbody = GetComponent<Rigidbody2D>();
            moveTask.animator = GetComponent<IMoveAnimator>();
            if (markStart)
            { 
                StartAI();
            }

#if UNITY_EDITOR
            var debugger = (Debugger)gameObject.AddComponent(typeof(Debugger));
            debugger.BehaviorTree = behaviorTree;
#endif
        }

        private Root createBehaviorTree()
        {
            return new Root(ownBlackBoard,
                new Service(0.125f, 0.5f, updateBlackBoard,new Selector(
                    new BlackboardCondition("enemy_target", Operator.IS_NOT_SET, Stops.IMMEDIATE_RESTART , 
                        new Sequence(
                        new FindPathTask(destinationFinder),
                        new PathMove(),
                        new Wait(2f, 1f)
                    )),
                    new BlackboardCondition("enemy_target", Operator.IS_SET, Stops.IMMEDIATE_RESTART, 
                        new Selector(
                            new BlackboardCondition("enemy_distance", Operator.IS_SMALLER, 0.8f, Stops.IMMEDIATE_RESTART, 
                                new AnimalNormalAttack(GetComponent<IAttackAnimator>())),
                            new RandomSelector(
                                new MoveForSurround(1.5f, Vector3.one),
                                new MoveForAttack(0.8f, Vector3.one)
                            )
                        ))
                ))
            );
        }

        private void updateBlackBoard()
        {
            var previous = ownBlackBoard.Get<Collider2D>("enemy_target");
            var current = enemyFinder.GetResult(transform.position, previous);
            

            if (!ReferenceEquals(current, null))
            {
                ownBlackBoard.Set("enemy_target", current);
                ownBlackBoard.Set("enemy_distance", (current.transform.position - transform.position).magnitude);
            }
            else
            {
                ownBlackBoard.Unset("enemy_target");
                ownBlackBoard.Set("enemy_distance", float.MaxValue);
            }
        }

        private void Update()
        {
            moveTask.Update();
        }

        private void onBeforeStart()
        {
            ownBlackBoard.UpdateClock(UnityContext.GetClock());
            ownBlackBoard.Set("word_map", LifeEngine.Instance.Map);
            behaviorTree.UpdateClock();
        }

        private void onAfterStop()
        {
            ownBlackBoard.Unset("enemy_target");
            ownBlackBoard.Unset("enemy_distance");
        }

        public void StartAI()
        {
            if (behaviorTree is { IsActive: false })
            {
                onBeforeStart();
                behaviorTree.Start();
            }
            else
            {
                markStart = true;
            }
        }

        public void StopAI()
        {
            if (behaviorTree is { IsActive: true })
            {
                behaviorTree.Stop();
                onAfterStop();
            }
            else
            {
                markStart = false;
            }
        }
    }
}