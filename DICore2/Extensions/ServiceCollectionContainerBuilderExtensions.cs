using DICore2.Abstractions;
using DICore2.Classes;

namespace DICore2.Extensions;

public static class ServiceCollectionContainerBuilderExtensions
{
    public static ServiceProvider BuildServiceProvider(this IServiceCollection services)
    {
        return new ServiceProvider(services);
    }
}