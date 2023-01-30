using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;


[AttributeUsage(AttributeTargets.Class)]
public class ModelContainerOf : Attribute {

    public Type ModelType {get; set;}
    public string ListField {get; set;}
    public int LoadOrder {get; set;}
    public ModelContainerOf(Type type, string listField, int order = 0) {
        ModelType = type;
        ListField = listField;
        LoadOrder = order;
    }
}

public class ModelLoader : MonoBehaviour {
    private static readonly string dir = "Models/";

    public static ModelLoader Instance {get; private set;}

    public bool loaded {get; private set;}

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        Load().Coroutine();
    }
    
    public async Task Load() {
        Assembly asm = Assembly.GetAssembly(typeof(ModelContainerOf));
        Type[] types = asm.GetExportedTypes().Where((Type type) => type.IsDefined(typeof(ModelContainerOf), false)).OrderBy((type) => {
            return ((ModelContainerOf)type.GetCustomAttribute(typeof(ModelContainerOf), false)).LoadOrder;
        }).ToArray();

        foreach(Type type in types) {
            ModelContainerOf modelAttr = (ModelContainerOf)type.GetCustomAttribute(typeof(ModelContainerOf), false);
            Type modelType = modelAttr.ModelType;
            ResourceRequest loader = Resources.LoadAsync($"{dir}{modelType.Name}");
            while (!loader.isDone) {
                await YieldCoroutine.Instance.WaitForSeconds(0.005f);
            }
            TextAsset asset = loader.asset as TextAsset;
            Type modelListType = typeof(List<>).MakeGenericType(modelType);
            object modelList = MessagePackSerializer.Deserialize(modelListType, asset.bytes);
            object modelContainer = type.GetProperty("Instance").GetMethod.Invoke(null, null);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            type.GetField(modelAttr.ListField, flags)?.SetValue(modelContainer, modelList);
            type.GetMethod("OnLoad", flags)?.Invoke(modelContainer, null);
        }
        loaded = true;
    }
}