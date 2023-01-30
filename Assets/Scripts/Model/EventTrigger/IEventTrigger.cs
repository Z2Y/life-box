using Model;
using System.Threading.Tasks;
public interface IEventTrigger {
    Event GetEvent();
}

public static class EventTriggerHelper {
    public static async Task<EventNode> Trigger(this IEventTrigger trigger)
    {
        LifeNode lifeNode = LifeEngine.Instance?.lifeData?.current;
        if (lifeNode == null) return null;
        EventNode eventNode = lifeNode.AddEventByTrigger(trigger);
        await lifeNode.ProcessEvent();
        if (eventNode == null && lifeNode.ProcessedCount > 0) {
            eventNode = lifeNode.Events[lifeNode.ProcessedCount - 1];
        }
        return eventNode;
    }
}