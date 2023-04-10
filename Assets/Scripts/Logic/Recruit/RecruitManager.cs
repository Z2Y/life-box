using System.Collections.Generic;
using Assets.HeroEditor.Common.Scripts.Common;
using Cathei.LinqGen;
using Random = UnityEngine.Random;
using Model;
using Utils;

namespace Logic.Recruit
{
    public class RecruitManager : Singleton<RecruitManager>
    {
        private RecruitConfiguration config;

        public void Use(RecruitConfiguration newConfig)
        {
            config = newConfig;
        }

        public IList<RecruitCharacter> RefreshRecruit(int size)
        {
            return Gen.Enumerable.Range(0, size).Select((_) =>
            {
                var p = Random.Range(0, 1);
                var current = 0f;
                foreach (var item in config.items)
                {
                    current += item.probability;
                    if (current >= p)
                    {
                        return RandomRecruit(item);
                    }
                }

                return RandomRecruit(config.items.Gen().First());
            }).GetEnumerator().ToList();
        }

        private RecruitCharacter RandomRecruit(RecruitConfigItem recruitConfig)
        {
            var item = new RecruitCharacter
            {
                Character = new Character
                {
                    ID = IDGenerator.GenerateInstanceId(),
                    ModelID = recruitConfig.npc_includes.Count > 0 ? recruitConfig.npc_includes.Random() : -1
                },
                Property = LifePropertyFactory.Random(recruitConfig.maxPoint),
                Cost = recruitConfig.cost,
                Level = recruitConfig.level
            };
            return item;
        }
    }
}