using System;
using Logic.Enemy;
using Logic.Enemy.Scriptable;
using UnityEngine;

namespace Logic.Map.MapProcedure
{
    [CreateAssetMenu(menuName = "BattleMapProcedure/BattleMapWar", order = 1)]
    public class BattleMapWar : BattleMapProcedure
    {
        public int enemyCount;

        public float spawnRange;

        public float spawnInterval;
        
        [SerializeField]
        private EnemySpawner _enemySpawner;

        private Action _onFinish;

        public override void StartProcedure(BattlePlaceController from)
        {
            place = from;

            _enemySpawner.Spawn(enemyCount, from.transform.position, spawnRange, spawnInterval);
            
            _enemySpawner.OnDeathAll(finish);
        }

        public override void OnProcedureFinish(Action onFinish)
        {
            _onFinish += onFinish;
        }

        public override void TerminateProcedure()
        {
            _enemySpawner.Dispose();
            _onFinish = null;
        }

        private void finish()
        {
            _onFinish?.Invoke();
            _onFinish = null;
        }
    }
}