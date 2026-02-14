using DICore3.Abstractions;

namespace DICore3.Classes;

public abstract class ServiceCallSite
{
    public ServiceLifetime Lifetime { get; set; }
    public CallSiteKind Kind { get; set; }
    public Type ServiceType { get; set; }
    public Type ImplementationType { get; set; }
    // Используется для кэширования Singleton-объектов
    public volatile object? Value; 
}