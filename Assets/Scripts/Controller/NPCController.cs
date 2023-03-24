using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using Logic.Battle.Realtime;
using Logic.Battle.Realtime.SkillAction;
using ModelContainer;
using UnityEngine;
using Character = Model.Character;

// ReSharper disable MemberCanBePrivate.Global

namespace Controller
{
    public class NPCController : MonoBehaviour
    {
        private static readonly Dictionary<long, NPCController> lookup = new ();
        
        [SerializeField] private long characterID;
        
        public CollisionDetector Detector { get; private set; }

        public List<BattleSkill> skillShortCuts;
        public Character character { get; private set; }
        public NPCAnimationController Animator { get; private set; }
        public NPCMovementController Movement { get; private set;  }
        public NPCInteractController Interact { get; private set; }
        public LifeProperty Property { get; private set; }

        private void Awake()
        {
            character = CharacterCollection.Instance.GetCharacter(characterID);
            Animator = gameObject.AddComponent<NPCAnimationController>();
            Movement = gameObject.AddComponent<NPCMovementController>();
            Interact = gameObject.GetComponent<NPCInteractController>();
            Detector = gameObject.AddComponent<CollisionDetector>();
            gameObject.AddComponent<NPCHitController>();
            Detector.enabled = false;
            Property = LifePropertyFactory.Random(40);
        }

        public void SetLocation(Location location)
        {
            // do map position translate
            transform.position = location.Position;
        }

        public void SetAsPlayer(bool isPlayer)
        {
            Movement.SetAsPlayer(isPlayer);
            Detector.enabled = isPlayer;
            if (isPlayer)
            {
                addSkillShortCuts(KeyCode.Mouse0, new SwordSkillAction()
                {
                    skill = SkillCollection.Instance.GetSkill(3),
                    self = gameObject,
                    meleeSwordType = "Sword_1"
                });
                addSkillShortCuts(KeyCode.Space, new SlideSkillAction()
                {
                    self = gameObject
                });
                addSkillShortCuts(KeyCode.Mouse1, new BowSkillAction()
                {
                    skill = SkillCollection.Instance.GetSkill(3),
                    self = gameObject
                });
                Property = LifeEngine.Instance.lifeData.property;
            }
            else
            {
                removeAllShortCuts();
            }
        }

        public void disableAllShortCuts()
        {
            foreach (var battleSkill in skillShortCuts)
            {
                battleSkill.enabled = false;
            }
            // ReSharper disable once Unity.NoNullPropagation
            Interact?.disableInteract();
        }

        public void enableAllShortCuts()
        {
            foreach (var battleSkill in skillShortCuts)
            {
                battleSkill.enabled = true;
            }
            // ReSharper disable once Unity.NoNullPropagation
            Interact?.enableInteract();
        }

        public void removeAllShortCuts()
        {
            foreach (var battleSkill in skillShortCuts)
            {
                Destroy(battleSkill);
            }
            skillShortCuts.Clear();
        }

        public void disableMove()
        {
            Movement.enabled = false;
        }

        public void enableMove()
        {
            Movement.enabled = true;
        }

        public void addSkillShortCuts(KeyCode keyCode, ISkillAction action)
        {
            var self = gameObject;
            var skillControl = self.AddComponent<BattleSkill>();
            skillControl.keycode = keyCode;
            skillControl.action = action;
            skillShortCuts.Add(skillControl);
        }

        public void SetBodyScale(Vector2 bodyScale)
        {
            Animator.SetBodyScale(bodyScale);
        }

        public void onDeath()
        {
            Animator.SetState(CharacterState.DeathF);
            if (character.ID == 0)
            {
                disableAllShortCuts();
                LifeEngine.Instance.GameEnd();
            }
            Destroy(gameObject, 1f);
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
            lookup[characterID] = obj;
            return obj;
        }
        
        public static NPCController GetCharacterController(long characterID)
        {
            return lookup.TryGetValue(characterID, out var controller) ? controller : null;
        }
    }
}