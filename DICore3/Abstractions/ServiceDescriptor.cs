namespace DICore3.Abstractions;

public class ServiceDescriptor
{
    public ServiceLifetime Lifetime { get; set; }
    public Type ServiceType { get; set; }
    public Type ImplementationType { get; set; }

    public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
    }
    
    private object? _implementationInstance;
    
    public object? ImplementationInstance => _implementationInstance;
    
    private object? _implementationFactory;
    
    /// <summary>
    /// Gets the factory used for creating service instance
    /// </summary>
    /// <remarks>
    /// </remarks>
    public Func<IServiceProvider, object>? ImplementationFactory => (Func<IServiceProvider, object>?) _implementationFactory;
        
}