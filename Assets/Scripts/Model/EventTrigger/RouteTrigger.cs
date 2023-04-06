using Cathei.LinqGen;
using Model;
using ModelContainer;
using Cysharp.Threading.Tasks;

public class RouteTrigger : Singleton<RouteTrigger>, IEventTrigger
{

    public static Event routeEvent;

    // ReSharper disable Unity.PerformanceAnalysis
    public Event GetEvent()
    {
        if (routeEvent == null)
        {
            routeEvent = EventCollection.GetEventByType(EventType.Route).Gen().FirstOrDefault();
        }
        var node = LifeEngine.Instance.lifeData?.current;
        var lastEvent = node?.Events?.Gen().LastOrDefault();
        if (lastEvent != null && lastEvent.Event.EventType == (int)EventType.Route) {
            lastEvent.DoEffect().Forget();
            return null;
        }
        return routeEvent;
    }
}