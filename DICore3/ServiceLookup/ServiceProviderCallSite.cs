namespace DICore3.ServiceLookup;

internal sealed class ServiceProviderCallSite : ServiceCallSite
{
    public ServiceProviderCallSite() : base(ResultCache.None(typeof(IServiceProvider)))
    {
    }

    public override Type ServiceType { get; } = typeof(IServiceProvider);
    public override Type ImplementationType { get; } = typeof(ServiceProvider);
    public override CallSiteKind Kind { get; } = CallSiteKind.ServiceProvider;
}