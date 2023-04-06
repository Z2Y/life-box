using UnityEngine.Events;
using ModelContainer;
using System.Collections.Generic;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LifeNode
{
    public Model.Place Place;
    public readonly List<EventNode> Events = new ();
    public LifeNode Next;
    public LifeNode Prev;
    public Location Location;
    private readonly Dictionary<string, object> environment = new ();

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
        var evt = trigger?.GetEvent();
        if (evt == null) return null;
        var node = new EventNode(this, evt);
        Events.Add(node);
        return node;
    }

    public Dictionary<string, object> Environment
    {
        get
        {
            environment["$Place"] = Place?.Name;
            environment["$Age"] = 18;
            return environment;
        }
    }

    public async UniTask ProcessEvent()
    {
        // EventNode last = ProcessedCount > 0 ? Events[ProcessedCount - 1] : null;
        Processing = true;
        for (var i = ProcessedCount; i < Events.Count; i++)
        {
            var e = Events[i];
            Debug.Log($"Process Event {e.Event.ID} {e.Event.Effect}");
            LifeEngine.Instance.lifeData.AddNodeEvent(e.Event);
            await e.DoEffect();
            var branch = await e.DoBranch();
            if (branch >= 0 && branch < e.Event.Branch.Count)
            {
                Debug.Log($"EventBranch {branch} {e.Event.Branch[branch]} {EventCollection.GetEvent(e.Event.Branch[branch])}");
                var branchEvent = EventCollection.GetEvent(e.Event.Branch[branch]);
                if (branchEvent != null)
                {
                    Events.Insert(i + 1, new EventNode(this, branchEvent));
                }

            }
            ProcessedCount = i + 1;
        }
        Processing = false;
        OnLifeNodeChange?.Invoke();
    }

    public string Description
    {
        get { return Events.Count <= 0 ? "无事发生" : string.Join(" ", Events.Gen().Select(e => e.Description)); }
    }

    public bool IsDeath
    {
        get
        {
            return Events.Exists(e => e.Event.EventType == (int)Model.EventType.Death);
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
        var bornPlace = PlaceCollection.GetPlace(10002);
        node.Place = bornPlace;
        node.Location = new Location() {MapID = bornPlace.MapID, PlaceID = bornPlace.ID};
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