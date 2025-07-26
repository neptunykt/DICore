namespace DICore4.ServiceLookup;

internal abstract class ServiceCallSite
{
    protected ServiceCallSite(ResultCache cache)
    {
        Cache = cache;
    }

    public abstract Type ServiceType { get; }
    public abstract Type ImplementationType { get; }
    public abstract CallSiteKind Kind { get; }
    public ResultCache Cache { get; }
    public object Value { get; set; }

    public bool CaptureDisposable =>
        ImplementationType == null ||
        typeof(IDisposable).IsAssignableFrom(ImplementationType);
}