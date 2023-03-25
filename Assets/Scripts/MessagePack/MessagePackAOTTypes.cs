using System;
using System.Collections.Generic;
using MessagePack;

[MessagePackObject(true)]
[Serializable]
public class MessagePackAOTTypes
{
    public List<Model.Command> _Commands;
    public List<Model.Item> _Items;
    public List<Model.Event> _Events;
    public List<Model.ShopConfig> _ShopConfigs;
    public List<Model.Character> _Characters;
    public List<Model.Skill> _Skills;
    public List<Model.Place> _Places;
    public List<Model.Quest> _Quests;
    public List<Model.TimeTrigger> _TimeTriggers;
    public List<Model.InteractMenuConfig> _InteractMenuConfigs;
    public List<Model.TalkTrigger> _TalkTriggers;
}
