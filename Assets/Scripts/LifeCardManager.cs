using Model;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ModelContainer;
using System.Collections.Generic;

public class LifeCardManager : MonoBehaviour
{
    private ScrollRect scroll;
    private GameObject cardPrefab;
    private static LifeCardManager _instance;
    public static LifeCardManager Instance => _instance;

    public LifeNode CurrentLife => LifeEngine.Instance.lifeData.current;

    private void Awake()
    {
        _instance = this;
        cardPrefab = Resources.Load<GameObject>("Prefabs/LifeActionCard");
        scroll = transform.Find("Scroll View").GetComponent<ScrollRect>();
    }

    void OnDisable() {
        if (LifeEngine.Instance != null) {
            LifeEngine.Instance.AfterLifeChange -= UpdateCardActions;
        }
    }

    private void Start() {
        ListenLifeChange();    
    }

    private void ListenLifeChange() {
        LifeEngine.Instance.AfterLifeChange += UpdateCardActions;
    }

    public void UpdateCardActions()
    {
        Place place = CurrentLife?.Place;
        IEnumerable<Command> globalCommands = CommandCollection.GetValidGlobalCommands();
        List<Command> commands;
        if (place != null) {
            commands = CommandCollection.GetValidCommands(place.Commands).Concat(globalCommands).ToList();
        } else {
            commands = globalCommands.ToList();
        }
        for (int i = 0; i < commands.Count; i++)
        {
            LifeActionCard uiCard;
            if (i < scroll.content.childCount)
            {
                uiCard = scroll.content.GetChild(i).GetComponent<LifeActionCard>();
                uiCard.SetCommand(commands[i]);
            }
            else
            {
                uiCard = Instantiate(cardPrefab, scroll.content).GetComponent<LifeActionCard>();
                uiCard.SetCommand(commands[i]);
            }
            uiCard.gameObject.SetActive(true);
        }
        for (int i = commands.Count; i < scroll.content.childCount; i++)
        {
            scroll.content.GetChild(i).gameObject.SetActive(false);
        }
    }
}