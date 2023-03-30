using System;
using MessagePack;

namespace Model
{
    [Serializable]
    [MessagePackObject(true)]
    public class ClanInfo
    {
        public long ID;
        public string Name;
        public string Description;
        public long HomePlaceID;
    }
}