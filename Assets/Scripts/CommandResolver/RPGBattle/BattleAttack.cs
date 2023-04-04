using System.Collections.Generic;
using Controller;
using Cysharp.Threading.Tasks;

namespace Battle.RPGBattle
{
    [CommandResolverHandler("RPGBattle.NormalAttack")]
    public class BattleNormalAttack : CommandResolver
    {
        public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
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