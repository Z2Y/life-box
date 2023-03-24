using Controller;
using NPBehave;
using UnityEngine;
using Utils;

namespace Logic.Battle.Realtime.Ai
{
    public class FindPathTask : Task
    {
        private readonly FindDestRD destFinder;
        
        public FindPathTask(FindDestRD destFinder) : base("FindPathTask")
        {
            this.destFinder = destFinder;
        }

        protected override void DoStart()
        {
            var transform = Blackboard.Get<Transform>("self_transform");
            var map = Blackboard.Get<WorldMapController>("word_map");

            var destination = destFinder.GetResult();
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