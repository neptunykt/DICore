using DICore2.Abstractions;
using IServiceProvider = DICore2.Abstractions.IServiceProvider;

namespace DICore2.Classes;

public class ServiceProviderEngineScope : IServiceScope, IServiceProvider, IServiceScopeFactory
{
    internal ServiceProviderEngineScope(ServiceProvider serviceProvider, bool isRootScope)
    {
        IsRootScope = isRootScope;
        RootProvider = serviceProvider;
        ServiceProvider = this;
    }

    /// <summary>
    /// Хранилище объектов в скоупе
    /// </summary>
    internal Dictionary<ServiceDescriptor, object> ResolvedServices { get; } =
        new Dictionary<ServiceDescriptor, object?>();

    public IServiceProvider ServiceProvider { get; }

    internal bool IsRootScope { get; }

    internal ServiceProvider RootProvider { get; }

    public object? GetService(Type serviceType)
    {
        return RootProvider.GetService(serviceType, this);
    }

    public IServiceScope CreateScope()
    {
        return RootProvider.CreateScope();
    }
    
}