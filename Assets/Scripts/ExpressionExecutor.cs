using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using MessagePack;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ExpressionExecutor : MonoBehaviour
{
    public static ExpressionExecutor Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public static async UniTask<object> Execute(string command, string data, List<object> listData, Dictionary<string, object> env)
    {
        return await ExpressionCommandResolver.Resolve(command, data, listData, env);
    }

    public static bool Compare(string field, string op, int value)
    {
        int fieldValue;

        if (field.StartsWith("#")) {
            var resolved = ExpressionFieldResolver.Resolve(field[1..]);
            if (resolved == null) { return false; }
            fieldValue = Convert.ToInt32(resolved);
        } else {
            if (!int.TryParse(field, out fieldValue)) {
                var resolved = ExpressionFieldResolver.Resolve(field);
                if (resolved == null) { return false; }
                fieldValue = Convert.ToInt32(resolved);
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

    public bool Contains(string fieldName, List<object> data)
    {
        // var container = fieldName.ToUpper();
        var resolved = ExpressionFieldResolver.Resolve(fieldName.StartsWith("#") ? fieldName[1..] : fieldName);
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
    private string expression;
    private readonly List<ExpressionNode> nodes = new ();
    public Dictionary<string, object> environments = new ();

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

    private ExpressionNode()
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

    public async UniTask<object> ExecuteExpressionAsync()
    {
        try
        {
            var result = ExecuteExpression();
            if (result is UniTask<object> task)
            {
                result = await task;
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
        var injectedExpression = InjectEnv();
        var match = Regex.Match(injectedExpression, @"[><\!\?\@=]");
        var command = injectedExpression[..match.Index];
        var op = injectedExpression.Substring(match.Index, (match.Index + 1) < injectedExpression.Length && injectedExpression[match.Index + 1] == '=' ? 2 : 1);
        var data = injectedExpression[(match.Index + op.Length)..];
        var listData = new List<object>();
        if (data.Length > 0 && data[0] == '[')
        {
            try
            {
                listData = MessagePackSerializer.Deserialize<object[]>(MessagePackSerializer.ConvertFromJson(data)).ToList();
            }
            catch (Exception e)
            {
                // Debug.LogWarning(e.ToString());
            }
        }

        // UnityEngine.Debug.Log($"{command} {op} {data} {listData.Count}");

        switch (op)
        {
            case "@":
                return ExpressionExecutor.Execute(command, data, listData, environments);
            case ">":
            case ">=":
            case "<":
            case "%":
            case "<=":
            case "==":
                if (int.TryParse(data, out var rightValue))
                {
                    return ExpressionExecutor.Compare(command, op, rightValue);
                }
                break;
            case "?":
                if (data[0] == '[')
                {
                    return ExpressionExecutor.Instance.Contains(command, listData);
                }
                break;
            default:
                break;
        }
        return null;
    }

    public async UniTask<object> ExecuteAsync()
    {
        if (expression == null && nodes.Count == 0)
        {
            return null;
        }

        if (nodes.Count == 0 && !string.IsNullOrEmpty(expression))
        {
            return await ExecuteExpressionAsync();
        }

        int resultIndex = 0;
        ExecuteResult executeResult = new ExecuteResult(null);
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
                        executeResult |= new ExecuteResult(value);
                    }
                    else
                    {
                        executeResult &= new ExecuteResult(value);
                    }
                    SetEnv($"${resultIndex++}", value);
                    break;
            }
        }

        // Debug.Log($"Expression Result: {excuteResult.value}");

        return executeResult.value;
    }

    public object Execute()
    {
        if (expression == null && nodes.Count == 0)
        {
            return null;
        }

        if (nodes.Count == 0 && !string.IsNullOrEmpty(expression))
        {
            return ExecuteExpression();
        }

        int resultIndex = 0;
        ExecuteResult executeResult = new ExecuteResult(null);
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
                    ExecuteResult result = new ExecuteResult(subNode.Execute());
                    subNode.environments.Clear();
                    if (op == "|")
                    {
                        executeResult |= result;
                    }
                    else
                    {
                        executeResult &= result;
                    }
                    SetEnv($"${resultIndex++}", result.value);
                    break;
            }
        }

        Debug.Log($"Expression Result: {executeResult.value}");

        return executeResult.value;
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

        void GetExpression(int idx)
        {
            string sub = raw.Substring(cursor, idx - cursor).Trim();
            cursor = idx;
            if (sub.Length > 0)
            {
                stack.Peek().nodes.Add(new ExpressionNode(sub));
            }
        }

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
        if (value is UniTask<object> taskResult)
        {
            if (taskResult.Status == UniTaskStatus.Succeeded)
            {
                this.value = taskResult.GetAwaiter().GetResult();
            }
            else
            {
                // Debug.LogWarning("ExecuteResult Task Not Completed!");
                this.value = taskResult;
            }
        }
        else
        {
            this.value = value;
        }
    }
    public static ExecuteResult operator | (ExecuteResult left, ExecuteResult right)
    {
        if (left.value is not bool l || right.value is not bool r)
        {
            return right;
        }
        return new ExecuteResult(l | r);
    }

    public static ExecuteResult operator & (ExecuteResult left, ExecuteResult right)
    {
        if (left.value is not bool l || right.value is not bool r)
        {
            return right;
        }
        return new ExecuteResult(l & r);
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
        if (string.IsNullOrEmpty(expression)) { return null; }
        var node = ExpressionNode.ParseExpression(expression);
        if (environments == null) return node.Execute();
        foreach (var item in environments)
        {
            node.SetEnv(item.Key, item.Value);
        }
        return node.Execute();
    }

    public static async UniTask<object> ExecuteExpressionAsync(this string expression, Dictionary<string, object> environments = null) {
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
