using System.Reflection;

namespace DICore3.Classes;

public class ConstructorCallSite : ServiceCallSite
{
    public ConstructorInfo ConstructorInfo { get; set; }
    public ServiceCallSite[] ParameterCallSites { get; set; }
    public override Type ServiceType { get; set; }
    public override Type ImplementationType { get; set; }
}