using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using MessagePack;
using System.Collections;
using System.Collections.Generic;

public class ExpressionExcutor : MonoBehaviour
{
    public static ExpressionExcutor Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public static async Task<object> Execute(string command, string data, List<object> listData, Dictionary<string, object> env)
    {
        ICommandResolver resolver = ExpressionCommandResolver.GetResolver(command);
        if (resolver != null)
        {
            return await resolver.Resolve(data, listData, env);
        }
        return null;
    }

    public static bool Compare(string field, string op, int value)
    {
        int fieldValue;

        if (field.StartsWith("#")) {
            object resolved = ExpressionFieldResolver.Resolve(field.Substring(1));
            if (resolved == null) { return false; }
            fieldValue = Convert.ToInt32(resolved);
        } else {
            if (!int.TryParse(field, out fieldValue)) {
                return false;
            }
        }
        switch (op)
        {
            case ">":
                return fieldValue > value;
            case "<":
                return fieldValue < value;
            case ">=":
                return fieldValue >= value;
            case "<=":
                return fieldValue <= value;
            case "==":
                return fieldValue == value;
            case "%":
                return fieldValue % value == 0;
            default:
                return false;
        }
    }

    public bool Contains(string name, List<object> data)
    {
        var container = name.ToUpper();
        var resolved = ExpressionFieldResolver.Resolve(name.StartsWith("#") ? name.Substring(1) : name);
        if (resolved == null) return false;

        try
        {
            if (resolved is IDictionary dictContainer)
            {
                var dictType = dictContainer.GetType();
                var keyType = dictType.IsGenericType ? dictType.GetGenericArguments()[0] : typeof(object);

                foreach (var item in data)
                {
                    if (dictContainer.Contains(Convert.ChangeType(item, keyType)))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (resolved is IList listContainer)
            {
                var listType = listContainer.GetType();
                var keyType = listType.IsGenericType ? listType.GetGenericArguments()[0] : typeof(object);

                foreach (var item in data)
                {
                    if (listContainer.Contains(Convert.ChangeType(item, keyType)))
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (var item in data)
            {
                var converted = Convert.ChangeType(item, resolved.GetType());
                if (resolved.Equals(converted))
                {
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

        return false;
    }
}

public class ExpressionNode
{
    public string expression;
    public List<ExpressionNode> nodes = new List<ExpressionNode>();
    public Dictionary<string, object> environments = new Dictionary<string, object>();

    public void SetEnv(string key, object value)
    {
        if (environments.ContainsKey(key))
        {
            environments[key] = value;
        }
        else
        {
            environments.Add(key, value);
        }
    }

    public ExpressionNode()
    {
        expression = null;
    }
    public ExpressionNode(string exp)
    {
        expression = exp;
    }

    public ExpressionNode(string exp, Dictionary<string, object> environments)
    {
        expression = exp;
        if (environments != null)
        {
            foreach (var item in environments)
            {
                SetEnv(item.Key, item.Value);
            }
        }
    }

    public string InjectEnv()
    {
        var matches = Regex.Matches(expression, @"\{(\$\w+)\}");
        for (var i = 0; i < matches.Count; i++)
        {
            var valueKey = matches[i].Groups[1].Value;
            if (environments.TryGetValue(valueKey, out var envValue) && envValue != null)
            {
                expression = expression.Replace(matches[i].Value, envValue.ToString());
            }
        }
        return expression;
    }

    public async Task<object> ExecuteExpressionAsync()
    {
        try
        {
            var result = ExecuteExpression();
            if (result is Task<object>)
            {
                result = await (result as Task<object>);
            }
            return result;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
        return null;
    }

    public object ExecuteExpression()
    {
        string injectedExpression = InjectEnv();
        var match = Regex.Match(injectedExpression, @"[><\!\?\@=]");
        string command = injectedExpression.Substring(0, match.Index);
        string op = injectedExpression.Substring(match.Index, (match.Index + 1) < injectedExpression.Length && injectedExpression[match.Index + 1] == '=' ? 2 : 1);
        string data = injectedExpression.Substring(match.Index + op.Length);
        List<object> listData = new List<object>();
        if (data.Length > 0 && data[0] == '[')
        {
            try
            {
                listData = MessagePackSerializer.Deserialize<object[]>(MessagePackSerializer.ConvertFromJson(data)).ToList();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
        }

        // UnityEngine.Debug.Log($"{command} {op} {data} {listData.Count}");

        switch (op)
        {
            case "@":
                return ExpressionExcutor.Execute(command, data, listData, environments);
            case ">":
            case ">=":
            case "<":
            case "%":
            case "<=":
            case "==":
                if (int.TryParse(data, out var rightValue))
                {
                    return ExpressionExcutor.Compare(command, op, rightValue);
                }
                break;
            case "?":
                if (data[0] == '[')
                {
                    return ExpressionExcutor.Instance.Contains(command, listData);
                }
                break;
            default:
                break;
        }
        return null;
    }

    public async Task<object> ExecuteAsync()
    {
        if (expression == null && nodes.Count == 0)
        {
            return null;
        }

        if (nodes.Count == 0 && expression.Length > 0)
        {
            return await ExecuteExpressionAsync();
        }

        int resultIndex = 0;
        ExecuteResult excuteResult = new ExecuteResult(null);
        string op = "&";

        for (int i = 0; i < nodes.Count; i++)
        {
            ExpressionNode subNode = nodes[i];
            switch (subNode.expression)
            {
                case "&":
                case "|":
                    op = subNode.expression;
                    break;
                default:
                    subNode.environments = new Dictionary<string, object>(environments);
                    object value = await subNode.ExecuteAsync();
                    subNode.environments.Clear();
                    if (op == "|")
                    {
                        excuteResult = excuteResult | new ExecuteResult(value);
                    }
                    else
                    {
                        excuteResult = excuteResult & new ExecuteResult(value);
                    }
                    SetEnv($"${resultIndex++}", value);
                    break;
            }
        }

        // Debug.Log($"Expression Result: {excuteResult.value}");

        return excuteResult.value;
    }

    public object Execute()
    {
        if (expression == null && nodes.Count == 0)
        {
            return null;
        }

        if (nodes.Count == 0 && expression.Length > 0)
        {
            return ExecuteExpression();
        }

        int resultIndex = 0;
        ExecuteResult excuteResult = new ExecuteResult(null);
        string op = "&";

        for (int i = 0; i < nodes.Count; i++)
        {
            ExpressionNode subNode = nodes[i];
            switch (subNode.expression)
            {
                case "&":
                case "|":
                    op = subNode.expression;
                    break;
                default:
                    subNode.environments = new Dictionary<string, object>(environments);
                    ExecuteResult reuslt = new ExecuteResult(subNode.Execute());
                    subNode.environments.Clear();
                    if (op == "|")
                    {
                        excuteResult = excuteResult | reuslt;
                    }
                    else
                    {
                        excuteResult = excuteResult & reuslt;
                    }
                    SetEnv($"${resultIndex++}", reuslt.value);
                    break;
            }
        }

        Debug.Log($"Expression Result: {excuteResult.value}");

        return excuteResult.value;
    }

    public static ExpressionNode ParseExpression(string raw)
    {
        // Only Parse Complex Expression Start with "("
        if (!raw.StartsWith("(")) {
            return new ExpressionNode(raw);
        }
        int cursor = 0;
        Stack<ExpressionNode> stack = new Stack<ExpressionNode>();
        stack.Push(new ExpressionNode());
        Action<int> GetExpression = (int idx) =>
        {
            string sub = raw.Substring(cursor, idx - cursor).Trim();
            cursor = idx;
            if (sub.Length > 0)
            {
                stack.Peek().nodes.Add(new ExpressionNode(sub));
            }
        };

        for (int i = 0; i < raw.Length; i++)
        {
            switch (raw[i])
            {
                case ' ':
                    continue;
                case '(':
                    GetExpression(i);
                    cursor++;
                    var subNode = new ExpressionNode();
                    stack.Peek().nodes.Add(subNode);
                    stack.Push(subNode);
                    break;
                case ')':
                    GetExpression(i);
                    cursor++;
                    stack.Pop();
                    break;
                case '|':
                case '&':
                    GetExpression(i);
                    GetExpression(i + 1);
                    break;
                default:
                    continue;
            }
        }

        GetExpression(raw.Length);

        return stack.Pop();
    }
}

public class ExecuteResult
{
    public object value;
    public ExecuteResult(object value)
    {
        if (value is Task<object> taskResult)
        {
            if (taskResult.IsCompleted)
            {
                this.value = taskResult.Result;
            }
            else
            {
                Debug.LogWarning("ExecuteResult Task Not Completed!");
                this.value = taskResult;
            }
        }
        else
        {
            this.value = value;
        }
    }
    public static ExecuteResult operator |(ExecuteResult left, ExecuteResult right)
    {
        bool? l = left.value as bool?;
        bool? r = right.value as bool?;
        if (l == null || r == null)
        {
            return right;
        }
        return new ExecuteResult((bool)l | (bool)r);
    }

    public static ExecuteResult operator &(ExecuteResult left, ExecuteResult right)
    {
        bool? l = left.value as bool?;
        bool? r = right.value as bool?;
        if (l == null || r == null)
        {
            return right;
        }
        return new ExecuteResult((bool)l & (bool)r);
    }
}

public static class ExpressionHelper
{

    public static string InjectedExpression(this string expression, Dictionary<string, object> environments = null)
    {
        if (expression.Length <= 0) { return ""; }
        var node = new ExpressionNode(expression);
        if (environments == null) return node.InjectEnv();
        foreach (var item in environments)
        {
            node.SetEnv(item.Key, item.Value);
        }
        return node.InjectEnv();
    }
    public static object ExecuteExpression(this string expression, Dictionary<string, object> environments = null)
    {
        if (expression.Length <= 0) { return null; }
        var node = ExpressionNode.ParseExpression(expression);
        if (environments == null) return node.Execute();
        foreach (var item in environments)
        {
            node.SetEnv(item.Key, item.Value);
        }
        return node.Execute();
    }

    public static async Task<object> ExecuteExpressionAsync(this string expression, Dictionary<string, object> environments = null) {
        if (expression.Length <= 0) { return null; }
        var node = ExpressionNode.ParseExpression(expression);
        if (environments == null) return await node.ExecuteAsync();
        foreach (var item in environments)
        {
            node.SetEnv(item.Key, item.Value);
        }
        return await node.ExecuteAsync();        
    }
}
