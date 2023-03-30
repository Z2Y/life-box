using System.Collections.Generic;
using Model;

namespace Logic.Clan
{
    public class NPCRelationManager : Singleton<NPCRelationManager>
    {
        private readonly Dictionary<KeyValuePair<long, long>, ClanNPCRelation> lookup = new ();

        public void UpdateNPCRelation(long characterID, long clanID, long value)
        {
            if (lookup.TryGetValue(new KeyValuePair<long, long>(characterID, clanID), out var entry))
            {
                entry.Relation = value;
            }
            else
            {
                lookup.Add(new KeyValuePair<long, long>(characterID, clanID), new ClanNPCRelation()
                {
                    CharacterID = characterID,
                    ClanID = clanID,
                    Relation = value
                });
            }
        }

        public void UpdateNPCRelationTitle(long characterID, long clanID, long titleID)
        {
            if (lookup.TryGetValue(new KeyValuePair<long, long>(characterID, clanID), out var entry))
            {
                entry.RelationTitleID = titleID;
            }
            // add relation title without relation is not allowed
        }

        public ClanNPCRelation GetNpcRelation(long characterID, long clanID)
        {
            return lookup.TryGetValue(new KeyValuePair<long, long>(characterID, clanID), out var relation)
                ? relation
                : null;
        }
    }
}