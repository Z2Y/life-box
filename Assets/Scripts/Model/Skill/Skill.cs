using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;

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
    public class Skill
    {
        public long ID;
        public SkillType SkillType;
        public SelectType SelectType;
        public string Name;
        public string Effect;
        public string Cost;
        public long Suilt;
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
        private Dictionary<long, Model.Skill> lookup = new Dictionary<long, Model.Skill>();
        private List<Model.Skill> skills = new List<Model.Skill>();
        private static SkillCollection _instance;
        private SkillCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(Model.Skill skill in skills) {
                lookup.Add(skill.ID, skill);
            }
        }

        public static SkillCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SkillCollection();
                }
                return _instance;
            }
        }

        public Model.Skill GetSkill(long id)
        {
            Model.Skill value;
            if (lookup.TryGetValue(id, out value))
            {
                return value;
            }
            return null;
        }

        public List<Model.Skill> Skills {
            get {
                return skills;
            }
        }
    }
}