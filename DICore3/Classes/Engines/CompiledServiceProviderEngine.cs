using DICore3.Classes.Visitors;

namespace DICore3.Classes.Engines;

public abstract class CompiledServiceProviderEngine : ServiceProviderEngine
{

    public ExpressionResolverBuilder ResolverBuilder { get; }

    

    public override Func<ServiceProviderEngineScope, object> Realize(ServiceCallSite callSite) => ResolverBuilder.Build(callSite);
    
}