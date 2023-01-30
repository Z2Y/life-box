using Model;
using ModelContainer;
using System.Linq;

public interface ICharacterState {
    bool IsState(Character character);
}

public class Talkable : Singleton<Talkable>, ICharacterState {
    public bool IsState(Character character) {
        if (character == null) return false;
        TalkTrigger trigger = TalkTriggerContainer.Instance.GetTrigger(character.ID);
        long relation = 0;
        UnityEngine.Debug.Log($"{character.ID} {relation}");
        character.Relations.TryGetValue(0, out relation); // 0 for player
        return trigger != null && trigger.Event.Length > 0 && relation >= trigger.RelationLimit;
    }
}

public class Shopable : Singleton<Shopable>, ICharacterState {
    public bool IsState(Character character) {
        if (character == null) return false;
        ShopConfig config = ShopConfigCollection.Instance.GetShopConfigsByCharacter(character.ID)?.FirstOrDefault();
        UnityEngine.Debug.Log($"{character.ID} {config}");
        return config != null && config.Item.Length > 0;
    }
}

public static class CharacterStateHelper {
    public static bool IsState(this Character character, string state) {
        switch(state) {
            case "Talkable":
                return character.IsTalkable();
            case "Shopable":
                return character.IsShopable();
            default:
                return false;
        }
    }
    public static bool IsTalkable(this Character character) {
        return Talkable.Instance.IsState(character);
    }

    public static bool IsShopable(this Character character) {
        return Shopable.Instance.IsState(character);
    }
}