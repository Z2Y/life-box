using Cathei.LinqGen;
using Model;
using ModelContainer;

public class ShopTrigger : Singleton<ShopTrigger>, IEventTrigger
{

    public static Event shopEvent;

    public ShopConfig Config {get; private set;}

    public ShopTrigger WithConfig(ShopConfig config)
    {
        Config = config;
        return this;
    }

    public Event GetEvent()
    {
        if (shopEvent == null)
        {
            shopEvent = EventCollection.GetEventByType(EventType.Shop).Gen().FirstOrDefault();
        }
        var node = LifeEngine.Instance.lifeData?.current;
        var lastEvent = node?.Events?.Gen().LastOrDefault();
        if (lastEvent != null && lastEvent.Event.EventType == (int)EventType.Shop) {
            return (lastEvent.EffectResult is not ShopResult lastShopResult || lastShopResult.Sells.Config != Config) ? shopEvent : null;
        }
        return shopEvent;
    }
}