using System;
using System.Linq;
using System.Collections.Generic;
using Controller;
using MessagePack;
using Realms;
using Utils.Shuffle;

namespace Model
{

    public enum PlaceType {
        City = 0,
        Shop = 1,
        Other = 2,
        Water = 3,
        Fuben = 4
    }

    [MessagePackObject(true)]
    [Serializable]
    public partial class Place : IRealmObject
    {
        public long ID { get; set; }
        public PlaceType PlaceType;
        public string Name { get; set; }
        public long MapID { get; set; }
        public IList<long> Commands { get; }
        public IList<long> Child { get;  }
        public long Parent { get; set; }
        public Dictionary<long, Character> Characters = new ();

        public override string ToString()
        {
            return $"【{Name}】";
        }

    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Model.Place), "places")]
    public static class PlaceCollection
    {

        public static Model.Place GetPlace(long id)
        {
            return RealmDBController.Realm.Find<Model.Place>(id);
        }
    }
}