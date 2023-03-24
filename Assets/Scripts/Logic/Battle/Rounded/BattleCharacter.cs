using DG.Tweening;
using System.Collections.Generic;
using Model;
using ModelContainer;
using UnityEngine;

public class BattleCharacter {
    public Character character {get; private set;}

    public LifeProperty Property {get; private set;}

    public BattleCharacterView View {get; private set;}

    public readonly List<Skill> skills = new ();

    public int ActedTurn {get; set;}

    public int TeamID {get; private set;}

    public bool isAI {get; private set;}

    public Vector3Int Position {get; set;}

    public BattleCharacter(long characterID, int team = 0, bool ai = true) {
        character = CharacterCollection.Instance.GetCharacter(characterID);
        Property = LifePropertyFactory.Random(40);
        ActedTurn = -1;
        TeamID = team;
        isAI = ai;
        Property.onPropertyChange?.AddListener(onHpChange);
    }

    public bool isAlive => Hp.value > 0;

    public int Speed => Property.GetProperty(SubPropertyType.Agile).value;

    public PropertyValue Hp => Property.GetProperty(SubPropertyType.HitPoint);

    public void UpdateHpSlider()
    {
        View.HpSlider.DOValue((float)Hp.value / Hp.max, 0.5f);
    }

    public void CreateView(Vector3 worldPos)
    {
        View = BattleCharacterView.CreateNew();
        View.transform.position = worldPos;

    }

    private void onHpChange()
    {
        Debug.Log($"{character.ID} {isAlive}");
        if (!isAlive) {
            View.Hide();
        }
    }
}