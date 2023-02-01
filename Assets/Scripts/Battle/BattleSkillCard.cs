using System;
using DG.Tweening;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleSkillCard : UIBase, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public BattleCharacter Character {get; private set;}
    public Skill Skill { get; private set; }
    private Text nameText;
    private Text descriptionText;
    private Image cardImage;
    private Transform originParent;
    private Vector3 originPosition;
    private int originIndex;
    private Vector3 dragOffset;
    private BattleSkillAction skillAction;

    private void Awake()
    {
        nameText = transform.Find("Name").GetComponent<Text>();
        descriptionText = transform.Find("Description").GetComponent<Text>();
        cardImage = GetComponent<Image>();
        skillAction = new BattleSkillAction();
        originParent = transform.parent;
    }

    public void SetSkill(Skill skill, BattleCharacter character)
    {
        Skill = skill;
        Character = character;
        nameText.text = skill.Name;
        skillAction.skill = skill;
        skillAction.self = character;
        skillAction.Reset();
    }

    public bool isExecuteable()
    {
        return skillAction.isExecuteable();
    }

    public void OnBeginDrag(PointerEventData data)
    {
        originPosition = transform.position;
        originIndex = transform.GetSiblingIndex();
        dragOffset = Camera.main.ScreenToWorldPoint(data.position) - originPosition;
        dragOffset.z = 0;
        transform.SetParent(BattleActionManager.Instance.transform);

        skillAction.Reset();
    }

    public void OnEndDrag(PointerEventData data)
    {
        transform.SetParent(originParent);
        transform.SetSiblingIndex(originIndex);
        transform.position = originPosition;
        BattleBlockManager.Instance.HideAllBlocks();
    }

    public void OnDrag(PointerEventData data)
    {
        Vector3 dragPosition = Camera.main.ScreenToWorldPoint(data.position);
        dragPosition.z = transform.position.z;
        transform.position = dragPosition - dragOffset;

        SelectBlock();
    }

    public void ShowSkillIndicator() {
        var selectedBlock = skillAction.selectResult;
        var selectRange = skillAction.selectRange;

        BattleBlockManager.Instance.HideAllBlocks();
        if (selectedBlock != null) {
            selectRange.Remove(selectedBlock);
            BattleBlockManager.Instance.ShowBlocks(new List<BattlePositonBlock>() {selectedBlock});
        }
        BattleBlockManager.Instance.ShowBlocks(selectRange, BattleBlockType.Normal);        
    }

    public async Task ExecuteSkillEffect()
    {
        OnEndDrag(null);

        if (await CheckCost())
        {
            Hide();
            skillAction.battleCostResult?.Cost();
            await BattleActionManager.Instance.UpdateCardStates();
            var SkillEnv = new Dictionary<string, object>() { {"Skill" , skillAction}};
            var skillEffect = await Skill.Effect.ExecuteExpressionAsync(SkillEnv) as BattleEffectResult;
            await skillEffect?.DoEffect();
            Character.skills.Remove(Skill);
            BattleLogConsole.Instance.LogBattleEffect(skillAction, skillEffect);
        }
    }

    public async Task<bool> CheckCost()
    {
        if (skillAction.battleCostResult == null) {
            skillAction.battleCostResult = await Skill.Cost.ExecuteExpressionAsync(new Dictionary<string, object>() { {"Skill" , skillAction}}) as BattleCostResult;
        }
        bool costable = skillAction.battleCostResult == null || skillAction.battleCostResult.CouldCost();
        cardImage?.DOFade(costable ? 1f : 0.5f, 0.5f);
        return costable;
    }

    public void SelectBlock()
    {
        Vector3Int cellPos = BattleBlockManager.Instance.tilemap.WorldToCell(transform.position);

        if (skillAction.position == cellPos) return;

        skillAction.position = cellPos;
        skillAction.selectRange = BattleBlockManager.Instance.GetBlocksByRange(Character.Position, Skill.SelectRange, BlockRangeType.Circle);

        List<BattlePositonBlock> blocks = BattleBlockManager.Instance.GetBlocksByRange(cellPos);

        if (blocks.Count <= 0 || skillAction.selectRange.Intersect(blocks).Count() <= 0 )
        {
            skillAction.selectResult = null;
            ShowSkillIndicator();
            return;
        }

        BattleCharacter blockCharacter = BattleManager.Instance.TurnManager.GetCharacterByPosition(blocks.First().Position);

        bool isEnemy = blockCharacter != null && blockCharacter.TeamID != this.Character.TeamID;
        bool isFriend = blockCharacter != null && blockCharacter.TeamID == this.Character.TeamID;
        bool isEmpty = blockCharacter == null; 

        skillAction.target = blockCharacter;
        skillAction.self = this.Character;

        switch (Skill.SelectType) {
            case SelectType.EmptyBlock:
                skillAction.selectResult = isEmpty ? blocks.First() : null;
                break;
            case SelectType.EnemyBlock:
                skillAction.selectResult = isEnemy ? blocks.First() : null;
                break;
            case SelectType.FriendBlock:
                skillAction.selectResult = isEnemy ? blocks.First() : null;
                break;
            case SelectType.AnyBlock:
            default:
                skillAction.selectResult = blocks.First();
               break;
        }

        ShowSkillIndicator();
    }
}

public class BattleSkillAction {
    public Skill skill;
    public Vector3Int position;
    public BattlePositonBlock selectResult;
    public List<BattlePositonBlock> selectRange = new List<BattlePositonBlock>();
    public BattleCharacter self;
    public BattleCharacter target;
    public BattleCostResult battleCostResult;

    public bool isExecuteable()
    {
        return skill != null && selectResult != null;
    }

    public void Reset() {
        selectResult = null;
        position = new Vector3Int(int.MinValue, int.MinValue, 0);
        selectRange.Clear();
        target = null;
        battleCostResult = null;
    }
}