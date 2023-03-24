using Controller;
using NPBehave;
using UnityEngine;
using Utils;

namespace Logic.Battle.Realtime.Ai
{
    using Random = UnityEngine.Random;

namespace Battle.Realtime.Ai
{
    public class MoveForSurround : Task
    {
        private readonly float surroundRange;
        private readonly Vector3 speed;
        private Collider2D enemy;
        private Transform selfTransform;
        private WorldMapController map;
        private NPCMoveTask moveTask;
        
        public MoveForSurround(float surroundRange, Vector3 speed) : base("MoveForAttack")
        {
            this.surroundRange = surroundRange;
            this.speed = speed + new Vector3(0.05f - Random.Range(0f, 0.1f), 0.05f - Random.Range(0f, 0.1f), 0);
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
        }

        private void update()
        {
            var selfPosition = selfTransform.position;
            var enemyPosition = enemy.transform.position;
            var distance = (enemyPosition - selfPosition).magnitude;

            var source = map.Ground.WorldToCell(selfPosition);
            var direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
            if (distance < surroundRange)
            {
                direction = (enemyPosition - selfPosition).normalized + direction * 0.2f;
            }
            var position = enemyPosition + direction * surroundRange;
            
            
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

            if (distance > surroundRange || !moveTask.IsRunning)
            {
                moveTask.Cancel();
                moveTask.DoMove(destWorldPos, speed, -1);
                moveTask.animator.SetAim(enemy.transform);
            }
        }

        protected override void DoStop()
        {
            enemy = null;
            map = null;
            selfTransform = null;
            moveTask.Cancel();
            moveTask.animator.SetAim(null);
            moveTask = null;
            Clock.RemoveTimer(update);
            Stopped(true);
        }
    }
}
}