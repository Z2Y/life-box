using System;
using System.Collections.Generic;
using Controller;
using MessagePack;
using Realms;
using InteractMenuConfig = Model.InteractMenuConfig;

namespace Model
{
    [MessagePackObject(true)]
    [Serializable]
    public partial class InteractMenuConfig : IRealmObject
    {
        [PrimaryKey]
        public long ID { get; set; }
        public string Name  { get; set; }
        public string IconPath  { get; set; }
        public string MenuResolver  { get; set; }
        public long keyCode  { get; set; }
    }
}

namespace ModelContainer
{
    public static class InteractMenuConfigCollection
    {
        public static InteractMenuConfig GetConfig(long id)
        {
            return RealmDBController.Realm.Find<InteractMenuConfig>(id);
        }
    }
}