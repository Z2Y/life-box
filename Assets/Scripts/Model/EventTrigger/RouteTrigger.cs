using Model;
using ModelContainer;
using System.Linq;
using Cysharp.Threading.Tasks;

public class RouteTrigger : Singleton<RouteTrigger>, IEventTrigger
{

    public static Event routeEvent;

    // ReSharper disable Unity.PerformanceAnalysis
    public Event GetEvent()
    {
        if (routeEvent == null)
        {
            routeEvent = EventCollection.GetEventByType(EventType.Route).FirstOrDefault();
        }
        LifeNode lifenode = LifeEngine.Instance.lifeData?.current;
        EventNode lastEvent = lifenode?.Events?.LastOrDefault();
        if (lastEvent != null && lastEvent.Event.EventType == (int)EventType.Route) {
            lastEvent.DoEffect().Forget();
            return null;
        }
        return routeEvent;
    }
}