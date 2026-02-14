using System.Reflection;

namespace DICore3.Classes;

public class ConstructorCallSite : ServiceCallSite
{
    public ConstructorInfo ConstructorInfo { get; set; }
    public ServiceCallSite[] ParameterCallSites { get; set; }
}