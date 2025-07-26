using DICore4.Abstractions;

namespace DICore4;

public static class ServiceCollectionContainerBuilderExtensions
{
    public static ServiceProvider BuildServiceProvider(this IServiceCollection services)
    {
        return new ServiceProvider(services);
    }
}