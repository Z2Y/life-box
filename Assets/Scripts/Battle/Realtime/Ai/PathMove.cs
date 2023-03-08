using System;
using System.Threading.Tasks;
using Controller;
using UnityEngine;

namespace Battle.Realtime.Ai
{
    public class PathMove : NPBehave.Task
    {
        private readonly Vector3 speed;
        private Vector3 destination;
        private RoutePath path;
        private NPCMoveTask moveTask;
        private WorldMapController map;
        
        public PathMove() : base("PathMove")
        {
            speed = Vector3.one.normalized;
        }

        protected override void DoStart()
        {
            map = Blackboard.Get<WorldMapController>("word_map");
            moveTask = Blackboard.Get<NPCMoveTask>("move_task");
            path = Blackboard.Get<RoutePath>("path_route");
            destination = Blackboard.Get<Vector3>("destination");
            moveTask.Cancel(); //  cancel previous task
            if (isArrived())
            {
                Stopped(true);
            }
            else
            {
                moveByRoutePath().Coroutine();
            }
        }

        private async Task moveByRoutePath()
        {

            moveTask.npcTransform = Blackboard.Get<Transform>("self_transform");
            var movePath = path;

            while (movePath != null)
            {
                var dest = destination;
                if (movePath.ParentRoute != null) {
                    dest = map.Ground.CellToWorld(movePath.ParentRoute.Point);
                    dest.z = 0;
                }

                try
                {
                    await moveTask.DoMove(dest, speed, -1);
                }
                catch (OperationCanceledException e)
                {
                    
                    Stopped(false);
                    break;
                }
                movePath = movePath.ParentRoute;
            }
            
            
            Stopped(movePath == null);
            // clean up
            path?.Dispose();
        }
        
        private bool isArrived() {
            return path == null;
        }

        protected override void DoStop()
        {
            moveTask?.Cancel();
            moveTask = null;
            map = null;
            Stopped(true);
        }
    }
}