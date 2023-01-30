using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public class FieldResolverHandler : Attribute {
    public string FieldName {get; set;}
    public FieldResolverHandler(string fieldName) {
        FieldName = fieldName;
    }
}

public interface IFieldResolver {
    object Resolve();
}

public abstract class FieldResolver : IFieldResolver {
    public abstract object Resolve();
}