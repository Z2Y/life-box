using Model;
using ModelContainer;
using System.Linq;

public interface ICharacterState {
    bool IsState(Character character);
}

public class Talkable : Singleton<Talkable>, ICharacterState {
    public bool IsState(Character character) {
        if (character == null) return false;
        var trigger = TalkTriggerContainer.GetTalkConfig(character.ID);
        var relation = 0;
        return trigger != null && trigger.Event.Count > 0 && relation >= trigger.RelationLimit;
    }
}

public class Shopable : Singleton<Shopable>, ICharacterState {
    public bool IsState(Character character) {
        if (character == null) return false;
        var config = ShopConfigCollection.FirstConfigOfCharacter(character.ID);
        return config != null && config.Item.Count > 0;
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