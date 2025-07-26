namespace DICore3.ServiceLookup;

internal sealed class FactoryCallSite : ServiceCallSite
{
    public Func<IServiceProvider, object> Factory { get; }
    public FactoryCallSite(ResultCache cache, Type serviceType, Func<IServiceProvider, object> factory) : base(cache)
    {
        Factory = factory;
        ServiceType = serviceType;
    }

    public override Type ServiceType { get; }
    public override Type ImplementationType { get; }
    public override CallSiteKind Kind { get; }
}