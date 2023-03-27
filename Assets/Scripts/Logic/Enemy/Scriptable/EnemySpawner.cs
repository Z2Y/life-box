using System;
using UnityEngine;

namespace Logic.Enemy.Scriptable
{
    public interface IEnemySpawner
    {
        public int CurrentAlive();

        public void Spawn(int count, Vector3 position, float range, float interval);

        public void Spawn(string variant, int count, Vector3 position, float range, float interval);

        public void OnDeathSingle(Action onEnemyDeath);

        public void OnDeathAll(Action onEnemyAllDeath);

        public void OnTerminate(Action onTerminate);

        public void Clear();
    }

    public abstract class EnemySpawner : ScriptableObject, IEnemySpawner
    {
        public abstract int CurrentAlive();

        public abstract void Spawn(int count, Vector3 position, float range, float interval);
        public virtual void Spawn(string variant, int count, Vector3 position, float range, float interval)
        {
            Spawn(count, position, range, interval);
        }

        public abstract void OnDeathSingle(Action onEnemyDeath);
        
        public abstract void OnDeathAll(Action onEnemyAllDeath);
        public abstract void OnTerminate(Action onTerminate);

        public abstract void Clear();
    }
}