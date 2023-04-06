using System;
using System.Linq;
using System.Collections.Generic;
using Cathei.LinqGen;
using Controller;
using MessagePack;
using Model;
using Realms;
using Utils;

namespace Model
{

    [MessagePackObject(true)]
    [Serializable]
    public partial class Command : IRealmObject
    {
        [PrimaryKey]
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
            return RealmDBController.Db.Find<Command>(id);
        }
        

        public static IEnumerable<Command> GetValidGlobalCommands()
        {
            return RealmDBController.Db.All<Command>().Where((command) => command.Global > 0).ToList().Where((command) => (command.Condition.ExecuteExpression() is true));
        }

        public static IEnumerable<Command> GetValidCommands(IList<long> ids)
        {
            return ids.Gen().Select((id) =>
            {
                var command = GetCommand(id);
                if (command == null) return null;
                if (command.Condition.ExecuteExpression() is bool isValid)
                {
                    UnityEngine.Debug.Log($"isValid {(bool?)isValid}");
                    return isValid ? command : null;
                }
                return command;
            }).Where(LinqHelper.IsNotNull).AsEnumerable();
        }

    }
}