using DICore3.Abstractions;

namespace DICore3.Classes;

public class ServiceIdentifier: IEquatable<ServiceIdentifier>
{
    public ServiceIdentifier(Type serviceType)
    {
        ServiceType = serviceType;
    }
    
    public Type ServiceType { get; set; }
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
    
    public static ServiceIdentifier FromDescriptor(ServiceDescriptor serviceDescriptor)
        => new ServiceIdentifier(serviceDescriptor.ServiceType);

    public static ServiceIdentifier FromServiceType(Type type)
    {
        return new ServiceIdentifier(type);
    }
}