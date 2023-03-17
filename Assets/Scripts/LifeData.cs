using System.Threading.Tasks;
using ModelContainer;
using System.Collections.Generic;
using Event = Model.Event;

public class LifeData
{
    public LifeNode current;
    public LifeProperty property;
    public ItemInventory knapsackInventory = new (40);
    public MoneyInventory moneyInventory = new ();
    public Dictionary<long, long> Events = new (); // 所有经历过的事件

    public void DoForecast(LifeTime time)
    {
        current.Forecast(TimeTriggerContainer.Instance.GetTrigger(time.Time.Next()));
    }

    public async Task DoNext()
    {
        current = current.Next;
        await current.ProcessEvent();
    }

    public static LifeData CreateNew()
    {
        var life = new LifeData
        {
            current = LifeNode.CreateBornNode(),
            property = LifePropertyFactory.Random(40)
        };
        life.moneyInventory.BindToWealth(life.property.GetProperty(SubPropertyType.Wealth));
        return life;
    }

    public void AddNodeEvent(Event evt)
    {
        if (!Events.ContainsKey(evt.ID))
        {
            Events.Add(evt.ID, evt.ID);
        }
    }
}