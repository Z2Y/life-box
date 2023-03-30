using System;
using System.Linq;
using System.Collections.Generic;
using Controller;
using MessagePack;
using Model;
using Realms;

namespace Model
{

    [MessagePackObject(true)]
    [Serializable]
    public partial class Command : IRealmObject
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public int Global { get; set; }
        public string Description { get; set; }
        public string Expression { get; set; }
        public string Condition { get; set; }
    }
}

namespace ModelContainer
{
    public static class CommandCollection
    {

        public static Command GetCommand(long id)
        {
            return RealmDBController.Realm.Find<Command>(id);
        }

        public static IEnumerable<int> GetValidCommandIndex(IEnumerable<long> ids)
        {
            return ids.Select((long id, int idx) =>
            {
                var command = GetCommand(id);
                if (command == null) return -1;
                if (command.Condition.ExecuteExpression() is bool isValid)
                {
                    UnityEngine.Debug.Log($"isValid {(bool?)isValid}");
                    return isValid ? idx : -1;
                }
                return idx;
            }).Where((int v) => v >= 0);
        }

        public static IEnumerable<Command> GetValidGlobalCommands()
        {
            return RealmDBController.Realm.All<Command>().Where((command) => command.Global > 0).ToList().Where((command) => (command.Condition.ExecuteExpression() is true));
        }

        public static IEnumerable<Command> GetValidCommands(IList<long> ids)
        {
            return GetValidCommandIndex(ids).Select((idx) => GetCommand(ids[idx]));
        }

    }
}