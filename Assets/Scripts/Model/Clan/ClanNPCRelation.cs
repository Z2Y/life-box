using System;
using MessagePack;
using Realms;

namespace Model
{
    [Serializable]
    public partial class ClanNPCRelation : IRealmObject
    {
        public long ClanID { get; set; }
        public long CharacterID { get; set; }
        public long Relation { get; set; }
        public long RelationTitleID { get; set; }
    }

    [Serializable]
    [MessagePackObject(true)]
    public partial class ClanRelationTitle : IRealmObject
    {
        public long ID { get; set; }
        public long ClanID { get; set; }
        public long MinRelation { get; set; }
        public string ExtraRule { get; set; }
        public string Title { get; set; }
    }
}