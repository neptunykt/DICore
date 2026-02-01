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
    /// Get the key of the service, if applicable.
    /// </summary>
    public object? ServiceKey { get; }
    
    /// <summary>
    /// Gets the factory used for creating service instance
    /// </summary>
    /// <remarks>
    /// </remarks>
    public Func<IServiceProvider, object>? ImplementationFactory => (Func<IServiceProvider, object>?) _implementationFactory;
    
    
    /// <summary>
    /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="instance"/>
    /// as a <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the service.</param>
    /// <param name="instance">The instance implementing the service.</param>
    public ServiceDescriptor(
        Type serviceType,
        object? serviceKey,
        object instance)
        : this(serviceType, serviceKey, ServiceLifetime.Singleton)
    {
        _implementationInstance = instance;
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="factory"/>.
    /// </summary>
    /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
    /// <param name="factory">A factory used for creating service instances.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
    public ServiceDescriptor(
        Type serviceType,
        Func<IServiceProvider, object> factory,
        ServiceLifetime lifetime)
        : this(serviceType, serviceKey: null, lifetime)
    {
        
        _implementationFactory = factory;
    }
    
    private ServiceDescriptor(Type serviceType, object? serviceKey, ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
        ServiceType = serviceType;
        ServiceKey = serviceKey;
    }
        
}