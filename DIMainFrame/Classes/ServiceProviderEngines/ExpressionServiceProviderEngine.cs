using DIMainFrame.Classes.Visitors;

namespace DIMainFrame.Classes.ServiceProviderEngines;

public abstract class CompiledServiceProviderEngine : ServiceProviderEngine
{

    public ExpressionResolverBuilder ResolverBuilder { get; }


    // [RequiresDynamicCode("Creates DynamicMethods")]
    public CompiledServiceProviderEngine(ServiceProvider provider)
    {
        ResolverBuilder = new(provider);
    }

    public override Func<ServiceProviderEngineScope, object> Realize(ServiceCallSite callSite) => ResolverBuilder.Build(callSite);
    
}