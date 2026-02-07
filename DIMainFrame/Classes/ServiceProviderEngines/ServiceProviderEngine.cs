namespace DIMainFrame.Classes.ServiceProviderEngines;

public abstract class ServiceProviderEngine
{
    // Замыкаем CallSite
    public abstract Func<ServiceProviderEngineScope, object> Realize(ServiceCallSite callSite);
}