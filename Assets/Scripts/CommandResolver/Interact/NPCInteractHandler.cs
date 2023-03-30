using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Controller;
using Cysharp.Threading.Tasks;
using Logic.Detector;
using Model;
using ModelContainer;
using NPBehave;
using UI;
using UnityEngine;

namespace Interact
{
    [CommandResolverHandler("InteractToNPC")]
    public class NPCInteractHandler : CommandResolver
    {
        public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
        {
            var activeDetectors = env["activeDetectors"] as HashSet<KeyValuePair<IDetector, Collider2D>>;
            var self = LifeEngine.Instance.MainCharacter;

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
                self!.disableAllShortCuts();
                var selector = await SelectPanel.Show("选择想要交互的人物", names, (idx) =>
                {
                    var detectors = activeDetectors.Where((pair) => pair.Value.gameObject == characters[idx].gameObject)
                        .Select((pair) => pair.Key);
                    buildDialog(characters[idx].character, new HashSet<IDetector>(detectors));
                });
                selector.SetCancelable(true, onCancel: () =>
                {
                    self.enableAllShortCuts();
                });
            }

            return null;
        }

        private void stopTalk(IDetector detector, Collider2D collider)
        {
            detector.offEndDetect(stopTalk);
            LifeEngine.Instance.MainCharacter.enableAllShortCuts();
            var npcController = collider.gameObject.GetComponent<NPCController>();
            if (ReferenceEquals(npcController, null)) return;
            var character = npcController.character;
            // Debug.Log("Character leave");
            var panel = UIManager.Instance.FindByType<DialoguePanel>();
            if (panel != null && panel.CurrentDialogue.speakerName == character.Name)
            {
                UIManager.Instance.Hide(panel.GetInstanceID());
            }
        }
        
        void onLeave()
        {
            LifeEngine.Instance.MainCharacter.enableMove();
            LifeEngine.Instance.MainCharacter.enableAllShortCuts();
        }

        private void buildDialog(Character character, IEnumerable<IDetector> npcDetectors)
        {

            var choices = new List<DialogueChoice>();
            foreach (var detector in npcDetectors)
            {
                if (detector is TalkableNPCDetector)
                {
                    var talks = TalkTriggerContainer.GetTalkConfig(character.ID).GetTalks();
                    foreach (var talkEvent in talks)
                    {
                        choices.Add(new DialogueChoice()
                        {
                            text = talkEvent.Description,
                            onSelect = () =>
                            {
                                talkEvent.Trigger().Coroutine();
                            }
                        });
                    }
                    detector.onEndDetect(stopTalk);
                }

                if (detector is ShopableNPCDetector)
                {
                    var shops = ShopConfigCollection.GetShopConfigsByCharacter(character.ID);
                    foreach (var shopConfig in shops)
                    {
                        async void OnSelect()
                        {
                            UIManager.Instance.FindByType<DialoguePanel>()?.Hide();
                            LifeEngine.Instance.MainCharacter.disableMove();
                            var panel = await ShopPanel.Show(shopConfig, (result) =>
                            {
                                onLeave();
                                Debug.Log(result);
                            });
                            if (ReferenceEquals(panel, null))
                            {
                                onLeave();
                            }
                            else
                            {
                                panel.SetCancelable(true, onLeave);
                            }
                        }

                        choices.Add(new DialogueChoice()
                        {
                            text = $"购买 {shopConfig.Name}",
                            onSelect = OnSelect
                        });
                    }
                }
            }
            
            LifeEngine.Instance.MainCharacter.disableAllShortCuts();
            
            var dialogue = new DialogueLine()
            {
                speakerName = character.Name,
                text = choices.Count == 0 ? "少侠，我们不熟，快快离去！" : "少侠 找我有什么事呢?",
                choices = choices,
                onCancel = onLeave
            };

            
            DialoguePanel.Show(dialogue).Coroutine();
        }
    }
    

    [CommandResolverHandler("StopInteractToNPC")]
    public class StopNPCInteractHandler : CommandResolver
    {
        public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
        {
            var dialog = UIManager.Instance.FindByType<DialoguePanel>();
            if (dialog is { gameObject: { activeSelf: true } })
            {
                dialog.CurrentDialogue.onCancel?.Invoke();
                dialog.Hide();
            }
            
            return await this.Done();;
        }
    }
}