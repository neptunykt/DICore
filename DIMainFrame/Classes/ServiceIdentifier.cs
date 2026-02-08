namespace DIMainFrame.Classes;

public class ServiceIdentifier : IEquatable<ServiceIdentifier>
{
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
}