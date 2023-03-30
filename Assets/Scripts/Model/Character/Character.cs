using System;
using System.Collections.Generic;
using Controller;
using MessagePack;
using Model;
using Realms;

namespace Model
{
    [Serializable]
    public partial class Character : IRealmObject
    {
        [Indexed]
        public long ID { get; set; }
        public string Name { get; set; }
        public long PlaceID { get; set; }
        public long ModelID { get; set; }
        
        public IList<long> RelationID { get; }
        
        public IList<long> RelationValue { get; }
        public IList<long> Skills { get; }
    }
}

namespace ModelContainer
{
    public static class CharacterCollection
    {
        public static void LoadPlayerCharacter()
        {
            var player = new Character()
            {
                ID = 0,
                Name = "xxx",
                ModelID = 0
            };
            var enemy = new Character()
            {
                ID = 1,
                Name = "xxx",
                ModelID = 1
            };
            RealmDBController.Realm.Write(() =>
            {
                RealmDBController.Realm.Add(player, true);
                RealmDBController.Realm.Add(enemy, true);
            });
        }

        public static Character GetCharacter(long id)
        {
            return RealmDBController.Realm.Find<Character>(id);
        }
        
    }
}