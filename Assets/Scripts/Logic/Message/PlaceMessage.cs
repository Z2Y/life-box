using System;
using Utils;

namespace Logic.Message
{
    public abstract class PlaceMessage : PoolObject, IDisposable
    {
        public long mapID;
        public long placeID;
    }

    public class EnterPlace : PlaceMessage { }

    public class LeavePlace : PlaceMessage { }
    
    public class BattleComplete : PlaceMessage { }
}