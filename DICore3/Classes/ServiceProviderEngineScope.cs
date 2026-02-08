using System.Collections.Concurrent;
using DICore3.Abstractions;
using IServiceProvider = DICore3.Abstractions.IServiceProvider;


namespace DICore3.Classes;

public class ServiceProviderEngineScope : IServiceProvider , IServiceScope, IServiceScopeFactory, IDisposable
{
    public bool IsRootScope { get; set; }
    public ServiceProvider RootProvider;
    public IServiceProvider ServiceProvider => this;
    public string Name { get; set; }
    public ServiceProviderEngineScope(ServiceProvider serviceProvider, bool isRootScope)
    {
        RootProvider = serviceProvider;
        IsRootScope = isRootScope;
    }
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
        return RootProvider.GetService(serviceIdentifier, this);
    }

    public object? GetService(Type serviceType)
    {
        return RootProvider.GetService(new ServiceIdentifier(serviceType), this);
    }

    public IServiceScope CreateScope()
    {
        return RootProvider.CreateScope();
    }
    
}