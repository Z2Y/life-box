using Controller;
using NPBehave;
using UnityEngine;

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
                return;
            }
            else
            {
                moveTask.Cancel();
                Clock.AddTimer(0.125f, 0, -1, update);
            }
        }

        private void update()
        {
            var distance = (enemy.transform.position - selfTransform.position).magnitude;
            if (distance < attackRange)
            {
                Stopped(true);
                return;
            }
            
            var source = map.Ground.WorldToCell(selfTransform.position);
            var dest = map.Ground.WorldToCell(enemy.transform.position);
            var aRoute = AstarRoute.Get();
                
            var path = aRoute.FindPath(map, source, dest);
            
            var destWorldPos = enemy.transform.position;
            if (path.ParentRoute != null) {
                destWorldPos = map.Ground.CellToWorld(path.ParentRoute.Point);
                destWorldPos.z = 0;
            }

            while (path != null)
            {
                var parent = path.ParentRoute;
                path.Dispose();
                path = parent;
            }

            moveTask.Cancel();
            moveTask.DoMove(destWorldPos, speed, -1);
        }

        protected override void DoStop()
        {
            enemy = null;
            moveTask = null;
            map = null;
            selfTransform = null;
            Clock.RemoveTimer(update);
        }
    }
}