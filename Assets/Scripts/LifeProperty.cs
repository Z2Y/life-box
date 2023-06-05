using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Cathei.LinqGen;

public class LifeProperty
{
    public readonly Dictionary<SubPropertyType, PropertyValue> propertys = new ();

    public readonly PropertyChangeEvent onPropertyChange = new ();

    public PropertyValue GetProperty(SubPropertyType pType)
    {
        return propertys.TryGetValue(pType, out var value) ? value : null;
    }

    public bool AddProperty(SubPropertyType type, int value)
    {
        if (propertys.ContainsKey(type)) return false;
        propertys.Add(type, new PropertyValue(type, value, this));
        return true;
    }
}

public static class LifePropertyFactory
{
    public class RandomPropertyConfig
    {
        public readonly SubPropertyType propertyType;
        public RangeInt propertyRange;

        public RandomPropertyConfig(SubPropertyType type, RangeInt range)
        {
            propertyType = type;
            propertyRange = range;
        }
    }

    private static readonly RangeInt DefaultRange = new (0, 20);

    private static readonly List<RandomPropertyConfig> DefaultRandomConfig = new List<RandomPropertyConfig>() {
        new (SubPropertyType.Fortune, DefaultRange),
        new (SubPropertyType.Literary, DefaultRange),
        new (SubPropertyType.Wealth, DefaultRange),
        new (SubPropertyType.Agile, DefaultRange),
        new (SubPropertyType.Strength, DefaultRange)
    };
    public static LifeProperty Random(int total, List<RandomPropertyConfig> configs = null)
    {
        configs ??= DefaultRandomConfig;
        var lifeProperty = new LifeProperty();
        // int randomTotal = configs.Gen().Select((config) => config.propertyRange.end).Sum();
        foreach (var config in configs)
        {
            var min = Math.Min(total, config.propertyRange.start);
            var max = Math.Min(total, config.propertyRange.end);
            var value = UnityEngine.Random.Range(min, max + 1);
            total -= value;
            lifeProperty.AddProperty(config.propertyType, value);
        }
        lifeProperty.AddProperty(SubPropertyType.HitPoint, UnityEngine.Random.Range(10, 20));
        lifeProperty.AddProperty(SubPropertyType.Energy, 10);
        return lifeProperty;
    }
}

public enum PropertyType
{
    Basic, // 基础属性
    Attack, // 攻击属性
    Defend, // 防御属性
}

[AttributeUsage(AttributeTargets.Field)]
public class SubPropertyOf : Attribute
{
    public SubPropertyOf(PropertyType pType, string name, bool frozen = false, bool limitMax = false)
    {
        PropertyType = pType;
        PropertyName = name;
        Frozen = frozen;
        LimitMax = limitMax;
    }
    public PropertyType PropertyType { get; private set; }
    public string PropertyName { get; private set; }
    public bool Frozen { get; set;}
    public bool LimitMax { get; set;}
}

public enum SubPropertyType
{
    [SubPropertyOf(PropertyType.Basic, "生命值", false, true)]
    HitPoint,
    [SubPropertyOf(PropertyType.Basic, "体力", false, true)]
    Energy,
    [SubPropertyOf(PropertyType.Attack, "力量")]
    Strength,
    [SubPropertyOf(PropertyType.Basic, "文采")]
    Literary,
    [SubPropertyOf(PropertyType.Basic, "身法")]
    Agile,
    [SubPropertyOf(PropertyType.Basic, "运气")]
    Fortune,
    [SubPropertyOf(PropertyType.Basic, "财富")]
    Wealth,
}

public class PropertyValue
{
    private int _value;
    private int _max;
    public SubPropertyType Type {get; private set;}
    public LifeProperty owner { get; private set;}
    public int max {
        get => _max;
        set {
            _max = value;
            owner?.onPropertyChange?.Invoke();
        }
    }
    public int value {
        get => _value;
        set {
            if (value > max) {
                value = max;
            } else {
                _value = value;
            }
            owner?.onPropertyChange?.Invoke();
        }
    }

    public void Change(int value)
    {
        _value = value;
        _max = value;
        owner?.onPropertyChange?.Invoke();
    }

    public PropertyValue(SubPropertyType type, int v, LifeProperty o)
    {
        Type = type;
        owner = o;
        _value = v;
        max = type.IsLimitMax() ? v : int.MaxValue;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}

public static class PropertyHelper
{
    public static string GetPropertyName(this SubPropertyType propertyType)
    {
        Type type = propertyType.GetType();
        FieldInfo fieldInfo = type.GetField(propertyType.ToString());
        // 判断是否加了特性注解
        if (fieldInfo.IsDefined(typeof(SubPropertyOf), true))
        {
            SubPropertyOf remark = (SubPropertyOf)fieldInfo.GetCustomAttribute(typeof(SubPropertyOf), true);
            return remark.PropertyName;
        }
        else
        {
            return propertyType.ToString();
        }
    }

    public static bool IsLimitMax(this SubPropertyType propertyType) {
        Type type = propertyType.GetType();
        FieldInfo fieldInfo = type.GetField(propertyType.ToString());
        if (fieldInfo.IsDefined(typeof(SubPropertyOf), true))
        {
            SubPropertyOf remark = (SubPropertyOf)fieldInfo.GetCustomAttribute(typeof(SubPropertyOf), true);
            return remark.LimitMax;
        }
        else
        {
            return false;
        }        
    }

    public static bool IsFrozen(this SubPropertyType propertyType) {
        Type type = propertyType.GetType();
        FieldInfo fieldInfo = type.GetField(propertyType.ToString());
        // 判断是否加了特性注解
        if (fieldInfo.IsDefined(typeof(SubPropertyOf), true))
        {
            SubPropertyOf remark = (SubPropertyOf)fieldInfo.GetCustomAttribute(typeof(SubPropertyOf), true);
            return remark.Frozen;
        }
        else
        {
            return false;
        }        
    }

    public static void SetFrozen(this SubPropertyType propertyType, bool value) {
        Type type = propertyType.GetType();
        FieldInfo fieldInfo = type.GetField(propertyType.ToString());
        // 判断是否加了特性注解
        if (fieldInfo.IsDefined(typeof(SubPropertyOf), true))
        {
            SubPropertyOf remark = (SubPropertyOf)fieldInfo.GetCustomAttribute(typeof(SubPropertyOf), true);
            remark.Frozen = value;
        }       
    }
}

public class PropertyChangeEvent : UnityEvent { }