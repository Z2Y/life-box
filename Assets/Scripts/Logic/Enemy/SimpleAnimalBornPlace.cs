using UnityEngine;
using Utils;

namespace Logic.Enemy
{
    public class SimpleAnimalBornPlace : MonoBehaviour
    {
        public static readonly PrefabPool<SimpleAnimal, string> Pool = new();

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


        private async void DoSpawn()
        {
            while (enabled)
            {
                await YieldCoroutine.WaitForSeconds(SpawnInterval + Random.Range(-SpawnVariance, SpawnVariance));
                if (currentAliveInstance < MaxAliveInstance && animalTypes.Length > 0)
                {
                    var animalType = animalTypes[Random.Range(0, animalTypes.Length)];
                    var animal = await Pool.GetAsync(animalType);
                    var direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
                    animal.transform.position = transform.position +
                                                direction * Random.Range(0f,
                                                    SpawnRange / Mathf.Max(1, (int)direction.magnitude));
                    animal.OnLoaded(animalType);
                    animal.SetBornPlace(this);
                    currentAliveInstance++;
                }
            }
        }

        public void OnAnimalDeath(SimpleAnimal animal)
        {
            Pool.Return(animal, animal.animalType);
            currentAliveInstance--;
        }
    }
}