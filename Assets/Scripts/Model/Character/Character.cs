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
        public Dictionary<long, long> Relations = new ();
        public long ModelID;
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Character), "characters", 1)]
    public class CharacterCollection
    {
        private readonly Dictionary<long, Character> lookup = new ();
        private readonly List<Character> characters = new ();
        private static CharacterCollection _instance;
        private CharacterCollection() { }

        private void OnLoad() {
            lookup.Clear();
            // load Main Character
            LoadPlayerCharacter();
            foreach(Character character in characters) {
                lookup.Add(character.ID, character);
                var initialPlace = PlaceCollection.Instance.GetPlace(character.PlaceID);
                initialPlace?.Characters.Add(character.ID, character);
                if (character.RelationID.Length != character.RelationValue.Length) continue;
                for(var i = 0; i < character.RelationID.Length; i++) {
                    character.Relations.Add(character.RelationID[i], character.RelationValue[i]);
                }
            }
        }

        private void LoadPlayerCharacter()
        {
            var player = new Character()
            {
                ID = 0,
                Name = "xxx",
                RelationID = new long[] {},
                RelationValue = new long[] {},
                ModelID = 0
            };
            var enemy = new Character()
            {
                ID = 1,
                Name = "xxx",
                RelationID = new long[] {},
                RelationValue = new long[] {},
                ModelID = 1
            };
            lookup.Add(player.ID, player);
            lookup.Add(enemy.ID, enemy);
        }

        public static CharacterCollection Instance => _instance ??= new CharacterCollection();

        public Character GetCharacter(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public List<Character> Characters => characters;
    }
}