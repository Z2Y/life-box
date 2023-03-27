using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Logic.Enemy.Scriptable
{
    [CreateAssetMenu(menuName = "EnemySpawner/AnimalSpawner", order = 1)]
    public class AnimalSpawner : EnemySpawner
    {
        private static readonly PrefabPool<SimpleAnimal, string> Pool = new();
        
        private readonly List<SimpleAnimal> activeAnimals = new();

        public string[] animalTypes;

        private const float SpawnVariance = 0.5f;

        private int currentAliveInstance;

        private bool isSpawning;

        private Action _onDeathAll;

        private Action _onDeathSingle;

        private Action _onTerminate;

        private string[] animalTypesOverride;

        public override int CurrentAlive()
        {
            return currentAliveInstance;
        }

        public override async void Spawn(int count, Vector3 position, float range, float interval)
        {
            isSpawning = true;
            var usedAnimalTypes = animalTypesOverride ?? animalTypes;
            for (var i = 0; i < count; i++)
            {
                
                var animalType = usedAnimalTypes[Random.Range(0, usedAnimalTypes.Length)];
                var animal = await Pool.GetAsync(animalType);

                if (!isSpawning)
                {
                    Pool.Return(animal, animal.animalType);
                    return;
                }
                    
                var direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
                animal.transform.position = position +
                                            direction * Random.Range(0f,
                                                range / Mathf.Max(1, (int)direction.magnitude));
                animal.OnLoaded(animalType);
                animal.AddDeathListener(_onAnimalDeath);
                activeAnimals.Add(animal);
                currentAliveInstance++;
                
                await YieldCoroutine.WaitForSeconds(Mathf.Max(0, interval + Random.Range(-SpawnVariance, SpawnVariance)));
            }
            isSpawning = false;
            animalTypesOverride = null;
        }

        public override void Spawn(string variant, int count, Vector3 position, float range, float interval)
        {
            animalTypesOverride = new[] { variant };
            Spawn(count, position, range, interval);
        }

        private void _onAnimalDeath(SimpleAnimal animal)
        {
            animal.RemoveDeathListener(_onAnimalDeath);
            Pool.Return(animal, animal.animalType);
            activeAnimals.Remove(animal);
            currentAliveInstance--;
            
            _onDeathSingle?.Invoke();
            if (currentAliveInstance <= 0 && !isSpawning)
            {
                _onDeathAll?.Invoke();
                _onDeathAll = null;
                _onDeathSingle = null;
                _onTerminate = null;
            }
        }

        public override void OnTerminate(Action onTerminate)
        {
            _onTerminate += onTerminate;
        }

        public override void Clear()
        {
            isSpawning = false;
            if (currentAliveInstance > 0)
            {
                _onTerminate?.Invoke();
                _onTerminate = null;
            }

            foreach (var animal in activeAnimals.Where(animal => animal != null))
            {
                Pool.Return(animal, animal.animalType);
            }

            currentAliveInstance = 0;
            activeAnimals.Clear();
        }

        public override void OnDeathSingle(Action onEnemyDeath)
        {
            _onDeathSingle += onEnemyDeath;
        }

        public override void OnDeathAll(Action onEnemyAllDeath)
        {
            _onDeathAll += onEnemyAllDeath;
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}