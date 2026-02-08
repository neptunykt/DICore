using DICore3.Classes.Visitors;

namespace DICore3.Classes.Engines;

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