using DICore3.Abstractions;

namespace DICore3;

public static class ServiceCollectionContainerBuilderExtensions
{
    public static ServiceProvider BuildServiceProvider(this IServiceCollection services)
    {
        return new ServiceProvider(services);
    }
}