using DICore3.Abstractions;

namespace DICore3.ServiceLookup;

internal readonly struct ServiceIdentifier: IEquatable<ServiceIdentifier>
{

    public Type ServiceType { get; }

    public ServiceIdentifier(Type serviceType)
    {
        ServiceType = serviceType;
    }
        

    public static ServiceIdentifier FromDescriptor(ServiceDescriptor serviceDescriptor)
        => new ServiceIdentifier(serviceDescriptor.ServiceType);

    public static ServiceIdentifier FromServiceType(Type type)
    {
        return new ServiceIdentifier(type);
    }

    public bool Equals(ServiceIdentifier other)
    {
        return ServiceType == other.ServiceType;
    }

    public override bool Equals(object obj)
    {
        return obj is ServiceIdentifier && Equals((ServiceIdentifier)obj);
    }

    public override int GetHashCode()
    {
          
        return ServiceType.GetHashCode();
           
    }
        
    public override string ToString()
    {
        return ServiceType.ToString();

    }
}