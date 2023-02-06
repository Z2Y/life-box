using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class LifeProperty
{
    public Dictionary<SubPropertyType, PropertyValue> propertys = new Dictionary<SubPropertyType, PropertyValue>();

    public PropertyChangeEvent onPropertyChange = new PropertyChangeEvent();

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

public class LifePropertyFactory
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

    public static RangeInt DefaultRange = new RangeInt(0, 20);

    public static readonly List<RandomPropertyConfig> DefaultRandomConfig = new List<RandomPropertyConfig>() {
        new RandomPropertyConfig(SubPropertyType.Fortune, DefaultRange),
        new RandomPropertyConfig(SubPropertyType.Literary, DefaultRange),
        new RandomPropertyConfig(SubPropertyType.Wealth, DefaultRange),
        new RandomPropertyConfig(SubPropertyType.Agile, DefaultRange),
        new RandomPropertyConfig(SubPropertyType.Strength, DefaultRange)
    };
    public static LifeProperty Random(int total, List<RandomPropertyConfig> configs = null)
    {
        if (configs == null)
        {
            configs = DefaultRandomConfig;
        }
        LifeProperty lifeProperty = new LifeProperty();
        int randomTotal = configs.Select((config) => config.propertyRange.end).Sum();
        foreach (RandomPropertyConfig config in configs)
        {
            int min = Math.Min(total, config.propertyRange.start);
            int max = Math.Min(total, config.propertyRange.end);
            int value = UnityEngine.Random.Range(min, max + 1);
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
    [SubPropertyOf(PropertyType.Basic, "力量")]
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