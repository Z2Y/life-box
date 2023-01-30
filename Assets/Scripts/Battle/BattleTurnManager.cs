using System;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public enum BattleState
{
    Win,
    Lose,
    InProgress,
    Cancled
}

public class BattleTurnManager
{
    public List<BattleCharacter> AllRoles = new List<BattleCharacter>();
    public int CurrentTurn;
    public BattleCharacter CurrentCharacter { get; private set; }

    public BattleState State
    {
        get
        {
            var teams = AllRoles.GroupBy(role => role.TeamID).ToDictionary((group) => group.Key, (group) => group.ToList());
            int alivedTeam = 0;
            int alivedTeamID = 0;
            foreach (var team in teams)
            {
                bool isAlive = team.Value.Count(role => role.isAlive) > 0;
                alivedTeam += isAlive ? 1 : 0;
                alivedTeamID = isAlive ? team.Key : alivedTeamID;
            }
            return alivedTeam > 1 ? BattleState.InProgress : (alivedTeamID == 0 ? BattleState.Win : BattleState.Lose);
        }
    }

    public void InitFirstTurn()
    {
        Sort();
    }

    public BattleCharacter GetNextCharacter()
    {
        return AllRoles.Where(role => role.isAlive && role.ActedTurn < CurrentTurn).FirstOrDefault();
    }

    public BattleCharacter GetCharacterByPosition(Vector3Int pos)
    {
        return AllRoles.Where(role => role.isAlive && role.Position == pos).FirstOrDefault();
    }

    public void AddBattleCharacter(BattleCharacter character)
    {
        if (!AllRoles.Contains(character)) AllRoles.Add(character);
    }

    public async Task OnBeforeAction(BattleCharacter character)
    {
        CurrentCharacter = character;
        BattleActionManager.Instance.ResetEnergyState(character);
        BattleActionManager.Instance.ShuffleNewSkills(character, 2);
        if (!character.isAI) {
            BattleActionManager.Instance.UpdateEnergySlider();
            await BattleActionManager.Instance.UpdateCardActions(character);
            character.Property.onPropertyChange.AddListener(BattleActionManager.Instance.UpdateEnergySlider);
        } else {
            BattleAIManager.Instance.BindCharacter(character);
        }
    }

    public async Task OnAction(BattleCharacter character)
    {
        if (character.isAI)
        {
            await BattleAIManager.Instance.WaitForTurnComplete();
        }
        else
        {
            await BattleActionManager.Instance.WaitForTurnComplete();
        }
    }

    public void OnAfterAction(BattleCharacter character)
    {
        if (character.skills.Count > 6)
        {
            character.skills.RemoveRange(6, character.skills.Count - 6);
        }
        character.ActedTurn = CurrentTurn;
        if (!character.isAI)
        {
            character.Property.onPropertyChange.RemoveListener(BattleActionManager.Instance.UpdateEnergySlider);
        }
        CurrentCharacter = null;
    }

    public void OnBeforeTurnEnd()
    {

    }

    public void OnTurnEnd()
    {
        CurrentTurn += 1;
        Sort();
    }

    private void Sort()
    {
        AllRoles.Sort((characterA, characterB) =>
        {
            return characterA.Speed.CompareTo(characterB.Speed);
        });
    }
}