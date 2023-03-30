using System;
using MessagePack;

namespace Model
{
    [Serializable]
    [MessagePackObject(true)]
    public class ClanNPCRelation
    {
        public long ClanID;
        public long CharacterID;
        public long Relation;
        public long RelationTitleID;
    }

    [Serializable]
    [MessagePackObject(true)]
    public class ClanRelationTitle
    {
        public long ID;
        public long ClanID;
        public long MinRelation;
        public string ExtraRule;
        public string Title;
    }
}