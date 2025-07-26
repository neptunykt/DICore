namespace DICore2.Abstractions;

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
}