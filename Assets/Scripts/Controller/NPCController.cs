using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using ModelContainer;
using UnityEngine;
using BattleSkillAction = Battle.Realtime.BattleSkillAction;

// ReSharper disable MemberCanBePrivate.Global

namespace Controller
{
    public class NPCController : MonoBehaviour
    {
        private static readonly Dictionary<long, NPCController> lookup = new ();
        
        [SerializeField] private long characterID;
        
        private CollisionDetector collisionDetector;
        public Character character { get; private set; }
        public NPCAnimationController Animator { get; private set; }
        public NPCMovementController Movement { get; private set;  }

        private void Awake()
        {
            character = CharacterCollection.Instance.GetCharacter(characterID);
            Animator = gameObject.AddComponent<NPCAnimationController>();
            Movement = gameObject.AddComponent<NPCMovementController>();
            collisionDetector = gameObject.AddComponent<CollisionDetector>();
            collisionDetector.enabled = false;
        }

        public void SetLocation(Location location)
        {
            // do map position translate
            transform.position = location.Position;
        }

        public void SetAsPlayer(bool isPlayer)
        {
            Movement.SetAsPlayer(isPlayer);
            collisionDetector.enabled = isPlayer;
            if (isPlayer)
            {
                addSkillShortCuts(KeyCode.Mouse0, SkillCollection.Instance.GetSkill(3));
            }
            else
            {
                removeAllShortCuts();
            }
        }

        public void removeAllShortCuts()
        {
            var shortCuts = gameObject.GetComponents<Battle.Realtime.BattleSkill>();
            foreach (var battleSkill in shortCuts)
            {
                Destroy(battleSkill);
            }
        }

        public void addSkillShortCuts(KeyCode keyCode, Skill skill)
        {
            var self = gameObject;
            var skillControl = self.AddComponent<Battle.Realtime.BattleSkill>();
            skillControl.keycode = keyCode;
            skillControl.action = new Battle.Realtime.BattleSkillAction()
            {
                skill = skill,
                self = self,
                meleeSwordType = "Sword_1"
            };
        }

        public void SetBodyScale(Vector2 bodyScale)
        {
            Animator.SetBodyScale(bodyScale);
        }
        
        public static async Task<NPCController> LoadCharacterAsync(long characterID)
        {
            var controller = GetCharacterController(characterID);
            if (controller != null)
            {
                return controller;
            }

            var character = CharacterCollection.Instance.GetCharacter(characterID);
            
            

            if (character == null)
            {
                return null;
            }
            var loader = Resources.LoadAsync<GameObject>($"Prefabs/character/{character.ModelID}");

            await YieldCoroutine.WaitForInstruction(loader);
            
            if (loader.asset == null)
            {
                return null;
            }

            var obj = Instantiate(loader.asset as GameObject, GameObject.Find("CharacterRoot").transform).GetComponent<NPCController>();
            lookup.Add(characterID, obj);
            return obj;
        }
        
        public static NPCController GetCharacterController(long characterID)
        {
            return lookup.TryGetValue(characterID, out var controller) ? controller : null;
        }
    }
}