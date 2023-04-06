using DG.Tweening;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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

    public bool isExecutable()
    {
        return skillAction.isExecutable();
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
            BattleBlockManager.Instance.ShowBlocks(new List<BattlePositionBlock>() {selectedBlock});
        }
        BattleBlockManager.Instance.ShowBlocks(selectRange, BattleBlockType.Normal);        
    }

    public async UniTask ExecuteSkillEffect()
    {
        OnEndDrag(null);

        if (await CheckCost())
        {
            Hide();
            skillAction.battleCostResult?.Cost();
            await BattleActionManager.Instance.UpdateCardStates();
            var SkillEnv = new Dictionary<string, object>() { {"Skill" , skillAction}};
            if (await Skill.Effect.ExecuteExpressionAsync(SkillEnv) is BattleEffectResult skillEffect)
            {
                await skillEffect.DoEffect();
                BattleLogConsole.Instance.LogBattleEffect(skillAction, skillEffect);
            }
            Character.skills.Remove(Skill);
            
        }
    }

    public async Task<bool> CheckCost()
    {
        if (skillAction.battleCostResult == null) {
            skillAction.battleCostResult = await Skill.Cost.ExecuteExpressionAsync(new Dictionary<string, object>() { {"Skill" , skillAction}}) as BattleCostResult;
        }
        var available = skillAction.battleCostResult == null || skillAction.battleCostResult.CouldCost();
        cardImage?.DOFade(available ? 1f : 0.5f, 0.5f);
        return available;
    }

    public void SelectBlock()
    {
        Vector3Int cellPos = BattleBlockManager.Instance.tilemap.WorldToCell(transform.position);

        if (skillAction.position == cellPos) return;

        skillAction.position = cellPos;
        skillAction.selectRange = BattleBlockManager.Instance.GetBlocksByRange(Character.Position, Skill.SelectRange, BlockRangeType.Circle);

        List<BattlePositionBlock> blocks = BattleBlockManager.Instance.GetBlocksByRange(cellPos);

        if (blocks.Count <= 0 || !skillAction.selectRange.Intersect(blocks).Any() )
        {
            skillAction.selectResult = null;
            ShowSkillIndicator();
            return;
        }

        BattleCharacter blockCharacter = BattleManager.Instance.TurnManager.GetCharacterByPosition(blocks[0].Position);

        bool isEnemy = blockCharacter != null && blockCharacter.TeamID != Character.TeamID;
        bool isFriend = blockCharacter != null && blockCharacter.TeamID == Character.TeamID;
        bool isEmpty = blockCharacter == null; 

        skillAction.target = blockCharacter;
        skillAction.self = Character;

        switch (Skill.SelectType) {
            case SelectType.EmptyBlock:
                skillAction.selectResult = isEmpty ? blocks[0] : null;
                break;
            case SelectType.EnemyBlock:
                skillAction.selectResult = isEnemy ? blocks[0] : null;
                break;
            case SelectType.FriendBlock:
                skillAction.selectResult = isEnemy ? blocks[0] : null;
                break;
            case SelectType.AnyBlock:
            default:
                skillAction.selectResult = blocks[0];
               break;
        }

        ShowSkillIndicator();
    }
}

public class BattleSkillAction {
    public Skill skill;
    public Vector3Int position;
    public BattlePositionBlock selectResult;
    public List<BattlePositionBlock> selectRange = new ();
    public BattleCharacter self;
    public BattleCharacter target;
    public BattleCostResult battleCostResult;

    public bool isExecutable()
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