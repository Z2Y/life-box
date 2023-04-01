using System;
using UnityEngine;
using Utils;

namespace Logic.Message
{
    public abstract class MapMessage : PoolObject, IDisposable
    {
        public long mapID;
    }

    public class EnterMap : MapMessage {
        
    }
}