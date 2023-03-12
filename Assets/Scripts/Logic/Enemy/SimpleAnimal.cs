using System;
using Battle.Realtime.Ai;
using UnityEngine;
using Utils;

namespace Logic.Enemy
{
    [PrefabResourceWithArgs("Prefabs/Animal/{0}/Main")]
    public class SimpleAnimal : MonoBehaviour, IOnPrefabLoaded<string>
    {
        private string animalType;
        private SimpleAI ai;
        private void Awake()
        {
            ai = GetComponent<SimpleAI>();
        }

        private void Start()
        {
            ai.StartAI();
        }

        public void OnLoaded(string arg)
        {
            Debug.Log($"{arg} Loaded");
            animalType = arg;
            gameObject.name = arg;
        }
    }
}