using System.Collections.Generic;
using System.Threading.Tasks;
using Logic.Detector;
using Logic.Map;
using UnityEngine;

namespace Interact
{
    [CommandResolverHandler("InteractToGate")]
    public class MapGateHandler : CommandResolver
    {
        public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
        {
            var activeDetectors = env["activeDetectors"] as HashSet<KeyValuePair<IDetector, Collider2D>>;

            foreach (var pair in activeDetectors!)
            {
                if (pair.Key is MapGateDetector)
                {
                    pair.Value.GetComponent<IMapGate>().OnEnter();
                    break;
                }
            }

            return await this.Done();
        }
    }
}