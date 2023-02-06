using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;
using ModelContainer;
using System.Collections.Generic;

public class LifeNode
{
    public Model.Place Place;
    public List<EventNode> Events = new List<EventNode>();
    public LifeNode Next;
    public LifeNode Prev;
    private Dictionary<string, object> enviroments = new Dictionary<string, object>();

    public LifeNodeEvent OnLifeNodeChange = new LifeNodeEvent();

    public int ProcessedCount;

    public bool Processing { get; private set;}

    public void Forecast(IEventTrigger trigger)
    {
        Next = CreateEmptyNode();
        Next.AddEventByTrigger(trigger);
        Next.Place = Place;
        Next.Prev = this;
    }

    public EventNode AddEventByTrigger(IEventTrigger trigger)
    {
        Model.Event evt = trigger?.GetEvent();
        if (evt != null)
        {
            EventNode node = new EventNode(this, evt);
            Events.Add(node);
            return node;
        }
        return null;
    }

    public Dictionary<string, object> Enviroments
    {
        get
        {
            enviroments["$Place"] = Place?.Name;
            enviroments["$Age"] = 18;
            return enviroments;
        }
    }

    public async Task ProcessEvent()
    {
        EventNode last = ProcessedCount > 0 ? Events[ProcessedCount - 1] : null;
        Processing = true;
        for (int i = ProcessedCount; i < Events.Count; i++)
        {
            var e = Events[i];
            await e.DoEffect();
            int branch = await e.DoBranch();
            if (branch >= 0 && branch < e.Event.Branch.Length)
            {
                UnityEngine.Debug.Log($"EventBranch {branch} {e.Event.Branch[branch]} {EventCollection.Instance.GetEvent(e.Event.Branch[branch])}");
                Model.Event branchEvent = EventCollection.Instance.GetEvent(e.Event.Branch[branch]);
                if (branchEvent != null)
                {
                    Events.Insert(i + 1, new EventNode(this, branchEvent));
                }

            }
            UnityEngine.Debug.Log($"EventBranch {branch}");
            ProcessedCount = i + 1;
            LifeEngine.Instance.lifeData.AddNodeEvent(e.Event);
        }
        Processing = false;
        OnLifeNodeChange?.Invoke();
    }

    public string Description
    {
        get { return Events.Count <= 0 ? "无事发生" : string.Join(" ", Events.Select(e => e.Description)); }
    }

    public bool IsDeath
    {
        get
        {
            return Events.Exists(e => e.Event.EventType == Model.EventType.Death);
        }
    }

    public static LifeNode CreateEmptyNode()
    {
        LifeNode node = new LifeNode();
        return node;
    }

    public static LifeNode CreateBornNode()
    {
        LifeNode node = new LifeNode();
        Model.Place bornPlace = PlaceCollection.Instance.RamdomPlace(Model.PlaceType.City);
        Model.Event bornEvent = TimeTriggerContainer.Instance.GetTrigger(new TimeSpan(0, 0)).GetEvent();
        node.Place = bornPlace;
        node.Events.Add(new EventNode(node, bornEvent));
        return node;
    }
}

public class LifeNodeEvent : UnityEvent { }