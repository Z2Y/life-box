using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Cysharp.Threading.Tasks;
using Logic.Map;
using UnityEngine;
using Object = UnityEngine.Object;

[CommandResolverHandler("NPCMove")]
public class NPCMoveCommand : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var npc = NPCController.GetCharacterController(Convert.ToInt64(args[0]));

        if (npc == null)
        {
            return null;
        }

        return await npc.Movement.MoveTo(
            npc.transform.position + new Vector3(Convert.ToSingle(args[1]), Convert.ToSingle(args[2])), Vector3.one * Convert.ToSingle(args[3]),
            -1).SuppressCancellationThrow();
    }
}



[CommandResolverHandler("NPCMoveToNearGate")]
public class NPCMoveToNearGate : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var npc = NPCController.GetCharacterController(Convert.ToInt64(args[0]));

        if (npc == null)
        {
            return false;
        }
        
        var map = LifeEngine.Instance.Map;

        var place = map.ActivePlaces.FirstOrDefault((place) => place.bounds.Contains(npc.transform.position));

        if (place == null)
        {
            return false;
        }

        var gates = place.transform.Find("Gate").GetComponentsInChildren<IMapGate>();

        var distance = float.MaxValue;
        Component targetGate = null;

        foreach (var gate in gates)
        {
            if (!gate.Interactive())
            {
                continue;
            }

            var gateDis = (((Component)gate).transform.position - npc.transform.position).magnitude;

            if (gateDis  < distance)
            {
                targetGate = (Component)gate;
                distance = gateDis;
            }
        }

        if (targetGate != null)
        {
            return await npc.Movement.MoveTo(targetGate.transform.position, Vector3.one * Convert.ToSingle(args[1]), -1).SuppressCancellationThrow();
        }

        return false;
    }
}



[CommandResolverHandler("ShowNPCNearPlayer")]
public class ShowNPCNearPlayer : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var npc = await NPCController.LoadCharacterAsync(Convert.ToInt64(args[0]));

        if (npc == null)
        {
            return null;
        }

        var offset = new Vector3(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]));

        npc.transform.position = LifeEngine.Instance.MainCharacter.transform.position + offset;

        npc.Animator.Turn(-offset.x);
        
        npc.gameObject.SetActive(true);

        return await this.Done();
    }
}

[CommandResolverHandler("HideNPC")]
public class HideNPC : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var npc = NPCController.GetCharacterController(Convert.ToInt64(args[0]));

        if (npc == null)
        {
            return null;
        }
        
        // npc.gameObject.SetActive(false);
        return await this.Done();
    }
}

[CommandResolverHandler("UnloadNPC")]
public class UnloadNPC : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var npc = NPCController.GetCharacterController(Convert.ToInt64(args[0]));

        if (npc == null)
        {
            return null;
        }
        
        Object.Destroy(npc.gameObject);
        return await this.Done();
    }
}