using System;
using System.Linq;
using System.Collections.Generic;
using HeroEditor.Common.Enums;
using MessagePack;
using Model;

namespace Model
{

    public enum SkillType {
        Move = 0,
        Attack = 1,
        Heal = 2,
    }

    public enum SelectType {
        EmptyBlock,
        EnemyBlock,
        FriendBlock,
        AnyBlock
    }

    [MessagePackObject(true)]
    [Serializable]
    public class Skill
    {
        public long ID;
        public SkillType SkillType;
        public SelectType SelectType;
        public WeaponType WeaponType;
        public string Name;
        public string Effect;
        public string Cost;
        public float CoolDown;
        public long Suit;
        public int Attack;
        public int SelectRange;
        public int AttackRange;
        public int Level;

    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Model.Skill), "skills")]
    public class SkillCollection
    {
        private readonly Dictionary<long, Model.Skill> lookup = new ();
        private readonly List<Model.Skill> skills = new ();
        private static SkillCollection _instance;
        private SkillCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(var skill in skills) {
                lookup.Add(skill.ID, skill);
            }
        }

        public static SkillCollection Instance => _instance ??= new SkillCollection();

        public Skill GetSkill(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public List<Skill> Skills => skills;
    }
}