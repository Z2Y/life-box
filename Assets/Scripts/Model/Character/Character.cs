using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;

namespace Model
{

    [MessagePackObject(true)]
    public class Character
    {
        public long ID;
        public string Name;
        public long[] RelationID;
        public long[] RelationValue;
        public long[] Skills;
        public long PlaceID;
        public Dictionary<long, long> Relations = new Dictionary<long, long>();
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Model.Character), "characters", 1)]
    public class CharacterCollection
    {
        private Dictionary<long, Model.Character> lookup = new Dictionary<long, Model.Character>();
        private List<Model.Character> characters = new List<Model.Character>();
        private static CharacterCollection _instance;
        private CharacterCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(Model.Character character in characters) {
                lookup.Add(character.ID, character);
                Model.Place initalPlace = PlaceCollection.Instance.GetPlace(character.PlaceID);
                if (initalPlace != null) {
                    initalPlace.Characters.Add(character.ID, character);
                }
                if (character.RelationID.Length == character.RelationValue.Length) {
                    for(int i = 0; i < character.RelationID.Length; i++) {
                        character.Relations.Add(character.RelationID[i], character.RelationValue[i]);
                    }
                }
            }
        }

        public static CharacterCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CharacterCollection();
                }
                return _instance;
            }
        }

        public Model.Character GetCharacter(long id)
        {
            Model.Character value;
            if (lookup.TryGetValue(id, out value))
            {
                return value;
            }
            return null;
        }

        public List<Model.Character> Characters {
            get {
                return characters;
            }
        }
    }
}