using System;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Model;
using ModelContainer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utils.Shuffle;

public class BattleActionManager : MonoBehaviour, IDropHandler
{
    public static BattleActionManager Instance { get; private set; }
    private ScrollRect scroll;
    private Slider battleEnergySlider;
    private Button turnCompleteBtn;
    private Button battleLogBtn;
    private GameObject cardPrefab;
    private TaskCompletionSource<bool> turnCompletionSource;

    private void Awake()
    {
        Instance = this;
        cardPrefab = Resources.Load<GameObject>("Prefabs/BattleActionCard");
        scroll = transform.Find("Scroll View").GetComponent<ScrollRect>();
        battleEnergySlider = transform.Find("EnergySlider").GetComponent<Slider>();
        turnCompleteBtn = transform.Find("Buttons/Complete").GetComponent<Button>();
        battleLogBtn = transform.Find("Buttons/BattleLog").GetComponent<Button>();

        battleLogBtn.onClick.AddListener(ToggleBattleLog);
    }

    private void ToggleBattleLog()
    {
        if (BattleLogConsole.Instance == null) return;
        if (BattleLogConsole.Instance.gameObject.activeSelf)
        {
            BattleLogConsole.Instance.Hide();
        }
        else
        {
            BattleLogConsole.Instance.Show();
        }
    }

    public async Task UpdateCardActions(BattleCharacter character)
    {
        List<Skill> skills = character.skills;
        try
        {
            for (int i = 0; i < skills.Count; i++)
            {
                BattleSkillCard uiCard;
                if (i < scroll.content.childCount)
                {
                    uiCard = scroll.content.GetChild(i).GetComponent<BattleSkillCard>();
                    uiCard.SetSkill(skills[i], character);
                }
                else
                {
                    uiCard = Instantiate(cardPrefab, scroll.content).GetComponent<BattleSkillCard>();
                    uiCard.SetSkill(skills[i], character);
                }
                uiCard.gameObject.SetActive(true);
                await uiCard.CheckCost();
            }
            for (int i = skills.Count; i < scroll.content.childCount; i++)
            {
                scroll.content.GetChild(i).gameObject.SetActive(false);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }
    }

    public async Task UpdateCardStates()
    {
        for (int i = 0; i < scroll.content.childCount; i++)
        {
            BattleSkillCard uiCard = scroll.content.GetChild(i).GetComponent<BattleSkillCard>();
            if (uiCard != null && uiCard.gameObject.activeSelf)
            {
                await uiCard.CheckCost();
            }
        }
    }

    public async void OnDrop(PointerEventData data)
    {
        BattleSkillCard skillCard = data.pointerDrag.GetComponent<BattleSkillCard>();
        if (skillCard != null && skillCard.isExecutable())
        {
            await skillCard.ExecuteSkillEffect();
        }
        if (BattleManager.Instance.TurnManager.State != BattleState.InProgress)
        {
            turnCompletionSource?.TrySetResult(true);
        }
    }

    public void ShuffleNewSkills(BattleCharacter character, int count)
    {
        List<Skill> currentSkills = character.skills;
        List<Skill> skills = character.character.Skills.Select(SkillCollection.Instance.GetSkill).ToList();
        skills.Shuffle();
        for (int i = 0; i < skills.Count; i++)
        {
            if (count <= 0) break;
            if (currentSkills.Contains(skills[i])) continue;
            character.skills.Add(skills[i]);
            count--;
        }
    }

    public void ResetEnergyState(BattleCharacter character)
    {
        var energyProp = character.Property.GetProperty(SubPropertyType.Energy);
        energyProp.value += 3; // 默认回复速度
    }

    public void UpdateEnergySlider()
    {
        BattleCharacter character = BattleManager.Instance.TurnManager?.CurrentCharacter;
        if (character == null || character.isAI) return;
        var energyProp = character.Property.GetProperty(SubPropertyType.Energy);
        battleEnergySlider.DOValue((float)energyProp.value / energyProp.max, 0.5f);
    }

    public async Task WaitForTurnComplete()
    {
        turnCompletionSource = new TaskCompletionSource<bool>();
        turnCompleteBtn.onClick.RemoveAllListeners();
        turnCompleteBtn.onClick.AddListener(() =>
        {
            turnCompletionSource.TrySetResult(true);
        });
        await turnCompletionSource.Task;
    }

}