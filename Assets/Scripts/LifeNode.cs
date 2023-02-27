using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;
using ModelContainer;
using System.Collections.Generic;
using UnityEngine;

public class LifeNode
{
    public Model.Place Place;
    public List<EventNode> Events = new ();
    public LifeNode Next;
    public LifeNode Prev;
    public Location Location;
    private Dictionary<string, object> enviroments = new ();

    public LifeNodeEvent OnLifeNodeChange = new ();

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
                Debug.Log($"EventBranch {branch} {e.Event.Branch[branch]} {EventCollection.Instance.GetEvent(e.Event.Branch[branch])}");
                Model.Event branchEvent = EventCollection.Instance.GetEvent(e.Event.Branch[branch]);
                if (branchEvent != null)
                {
                    Events.Insert(i + 1, new EventNode(this, branchEvent));
                }

            }
            Debug.Log($"EventBranch {branch}");
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
        var node = new LifeNode();
        var bornPlace = PlaceCollection.Instance.RandomPlace(Model.PlaceType.City);
        var bornEvent = TimeTriggerContainer.Instance.GetTrigger(new TimeSpan(0, 0)).GetEvent();
        node.Place = bornPlace;
        node.Location = new Location() {MapID = bornPlace.MapID, PlaceID = bornPlace.ID};
        node.Events.Add(new EventNode(node, bornEvent));
        return node;
    }
}

public struct Location
{
    public long MapID;
    public long PlaceID;
    public Vector3 Position;
}

public class LifeNodeEvent : UnityEvent { }