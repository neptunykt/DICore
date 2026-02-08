namespace DICore3.Classes.Engines;

public abstract class ServiceProviderEngine
{
    // Замыкаем CallSite
    public abstract Func<ServiceProviderEngineScope, object> Realize(ServiceCallSite callSite);
}