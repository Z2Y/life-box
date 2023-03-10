using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Controller;
using Logic.Detector;
using Model;
using ModelContainer;
using UI;
using UnityEngine;

namespace Interact
{
    [CommandResolverHandler("InteractToNPC")]
    public class NPCInteractHandler : CommandResolver
    {
        public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
        {
            var activeDetectors = env["activeDetectors"] as HashSet<KeyValuePair<IDetector, Collider2D>>;

            var npcObjects = new HashSet<GameObject>();
            var npcDetectors = new HashSet<IDetector>();
            
            foreach (var pair in activeDetectors!)
            {
                if (pair.Key is TalkableNPCDetector)
                {
                    npcObjects.Add(pair.Value.gameObject);
                    npcDetectors.Add(pair.Key);
                }

                if (pair.Key is ShopableNPCDetector)
                {
                    npcObjects.Add(pair.Value.gameObject);
                    npcDetectors.Add(pair.Key);
                }
            }

            if (npcObjects.Count <= 0) return null;

            if (npcObjects.Count == 1)
            {
                buildDialog(npcObjects.First().GetComponent<NPCController>().character, npcDetectors);
            }
            else
            {
                var characters = npcObjects.Select((npc) => npc.GetComponent<NPCController>()).ToList();
                var names = characters.Select((npc) => npc.character.Name).ToList();
                var selector = await SelectPanel.Show("选择想要交互的人物", names, (idx) =>
                {
                    var detectors = activeDetectors.Where((pair) => pair.Value.gameObject == characters[idx].gameObject)
                        .Select((pair) => pair.Key);
                    buildDialog(characters[idx].character, new HashSet<IDetector>(detectors));
                });
                selector.SetCancelable(true);
            }

            return null;
        }

        private void onCharacterLeave(Character character)
        {   
            Debug.Log("Character leave");
            var dialoguePanel = UIManager.Instance.FindByType<DialoguePanel>();
            if (dialoguePanel is DialoguePanel panel && panel.CurrentDialogue.speakerName == character.Name)
            {
                UIManager.Instance.Hide(dialoguePanel.GetInstanceID());
            }
            
        }

        private void buildDialog(Character character, IEnumerable<IDetector> npcDetectors)
        {

            var choices = new List<DialogueChoice>();
            foreach (var detector in npcDetectors)
            {
                if (detector is TalkableNPCDetector)
                {
                    choices.Add(new DialogueChoice()
                    {
                        text = "随便聊聊",
                        onSelect = () =>
                        {
                            TalkTriggerContainer.Instance.GetTrigger(character.ID).Trigger().Coroutine();
                        }
                    });
                    detector.onEndDetect((_, __) => onCharacterLeave(character));
                }

                if (detector is ShopableNPCDetector)
                {
                    choices.Add(new DialogueChoice()
                    {
                        text = "购买",
                        onSelect = () => {}
                    });
                }
            }

            if (choices.Count == 1)
            {
                // choices[0].onSelect();
                // return;
            }
            
            var dialogue = new DialogueLine()
            {
                speakerName = character.Name,
                text = "少侠 找我有什么事呢?",
                choices = choices
            };
            
            DialoguePanel.Show(dialogue);
        }
    }
    

    [CommandResolverHandler("StopInteractToNPC")]
    public class StopNPCInteractHandler : CommandResolver
    {
        public override Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
        {
            var dialogID = UIManager.Instance.FindByType<DialoguePanel>()?.GetInstanceID() ?? 0;
            if (dialogID != 0)
            {
                UIManager.Instance.Hide(dialogID);
            }

            return null;
        }
    }
}