using System.Collections.Generic;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Logic.Enemy
{
    public class SimpleAnimalBornPlace : MonoBehaviour
    {
        private static readonly PrefabPool<SimpleAnimal, string> Pool = new();

        private readonly List<SimpleAnimal> activeAnimals = new();

        public string[] animalTypes;

        public int MaxAliveInstance = 10;

        public float SpawnRange = 2f;

        public float SpawnInterval = 2f;

        private const float SpawnVariance = 0.5f;

        private int currentAliveInstance;


        private void Start()
        {
            DoSpawn();
        }

        private void OnDestroy()
        {
            foreach (var animal in activeAnimals)
            {
                if (animal != null)
                {
                    Pool.Return(animal, animal.animalType);
                }
            }

            currentAliveInstance = 0;
            activeAnimals.Clear();
        }

        private async void DoSpawn()
        {
            while (enabled)
            {
                await YieldCoroutine.WaitForSeconds(SpawnInterval + Random.Range(-SpawnVariance, SpawnVariance));
                
                if (currentAliveInstance < MaxAliveInstance && animalTypes.Length > 0)
                {
                    var animalType = animalTypes[Random.Range(0, animalTypes.Length)];
                    var animal = await Pool.GetAsync(animalType);

                    if (this == null)
                    {
                        Pool.Return(animal, animal.animalType);
                        return;
                    }
                    
                    var direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
                    animal.transform.position = transform.position +
                                                direction * Random.Range(0f,
                                                    SpawnRange / Mathf.Max(1, (int)direction.magnitude));
                    animal.OnLoaded(animalType);
                    animal.SetBornPlace(this);
                    activeAnimals.Add(animal);
                    currentAliveInstance++;
                }
            }
        }

        public void OnAnimalDeath(SimpleAnimal animal)
        {
            Pool.Return(animal, animal.animalType);
            activeAnimals.Remove(animal);
            currentAliveInstance--;
        }
    }
}