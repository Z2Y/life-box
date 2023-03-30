using System;
using System.Linq;
using System.Collections.Generic;
using Controller;
using HeroEditor.Common.Enums;
using MessagePack;
using Model;
using Realms;

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
    public partial class Skill : IRealmObject
    {
        [PrimaryKey]
        public long ID { get; set; }
        public SkillType SkillType => (SkillType)ISkillType;
        public SelectType SelectType => (SelectType)ISelectType;
        public WeaponType WeaponType => (WeaponType)IWeaponType;
        
        public int ISkillType { get; set; }
        public int ISelectType { get; set; }
        public int IWeaponType { get; set; }
        public string Name { get; set; }
        public string Effect { get; set; }
        public string Cost { get; set; }
        public float CoolDown { get; set; }
        
        public long Suit { get; set; }
        public int Attack { get; set; }
        public int SelectRange { get; set; }
        public int AttackRange { get; set; }
        public int Level { get; set; }

    }
}

namespace ModelContainer
{
    public static class SkillCollection
    {
        public static Skill GetSkill(long id)
        {
            return RealmDBController.Realm.Find<Skill>(id);
        }
    }
}