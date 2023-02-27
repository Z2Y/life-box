using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Model;

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
        public long ModelID;
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Model.Character), "characters", 1)]
    public class CharacterCollection
    {
        private readonly Dictionary<long, Character> lookup = new ();
        private readonly List<Character> characters = new ();
        private static CharacterCollection _instance;
        private CharacterCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(Character character in characters) {
                lookup.Add(character.ID, character);
                Place initialPlace = PlaceCollection.Instance.GetPlace(character.PlaceID);
                initialPlace?.Characters.Add(character.ID, character);
                if (character.RelationID.Length != character.RelationValue.Length) continue;
                for(int i = 0; i < character.RelationID.Length; i++) {
                    character.Relations.Add(character.RelationID[i], character.RelationValue[i]);
                }
            }
        }

        public static CharacterCollection Instance => _instance ??= new CharacterCollection();

        public Character GetCharacter(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public List<Character> Characters => characters;
    }
}