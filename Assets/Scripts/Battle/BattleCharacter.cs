using System;
using DG.Tweening;
using System.Collections.Generic;
using Model;
using ModelContainer;
using UnityEngine;

public class BattleCharacter {
    public Character character {get; private set;}

    public LifeProperty Property {get; private set;}

    public BattleCharacterView View {get; private set;}

    public List<Skill> skills = new List<Skill>();

    public int ActedTurn {get; set;}

    public int TeamID {get; private set;}

    public bool isAI {get; private set;}

    public Vector3Int Position {get; set;}

    public BattleCharacter(long characterID, int team = 0, bool AI = true) {
        character = CharacterCollection.Instance.GetCharacter(characterID);
        Property = LifePropertyFactory.Ramdom(40);
        ActedTurn = -1;
        TeamID = team;
        isAI = AI;
        Property.onPropertyChange?.AddListener(ListenHpChange);
    }

    public bool isAlive {
        get {
            return Hp.value > 0;
        }
    }

    public int Speed {
        get {
            return Property.GetProperty(SubPropertyType.Agile).value;
        }
    }

    public PropertyValue Hp {
        get {
            return Property.GetProperty(SubPropertyType.HitPoint);
        }
    }

    public void UpdateHpSlider()
    {
        View.HpSlider.DOValue((float)Hp.value / Hp.max, 0.5f);
    }

    public void CreateView(Vector3 worldPos)
    {
        View = BattleCharacterView.CreateNew();
        View.transform.position = worldPos;

    }

    private void ListenHpChange()
    {
        UnityEngine.Debug.Log($"{character.ID} {isAlive}");
        if (!isAlive) {
            View.Hide();
        }
    }
}