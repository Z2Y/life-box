using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;
using Object = System.Object;


namespace Controller
{
    public class CharacterTrail : MonoBehaviour
    {
        public int maxTrailCount = 5; // 最大拖影数量
        public float trailInterval = 0.05f; // 拖影间隔时间
        public float trailFadeSpeed = 0.8f; // 拖影消失速度

        private List<CharacterTrialRenderer> trailList; // 拖影实例列表
        private float trailTimer; // 拖影计时器
        private SortingGroup characterRenderer; // 角色渲染器
        private static bool spawning;
        private static readonly PrefabPool<CharacterTrialRenderer> pool = new();

        void Start()
        {
            trailList = new List<CharacterTrialRenderer>();
            characterRenderer = GetComponentInChildren<SortingGroup>();
            enabled = false;
        }

        private void OnDisable()
        {
            foreach (var trial in trailList)
            {
                pool.Return(trial);
            }
            trailList.Clear();
        }

        void Update()
        {
            trailTimer += Time.deltaTime;

            if (trailTimer > trailInterval)
            {
                trailTimer = 0;

                if (trailList.Count >= maxTrailCount)
                {
                    pool.Return(trailList[0]);
                    trailList.RemoveAt(0);
                }
                
                addTrail();

            }

            for (int i = 0; i < trailList.Count; i++)
            {
                trailList[i].updateAlpha(trailFadeSpeed * Time.deltaTime);

                if (trailList[i].Alpha <= 0)
                {
                    pool.Return(trailList[i]);
                    trailList.RemoveAt(i);
                    i--;
                }
            }
        }
        
        private async void addTrail()
        {
            if (spawning) return;
            spawning = true;
            var trail = await pool.GetAsync();
            var trialTransform = trail.transform;
            trail.Init(transform);
            trail.updateSortingOrder(characterRenderer.sortingOrder - 1);

            trailList.Add(trail);
            spawning = false;
        }
    }

}