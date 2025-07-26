using DICore2.Abstractions;
using DICore2.Classes;

namespace DICore2.Extensions;

public static class ServiceCollectionServiceExtensions
{
    public static void AddScoped<TService, TImplementation>(this ServiceCollection services)
        where TService : class
        where TImplementation: class, TService
    {
        services.Add(new ServiceDescriptor( typeof(TService),typeof(TImplementation), ServiceLifetime.Scoped));
    }
    public static void AddTransient<TService, TImplementation>(this ServiceCollection services)
        where TService : class
        where TImplementation: class, TService
    {
        services.Add(new ServiceDescriptor( typeof(TService),typeof(TImplementation), ServiceLifetime.Transient));
    }
    public static void AddSingleton<TService, TImplementation>(this ServiceCollection services)
        where TService : class
        where TImplementation: class, TService
    {
        services.Add(new ServiceDescriptor( typeof(TService),typeof(TImplementation), ServiceLifetime.Singleton));
    }
}