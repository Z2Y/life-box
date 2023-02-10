using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Utils.Shuffle;

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
        public long MapID;
        public long[] Commands;
        public long[] Child;
        public long Parent;
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
    public class PlaceCollection
    {
        private readonly Dictionary<long, Model.Place> lookup = new ();
        private readonly List<Model.Place> places = new ();
        private static PlaceCollection _instance;
        private PlaceCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(var place in places) {
                lookup.Add(place.ID, place);
            }
        }

        public static PlaceCollection Instance => _instance ??= new PlaceCollection();

        public Model.Place GetPlace(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public List<Model.Place> Places => places;

        public Model.Place RandomPlace(Model.PlaceType placeType) {
            return places.Where((place) => place.PlaceType == placeType).ToList().Shuffle().FirstOrDefault();
        }
    }
}