namespace DICore4.ServiceLookup.Expressions;

internal sealed class ExpressionsServiceProviderEngine : ServiceProviderEngine
{
    private readonly ExpressionResolverBuilder _expressionResolverBuilder;

    public ExpressionsServiceProviderEngine(ServiceProvider serviceProvider)
    {
        _expressionResolverBuilder = new ExpressionResolverBuilder(serviceProvider);
    }

    public override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite)
    {
        return _expressionResolverBuilder.Build(callSite);
    }
}