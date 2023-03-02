using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using ModelContainer;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Controller
{
    public class NPCController : MonoBehaviour
    {
        private static readonly Dictionary<long, NPCController> lookup = new ();
        
        [SerializeField] private long characterID;

        public Character character { get; private set; }
        private CollisionDetector collisionDetector;
        public NPCAnimationController Animator { get; private set; }
        public NPCMovementController Movement { get; private set;  }
        public NPCAttackController Attack { get; private set;  }

        private void Awake()
        {
            character = CharacterCollection.Instance.GetCharacter(characterID);
            Animator = gameObject.AddComponent<NPCAnimationController>();
            Movement = gameObject.AddComponent<NPCMovementController>();
            Attack = gameObject.AddComponent<NPCAttackController>();
            collisionDetector = gameObject.AddComponent<CollisionDetector>();
            collisionDetector.enabled = false;
            lookup.Add(characterID, this);
        }

        public void SetLocation(Location location)
        {
            // do map position translate
            transform.position = location.Position;
        }

        public void SetAsPlayer(bool isPlayer)
        {
            Movement.SetAsPlayer(isPlayer);
            Attack.SetAsPlayer(isPlayer);
            collisionDetector.enabled = isPlayer;
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
            
            Debug.Log(character.ModelID);
            Debug.Log(loader.asset);

            if (loader.asset == null)
            {
                return null;
            }

            var obj = Instantiate(loader.asset as GameObject, GameObject.Find("CharacterRoot").transform);
            return obj.GetComponent<NPCController>();
        }
        
        public static NPCController GetCharacterController(long characterID)
        {
            return lookup.TryGetValue(characterID, out var controller) ? controller : null;
        }
    }
}