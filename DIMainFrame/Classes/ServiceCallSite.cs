namespace DIMainFrame.Classes;

public abstract class ServiceCallSite
{
    public string Name { get; set; }
    public ServiceLifetime Lifetime { get; set; }
    public CallSiteKind Kind { get; set; }
    public abstract Type ServiceType { get; set; }
    public abstract Type ImplementationType { get; set; }
    // Используется для кэширования Singleton-объектов
    public volatile object? Value; 
  
}