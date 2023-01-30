using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;

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
            Model.Command value;
            if (lookup.TryGetValue(id, out value))
            {
                return value;
            }
            return null;
        }

        public static CommandCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CommandCollection();
                }
                return _instance;
            }
        }

        public static IEnumerable<int> GetValidCommandIndex(IEnumerable<long> ids)
        {
            return ids.Select((long id, int idx) =>
            {
                Model.Command command = Instance.GetCommand(id);
                if (command == null) return -1;
                var isValid = command.Condition.ExecuteExpression() as bool?;
                if (isValid != null)
                {
                    UnityEngine.Debug.Log($"isValid {isValid}");
                    return (bool)isValid ? idx : -1;
                }
                return idx;
            }).Where((int v) => v >= 0);
        }

        public static IEnumerable<Model.Command> GetValidGlobalCommands()
        {
            return Instance.commands.Where((command) => command.Global > 0).Where((command) => {
                var isValid = command.Condition.ExecuteExpression() as bool?;
                if (isValid != null)
                {
                    return (bool)isValid;
                }
                return true;
            });
        }

        public static IEnumerable<Model.Command> GetValidCommands(long[] ids)
        {
            List<int> indexs = GetValidCommandIndex(ids).ToList();
            return indexs.Select((idx) => Instance.GetCommand(ids[idx]));
        }

    }
}