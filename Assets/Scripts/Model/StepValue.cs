using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;

namespace Model
{
    public enum StepValueType
    {
        Popularity = 0,
    }

    [MessagePackObject(true)]
    public class StepValue
    {
        public long ID;
        public StepValueType ValueType;
        public string Name;
        public int MinValue;
        public int MaxValue;
        public int[] StepPoint;

    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Model.StepValue), "values")]
    public class StepValueCollection
    {
        private Dictionary<long, Model.StepValue> lookup = new Dictionary<long, Model.StepValue>();
        private List<Model.StepValue> values = new List<Model.StepValue>();
        private static StepValueCollection _instance;
        private StepValueCollection() { }

        private void OnLoad()
        {
            lookup.Clear();
            foreach (Model.StepValue value in values)
            {
                lookup.Add(value.ID, value);
            }
        }

        public static StepValueCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StepValueCollection();
                }
                return _instance;
            }
        }

        public Model.StepValue GetStepValue(long id)
        {
            Model.StepValue value;
            if (lookup.TryGetValue(id, out value))
            {
                return value;
            }
            return null;
        }

        public List<Model.StepValue> StepValues
        {
            get
            {
                return values;
            }
        }
    }
}