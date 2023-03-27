using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Event = Model.Event;

public class EventNode
{
    public LifeNode Life { get; private set; }
    public Event Event { get; private set; }
    public object EffectResult { get;  private set; }
    public EventNode(LifeNode life, Event e)
    {
        Life = life;
        Event = e;
    }

    public void SetEffectResult(object result) {
        if (EffectResult != result) {
            EffectResult = result;
            Life?.OnLifeNodeChange?.Invoke();
        }
    }

    public async UniTask<object> DoEffect()
    {
        if (Event.Effect.Length > 0)
        {
            var node = new ExpressionNode(Event.Effect, Life.Environment);
            node.SetEnv("$description", Event.Description);
            EffectResult = await Event.Effect.ExecuteExpressionAsync(node.environments);
        }
        return EffectResult;
    }

    public async UniTask<int> DoBranch()
    {
        if (Event.BranchExpression.Length > 0 && Event.Branch.Length > 0)
        {
            // UnityEngine.Debug.Log(Event.BranchExpression);
            var node = new ExpressionNode(Event.BranchExpression, Life.Environment);
            node.SetEnv("$description", Description);
            node.SetEnv("$Effect", EffectResult);
            var result = await Event.BranchExpression.ExecuteExpressionAsync(node.environments);
            if (result != null)
            {
                return Convert.ToInt32(result);
            }
        }
        return -1;
    }

    public string Description
    {
        get
        {
            var environments = Life.Environment;
            environments["$Effect"] = EffectResult;
            return Event.Description.InjectedExpression(environments);
        }
    }
}