using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleLogConsole : UIBase
{
    private ScrollRect scroller;
    private LifeNode current;
    private GameObject nodePrefab;
    private TMP_Text logText;

    private static BattleLogConsole _instance;

    public static BattleLogConsole Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        scroller = GetComponentInChildren<ScrollRect>();
        logText = scroller.content.GetComponent<TMP_Text>();
        Hide();
    }

    public void ClearLog()
    {
        logText.text = "";
    }

    public void LogBattleEffect(BattleSkillAction action, BattleEffectResult battleEffect)
    {
        if (battleEffect == null || action == null) return;
        foreach (var effect in battleEffect.effects)
        {
            if (effect is BattleMoveEffect)
            {
                logText.text = $"{logText.text}\n【{action.self.character.Name}】移动了位置。";
            }
            else if (action.target != null)
            {
                logText.text = $"{logText.text}\n【{action.self.character.Name}】对 【{action.target.character.Name}】使用【{action.skill.Name}】{effect.EffectDescription()}";
            }
        }
        UpdateScroller();
    }

    public void UpdateScroller()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroller.content);
        scroller.verticalNormalizedPosition = 0f;
    }

}