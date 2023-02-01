using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Model;

namespace Model
{

    public enum PlaceType {
        City = 0,
        Shop = 1,
        Other = 2,
        Water = 3,
    }

    [MessagePackObject(true)]
    public class Place
    {
        public long ID;
        public PlaceType PlaceType;
        public string Name;
        public string Scene;
        public long[] Commands;
        public long[] Child;
        public long Parent;
        public Dictionary<long, Character> Characters = new Dictionary<long, Character>();

        public override string ToString()
        {
            return $"【{Name}】";
        }

    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Model.Place), "places")]
    public class PlaceCollection
    {
        private Dictionary<long, Model.Place> lookup = new Dictionary<long, Model.Place>();
        private List<Model.Place> places = new List<Model.Place>();
        private static PlaceCollection _instance;
        private PlaceCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(var place in places) {
                lookup.Add(place.ID, place);
            }
        }

        public static PlaceCollection Instance => _instance ?? (_instance = new PlaceCollection());

        public Model.Place GetPlace(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public List<Model.Place> Places => places;

        public Model.Place RamdomPlace(Model.PlaceType placeType) {
            List<Model.Place> valids = places.Where((Model.Place place) => place.PlaceType == placeType).ToList();
            if (valids.Count <= 0) { return null; }
            return valids.FirstOrDefault();
            // return valids[UnityEngine.Random.Range(0, valids.Count)];
        }
    }
}