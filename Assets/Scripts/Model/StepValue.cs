using System.Collections.Generic;
using MessagePack;
using Realms;

namespace Model
{
    public enum StepValueType
    {
        Popularity = 0,
    }

    [MessagePackObject(true)]
    public partial class StepValue : IRealmObject
    {
        [PrimaryKey]
        public long ID { get; set; }
        public int ValueType { get; set; }
        public string Name { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public IList<int> StepPoint { get;  } 

    }
}