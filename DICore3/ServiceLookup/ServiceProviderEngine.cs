namespace DICore3.ServiceLookup;

internal abstract class ServiceProviderEngine
{
    public abstract Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite);
}