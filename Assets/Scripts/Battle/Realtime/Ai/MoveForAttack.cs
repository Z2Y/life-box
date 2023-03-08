using Controller;
using NPBehave;
using UnityEngine;
using Utils;

namespace Battle.Realtime.Ai
{
    public class MoveForAttack : Task
    {
        private readonly float attackRange;
        private readonly Vector3 speed;
        private Collider2D enemy;
        private Transform selfTransform;
        private WorldMapController map;
        private NPCMoveTask moveTask;
        
        public MoveForAttack(float attackRange, Vector3 speed) : base("MoveForAttack")
        {
            this.attackRange = attackRange;
            this.speed = speed;
        }

        protected override void DoStart()
        {

            selfTransform = Blackboard.Get<Transform>("self_transform");
            map = Blackboard.Get<WorldMapController>("word_map");
            moveTask = Blackboard.Get<NPCMoveTask>("move_task");
            enemy = Blackboard.Get<Collider2D>("enemy_target");
            
            // todo
            if (ReferenceEquals(enemy, null))
            {
                Stopped(true);
            }
            else
            {
                moveTask.Cancel();
                Clock.AddTimer(0.125f, 0, -1, update);
            }
            Debug.Log("Start Move Attack");
        }

        private void update()
        {
            var distance = (enemy.transform.position - selfTransform.position).magnitude;
            if (distance < attackRange)
            {
                if (IsActive)
                {
                    DoStop();
                }
                return;
            }
            
            var source = map.Ground.WorldToCell(selfTransform.position);
            var position = enemy.transform.position;
            var dest = map.Ground.WorldToCell(position);
            var aRoute = SimplePoolManager.Get<AstarRoute>();
                
            var path = aRoute.FindPath(map, source, dest);

            if (path == null)
            {
                return;
            }
            
            var destWorldPos = position;
            if (path.ParentRoute != null) {
                destWorldPos = map.Ground.CellToWorld(path.ParentRoute.Point);
                destWorldPos.z = 0;
            }
            
            path.Dispose();

            moveTask.Cancel();
            moveTask.DoMove(destWorldPos, speed, -1);
        }

        protected override void DoStop()
        {
            enemy = null;
            map = null;
            selfTransform = null;
            moveTask.Cancel();
            moveTask = null;
            Debug.Log("Stop Move Attack");
            Clock.RemoveTimer(update);
            Stopped(true);
        }
    }
}