using System;
using System.Threading.Tasks;
using Model;
using ModelContainer;
using System.Collections.Generic;

public class LifeData
{
    public LifeNode current;
    public LifeProperty property;
    public ItemInventory knapsackInventory = new ItemInventory(40);
    public MoneyInventory moneyInventory = new MoneyInventory();
    public Dictionary<long, long> Events = new Dictionary<long, long>(); // 所有经历过的事件

    public void DoForcast(LifeTime time)
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
        LifeData life = new LifeData();
        life.current = LifeNode.CreateBornNode();
        life.property = LifePropertyFactory.Ramdom(40);
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