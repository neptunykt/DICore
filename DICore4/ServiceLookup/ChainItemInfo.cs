namespace DICore4.ServiceLookup;

public struct ChainItemInfo
{
    public int Order { get; }
    public Type ImplementationType { get; }

    public ChainItemInfo(int order, Type implementationType)
    {
        Order = order;
        ImplementationType = implementationType;
    }
}