using System;
using System.Linq;
using Model;
using ModelContainer;

public abstract class PlaceResolver : FieldResolver
{
    public LifeNode CurrentLife
    {
        get
        {
            return LifeEngine.Instance?.lifeData.current;
        }
    }
    public Place CurrentPlace
    {
        get
        {
            return CurrentLife?.Place;
        }
    }
}

[FieldResolverHandler("NearbyPlaceCount")]
public class PlaceNearbyCount : PlaceResolver
{   
    public override object Resolve()
    {
        if (CurrentPlace == null) return 0;
        UnityEngine.Debug.Log(CurrentPlace.Child);
        return CurrentPlace.Child.Count((pid) => PlaceCollection.Instance.GetPlace(pid) != null);
    }
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

[FieldResolverHandler("ParentPlace")]
public class ParentPlace : PlaceResolver
{
    public override object Resolve()
    {
        if (CurrentPlace == null) return 0;
        return CurrentPlace.Parent;
    }
}