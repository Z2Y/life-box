using System;
using System.Collections.Generic;
using Cathei.LinqGen;
using Controller;
using MessagePack;
using Microsoft.NET.StringTools;
using Model;
using Realms;

namespace Model
{
    [Serializable]
    [MessagePackObject(true)]
    public partial class Character : IRealmObject
    {
        [PrimaryKey]
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
            RealmDBController.Db.Write(() =>
            {
                RealmDBController.Db.Add(player, true);
                RealmDBController.Db.Add(enemy, true);
            });
        }

        public static Character GetCharacter(long id)
        {
            return RealmDBController.Db.Find<Character>(id);
        }

    }
}