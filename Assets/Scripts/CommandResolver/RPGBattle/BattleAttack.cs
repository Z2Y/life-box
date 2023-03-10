using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Controller;
using UnityEngine;

namespace Battle.RPGBattle
{
    [CommandResolverHandler("RPGBattle.NormalAttack")]
    public class BattleNormalAttack : CommandResolver
    {
        public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
        {
            var characterID = int.Parse(arg);
            var character = await NPCController.LoadCharacterAsync(characterID);
            if (ReferenceEquals(character, null))
            {
                return null;
            }
            
            character.Animator.AttackNormal();
            return null;
        }
    }
}