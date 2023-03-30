[FieldResolverHandler("TimeYear")]
public class TimeYear : FieldResolver
{
    public override object Resolve()
    {
        return LifeEngine.Instance.lifeTime?.Time.Year;
    }
}

[FieldResolverHandler("TimeMonth")]
public class TimeMonth : FieldResolver
{
    public override object Resolve()
    {
        return LifeEngine.Instance.lifeTime?.Time.Month;
    }
}