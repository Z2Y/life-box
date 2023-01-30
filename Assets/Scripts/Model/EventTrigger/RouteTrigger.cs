using Model;
using ModelContainer;
using System.Linq;

public class RouteTrigger : Singleton<RouteTrigger>, IEventTrigger
{

    public static Event routeEvent;

    public Event GetEvent()
    {
        if (routeEvent == null)
        {
            routeEvent = EventCollection.Instance.GetEventByType(EventType.Route).FirstOrDefault();
        }
        LifeNode lifenode = LifeEngine.Instance?.lifeData?.current;
        EventNode lastEvent = lifenode?.Events?.LastOrDefault();
        if (lastEvent != null && lastEvent.Event.EventType == EventType.Route) {
            lastEvent.DoEffect().Coroutine();
            return null;
        }
        return routeEvent;
    }
}