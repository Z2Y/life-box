using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Cysharp.Threading.Tasks;
using ModelContainer;
using UI;

[CommandResolverHandler("Talk")]
public class TalkCommand : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var characterID = Convert.ToInt64(args[0]);
        var description = Convert.ToString(args[1]);
        var uninterruptible = Convert.ToInt32(args[2]);
        var options = args.Skip(3);

        var character = CharacterCollection.Instance.GetCharacter(characterID);

        var dialogueOptions = new List<DialogueChoice>();

        foreach (var option in options)
        {
            var optionEvent = EventCollection.Instance.GetEvent(Convert.ToInt64(option));
            dialogueOptions.Add(new DialogueChoice()
            {
                text = optionEvent.Description,
                onSelect = () =>
                {
                    optionEvent.Trigger().Coroutine();
                }
            });
        }

        var dialogue = new DialogueLine()
        {
            speakerName = character.Name,
            text = description,
            choices = dialogueOptions,
            uninterruptible = uninterruptible == 1
        };

        var originCameraFollowTarget = WorldCameraController.Instance.GetFollowTarget();

        if (dialogue.uninterruptible)
        {
            LifeEngine.Instance.MainCharacter.disableMove();
            LifeEngine.Instance.MainCharacter.disableAllShortCuts();
        }

        WorldCameraController.Instance.FollowTo(NPCController.GetCharacterController(character.ID)?.gameObject).Coroutine();

        await DialoguePanel.Show(dialogue);

        if (dialogue.uninterruptible)
        {
            await YieldCoroutine.WaitForSeconds(0.5f);
        }
        
        if (dialogue.uninterruptible)
        {
            LifeEngine.Instance.MainCharacter.enableMove();
            LifeEngine.Instance.MainCharacter.enableAllShortCuts();
        }
        
        WorldCameraController.Instance.FollowTo(originCameraFollowTarget).Coroutine();

        return await this.Done();
    }
}
