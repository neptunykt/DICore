namespace DICore4.ServiceLookup.Expressions;

internal abstract class CompiledServiceProviderEngine : ServiceProviderEngine
{

    public ExpressionResolverBuilder ResolverBuilder { get; }


    // [RequiresDynamicCode("Creates DynamicMethods")]
    public CompiledServiceProviderEngine(ServiceProvider provider)
    {
        ResolverBuilder = new(provider);
    }

    public override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite) => ResolverBuilder.Build(callSite);
}