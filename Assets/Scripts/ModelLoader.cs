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
    private const string dir = "Models/";

    public static ModelLoader Instance {get; private set;}

    public bool loaded {get; private set;}

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        Load().Coroutine();
    }

    private async Task Load() {
        if (loaded) {
            return ;
        }
        var asm = Assembly.GetAssembly(typeof(ModelContainerOf));
        var types = asm.GetExportedTypes().Where((Type type) => type.IsDefined(typeof(ModelContainerOf), false)).OrderBy((type) => 
            ((ModelContainerOf)type.GetCustomAttribute(typeof(ModelContainerOf), false)).LoadOrder).ToArray();

        foreach(var type in types) {
            var modelAttr = (ModelContainerOf)type.GetCustomAttribute(typeof(ModelContainerOf), false);
            var modelType = modelAttr.ModelType;
            var loader = Resources.LoadAsync($"{dir}{modelType.Name}");
            while (!loader.isDone) {
                await YieldCoroutine.WaitForSeconds(0.005f);
            }
            var asset = loader.asset as TextAsset;
            var modelListType = typeof(List<>).MakeGenericType(modelType);
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            if (asset == null) continue;
            object modelList = MessagePackSerializer.Deserialize(modelListType, asset.bytes);
            object modelContainer = type.GetProperty("Instance").GetMethod.Invoke(null, null);
            type.GetField(modelAttr.ListField, flags)?.SetValue(modelContainer, modelList);
            type.GetMethod("OnLoad", flags)?.Invoke(modelContainer, null);
        }
        loaded = true;
    }
}