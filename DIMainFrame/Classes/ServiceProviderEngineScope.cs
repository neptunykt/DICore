using System.Collections.Concurrent;

namespace DIMainFrame.Classes;

public class ServiceProviderEngineScope : IDisposable
{
    public bool IsRootScope { get; set; }
    public ServiceProvider Root;
    public string Name { get; set; }
    public ServiceProviderEngineScope(ServiceProvider serviceProvider)
    {
        Root = serviceProvider;
    }
    public bool IsRoot { get; set; }
    public ConcurrentDictionary<ServiceCallSite, object> ResolvedServices { get; } 
        = new ConcurrentDictionary<ServiceCallSite, object>();
    public void Dispose()
    {
        foreach (var service in ResolvedServices.Values)
        {
            (service as IDisposable)?.Dispose();
        }
    }

    public object? GetService(ServiceIdentifier serviceIdentifier)
    {
        return Root.GetService(serviceIdentifier, this);
    }
}