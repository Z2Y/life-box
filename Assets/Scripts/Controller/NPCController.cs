using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using ModelContainer;
using UnityEngine;

namespace Controller
{
    public class NPCController : MonoBehaviour
    {
        private static readonly Dictionary<long, NPCController> lookup = new Dictionary<long, NPCController>();
        
        [SerializeField] private long characterID;

        private Character character;
        
        private void Awake()
        {
            character = CharacterCollection.Instance.GetCharacter(characterID);
            lookup.Add(characterID, this);
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
            var loader = Resources.LoadAsync<GameObject>($"Character/{character.ModelID}");
            while (!loader.isDone) {
                await YieldCoroutine.WaitForSeconds(0.005f);
            }

            if (loader.asset == null)
            {
                return null;
            }

            var obj = Instantiate(loader.asset as GameObject, GameObject.Find("CharacterRoot").transform);
            return obj.GetComponent<NPCController>();
        }
        
        private static NPCController GetCharacterController(long characterID)
        {
            return lookup.TryGetValue(characterID, out var controller) ? controller : null;
        }
    }
}