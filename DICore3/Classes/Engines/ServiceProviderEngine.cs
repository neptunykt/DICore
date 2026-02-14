namespace DICore3.Classes.Engines;

public abstract class ServiceProviderEngine
{
    public abstract Func<ServiceProviderEngineScope, object> Realize(ServiceCallSite callSite);
}