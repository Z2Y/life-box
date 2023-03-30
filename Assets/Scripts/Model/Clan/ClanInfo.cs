using System;
using MessagePack;
using Realms;

namespace Model
{
    [Serializable]
    public partial class ClanInfo : IRealmObject
    {
        [PrimaryKey]
        public long ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long HomePlaceID { get; set; }
    }
}