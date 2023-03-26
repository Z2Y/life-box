using System;
using System.Collections.Generic;
using Logic.Enemy.Scriptable;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Logic.Enemy
{
    public class SimpleAnimalBornPlace : MonoBehaviour
    {

        private AnimalSpawner _spawner;

        public int MaxAliveInstance = 10;

        public float SpawnRange = 2f;

        public float SpawnInterval = 2f;

        private const float SpawnVariance = 0.5f;

        private void Awake()
        {
            _spawner = ScriptableObject.CreateInstance<AnimalSpawner>();
        }

        private void Start()
        {
            DoSpawn();
        }

        private void OnDestroy()
        {
            _spawner.Clear();
        }

        private async void DoSpawn()
        {
            while (enabled)
            {

                await YieldCoroutine.WaitForSeconds(SpawnInterval + Random.Range(-SpawnVariance, SpawnVariance));

                if (_spawner.CurrentAlive() < MaxAliveInstance)
                {
                    _spawner.Spawn(1, transform.position, SpawnRange, SpawnInterval);
                }
            }
        }
    }
}