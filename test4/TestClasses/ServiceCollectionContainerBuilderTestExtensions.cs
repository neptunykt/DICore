using DICore4;
using DICore4.Abstractions;
using DICore4.ServiceLookup;
using DICore4.ServiceLookup.Dynamic;
using DICore4.ServiceLookup.Expressions;
using DICore4.ServiceLookup.Runtime;

namespace test4.TestClasses;

internal static class ServiceCollectionContainerBuilderTestExtensions
{
    public static ServiceProvider BuildServiceProvider(this IServiceCollection services, ServiceProviderMode mode)
    {

        if (mode == ServiceProviderMode.Default)
        {
            return services.BuildServiceProvider();
        }

        var provider = new ServiceProvider(services);
        ServiceProviderEngine engine = mode switch
        {
            ServiceProviderMode.Dynamic => new DynamicServiceProviderEngine(provider),
            ServiceProviderMode.Runtime => RuntimeServiceProviderEngine.Instance,
            ServiceProviderMode.Expressions => new ExpressionsServiceProviderEngine(provider),
            _ => throw new NotSupportedException()
        };
        provider._engine = engine;
        return provider;
    }
}