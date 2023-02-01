using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Model;

namespace Model
{

    [MessagePackObject(true)]
    public class Command
    {
        public long ID;
        public string Name;
        public int Global;
        public string Description;
        public string Expression;
        public string Condition;
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Model.Command), "commands")]
    public class CommandCollection
    {
        private Dictionary<long, Model.Command> lookup = new Dictionary<long, Model.Command>();
        private List<Model.Command> commands = new List<Model.Command>();
        private static CommandCollection _instance;
        private CommandCollection() { }

        private void OnLoad()
        {
            lookup.Clear();
            foreach (Model.Command command in commands)
            {
                lookup.Add(command.ID, command);
            }
        }

        public Model.Command GetCommand(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public static CommandCollection Instance => _instance ?? (_instance = new CommandCollection());

        public static IEnumerable<int> GetValidCommandIndex(IEnumerable<long> ids)
        {
            return ids.Select((long id, int idx) =>
            {
                var command = Instance.GetCommand(id);
                if (command == null) return -1;
                if (command.Condition.ExecuteExpression() is bool isValid)
                {
                    UnityEngine.Debug.Log($"isValid {(bool?)isValid}");
                    return isValid ? idx : -1;
                }
                return idx;
            }).Where((int v) => v >= 0);
        }

        public static IEnumerable<Model.Command> GetValidGlobalCommands()
        {
            return Instance.commands.Where((command) => command.Global > 0).Where((command) => {
                if (command.Condition.ExecuteExpression() is bool isValid)
                {
                    return isValid;
                }
                return true;
            });
        }

        public static IEnumerable<Model.Command> GetValidCommands(long[] ids)
        {
            return GetValidCommandIndex(ids).Select((idx) => Instance.GetCommand(ids[idx]));
        }

    }
}