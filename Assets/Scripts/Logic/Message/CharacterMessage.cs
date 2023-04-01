using System;
using Utils;

namespace Logic.Message
{
    public abstract class CharacterMessage : PoolObject, IDisposable
    {
        public long characterID;
    }
    
    public class CharacterDeath : CharacterMessage {}
}