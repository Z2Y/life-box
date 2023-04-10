using System;
using System.Collections.Generic;
using UnityEngine;

namespace Logic.Recruit
{
    public class RecruitConfiguration : ScriptableObject
    {
        public List<RecruitConfigItem> items;
    }

    [Serializable]
    public struct RecruitConfigItem
    {
        public int maxPoint;
        public float probability;
        public List<long> npc_includes;
        public int cost;
        public int level;
    }
}