using Controller;
using NPBehave;
using UnityEngine;
using Utils;

namespace Battle.Realtime.Ai
{
    public class FindPathTask : Task
    {
        private readonly Vector3 destination;
        
        public FindPathTask(Vector3 destination) : base("FindPathTask")
        {
            this.destination = destination;
        }

        protected override void DoStart()
        {

            var transform = Blackboard.Get<Transform>("self_transform");
            var map = Blackboard.Get<WorldMapController>("word_map");
            
            var source = map.Ground.WorldToCell(transform.position);
            var dest = map.Ground.WorldToCell(destination);
            var aRoute = SimplePoolManager.Get<AstarRoute>();
                
            var path = aRoute.FindPath(map, source, dest);

            if (path != null)
            {
                Blackboard.Set("path_route", path);
                Blackboard.Set("destination", destination);
                Stopped(true);
            }
            else
            {
                Stopped(false);
            }
        }

        protected override void DoStop()
        {
            Stopped(true);
        }
    }
}