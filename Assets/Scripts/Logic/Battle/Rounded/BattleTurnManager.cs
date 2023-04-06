using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Cathei.LinqGen;

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
            var teams = AllRoles.Gen().GroupBy(role => role.TeamID);
            var aliveTeam = 0;
            var aliveTeamID = 0;
            foreach (var team in teams)
            {
                var isAlive = team.Gen().Count(role => role.isAlive) > 0;
                aliveTeam += isAlive ? 1 : 0;
                aliveTeamID = isAlive ? team.Key : aliveTeamID;
            }
            return aliveTeam > 1 ? BattleState.InProgress : (aliveTeamID == 0 ? BattleState.Win : BattleState.Lose);
        }
    }

    public void InitFirstTurn()
    {
        Sort();
    }

    public BattleCharacter GetNextCharacter()
    {
        return AllRoles.Gen().Where(role => role.isAlive && role.ActedTurn < CurrentTurn).FirstOrDefault();
    }

    public BattleCharacter GetCharacterByPosition(Vector3Int pos)
    {
        return AllRoles.Gen().Where(role => role.isAlive && role.Position == pos).FirstOrDefault();
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
        AllRoles.Sort((characterA, characterB) => characterA.Speed.CompareTo(characterB.Speed));
    }
}