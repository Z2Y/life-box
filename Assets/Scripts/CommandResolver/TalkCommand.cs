using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelContainer;
using UI;
using UnityEngine;

[CommandResolverHandler("Talk")]
public class TalkCommand : CommandResolver
{
    public override Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        try
        {
            var characterID = Convert.ToInt64(args[0]);
            var description = Convert.ToString(args[1]);
            var options = args.Skip(2);

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
                choices = dialogueOptions
            };

            DialoguePanel.Show(dialogue);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        return null;
    }
}
