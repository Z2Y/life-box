using Logic.Map;
using Model;
using UnityEngine;

public abstract class PlaceResolver : FieldResolver
{
    private LifeNode CurrentLife => LifeEngine.Instance.lifeData.current;

    protected Place CurrentPlace => CurrentLife?.Place;
}

[FieldResolverHandler("CurrentPlace")]
public class CurrentPlace : PlaceResolver
{
    public override object Resolve()
    {
        if (CurrentPlace == null) return 0;
        return CurrentPlace.ID;
    }
}

[FieldResolverHandler("CurrentPlaceType")]
public class CurrentPlaceType : PlaceResolver
{
    public override object Resolve()
    {
        if (CurrentPlace == null) return 0;
        return CurrentPlace.PlaceType;
    }
}

[FieldResolverHandler("CurrentBattleDepth")]
public class CurrentBattleDepth : PlaceResolver
{
    public override object Resolve()
    {
        var curPace = LifeEngine.Instance.Place;
        if (curPace == null) return 0;
        Debug.Log($"Current Battle Depth {curPace.GetComponent<BattlePlaceController>()?.battleDepth ?? 0}");
        return curPace.GetComponent<BattlePlaceController>()?.battleDepth ?? 0;
    }
}

