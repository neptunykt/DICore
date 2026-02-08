using DICore3.Abstractions;
using DICore3.Classes;

namespace DICore3.Extensions;

public static class ServiceCollectionContainerBuilderExtensions
{
    public static ServiceProvider BuildServiceProvider(this IServiceCollection services)
    {
        return new ServiceProvider(services);
    }
}