using System;

[FieldResolverHandler("Event")]
public class EventResolver : FieldResolver {
    public override object Resolve()
    {
        return LifeEngine.Instance.lifeData?.Events;
    }
}