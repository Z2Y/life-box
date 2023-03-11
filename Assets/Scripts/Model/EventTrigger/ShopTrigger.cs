using Model;
using ModelContainer;
using System.Linq;

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
            shopEvent = EventCollection.Instance.GetEventByType(EventType.Shop).FirstOrDefault();
        }
        LifeNode lifenode = LifeEngine.Instance.lifeData?.current;
        EventNode lastEvent = lifenode?.Events?.LastOrDefault();
        if (lastEvent != null && lastEvent.Event.EventType == EventType.Shop) {
            ShopResult lastShopResult = lastEvent.EffectResult as ShopResult;
            return (lastShopResult == null || lastShopResult.Sells.Config != Config) ? shopEvent : null;
        }
        return shopEvent;
    }
}