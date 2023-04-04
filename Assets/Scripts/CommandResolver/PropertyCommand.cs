using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using StructLinq;

[CommandResolverHandler("ChangeProperty")]
public class ChangePropertyCommand : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        PropertyChangeResult result = new PropertyChangeResult();
        for (int i = 0; i < args.Count; i += 2)
        {
            int propertyChangeValue = Convert.ToInt32(args[i + 1]);
            if (Enum.TryParse<SubPropertyType>(args[i] as string, true, out var propertyType) && ChangeProperty(propertyType, propertyChangeValue)) {
                result.Add(propertyType, propertyChangeValue);
            }
        }
        await this.Done();
        return result;
    }

    private bool ChangeProperty(SubPropertyType propertyType, int value)
    {
        if (value == 0 || LifeEngine.Instance.lifeData == null || propertyType.IsFrozen())
        {
            return false;
        }
        PropertyValue propertyValue = LifeEngine.Instance.lifeData.property.GetProperty(propertyType);
        propertyValue.Change(propertyValue.value + value);
        return true;
    }

    public class PropertyChangeResult {
        public Dictionary<SubPropertyType, int> changes = new ();

        public void Add(SubPropertyType propertyType, int value) {
            if (changes.ContainsKey(propertyType)) {
                changes[propertyType] += value;
                if (changes[propertyType] == 0) {
                    changes.Remove(propertyType);
                }
            } else {
                changes[propertyType] = value;
            }
        }

        public override string ToString()
        {
            return string.Join(" ", changes.Keys.ToStructEnumerable().Select(pType => $"【{pType.GetPropertyName()}】 {(changes[pType] > 0 ? "+" : "-")} {changes[pType]}"));
        }
    }
}