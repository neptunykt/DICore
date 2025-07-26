using System.Collections.Concurrent;
using DICore4.Abstractions;
using DICore4.ServiceLookup;
using DICore4.ServiceLookup.Dynamic;
using DICore4.ServiceLookup.Runtime;

namespace DICore4;

public class ServiceProvider : IServiceProvider
{
    internal ServiceProviderEngineScope Root { get; }
    private bool _disposed;
    internal CallSiteFactory CallSiteFactory { get; }
    internal ServiceProviderEngine _engine;
    private readonly Func<ServiceIdentifier, ServiceAccessor> _createServiceAccessor;
    private readonly ConcurrentDictionary<ServiceIdentifier, ServiceAccessor> _serviceAccessors;
    internal static readonly bool s_allowNonKeyedServiceInject = true;

    internal ServiceProvider(ICollection<ServiceDescriptor> serviceDescriptors)
    {
        Root = new ServiceProviderEngineScope(this, isRootScope: true);
        _engine = CreateDynamicEngine();
        _createServiceAccessor = CreateServiceAccessor;
        _serviceAccessors = new ConcurrentDictionary<ServiceIdentifier, ServiceAccessor>();
        CallSiteFactory = new CallSiteFactory(serviceDescriptors);
        CallSiteFactory.Add(ServiceIdentifier.FromServiceType(typeof(IServiceProvider)), new ServiceProviderCallSite());
        CallSiteFactory.Add(ServiceIdentifier.FromServiceType(typeof(IServiceScopeFactory)), new ConstantCallSite(typeof(IServiceScopeFactory), Root));
        CallSiteFactory.Add(ServiceIdentifier.FromServiceType(typeof(IServiceProviderIsService)), new ConstantCallSite(typeof(IServiceProviderIsService), CallSiteFactory));
    }

    public object? GetService(Type serviceType)
    {
        return GetService(ServiceIdentifier.FromServiceType(serviceType), Root);
    }

    internal object? GetService(ServiceIdentifier serviceIdentifier, ServiceProviderEngineScope serviceProviderEngineScope)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException("ThrowHelper.ThrowObjectDisposedException())");
        }
        ServiceAccessor serviceAccessor = _serviceAccessors.GetOrAdd(serviceIdentifier, _createServiceAccessor);
        //  OnResolve(serviceAccessor.CallSite, serviceProviderEngineScope);
        //  DependencyInjectionEventSource.Log.ServiceResolved(this, serviceIdentifier.ServiceType);
        object? result = serviceAccessor.RealizedService?.Invoke(serviceProviderEngineScope);
        //  System.Diagnostics.Debug.Assert(result is null || CallSiteFactory.IsService(serviceIdentifier));
        return result;
    }

    private ServiceAccessor CreateServiceAccessor(ServiceIdentifier serviceIdentifier)
    {
        ServiceCallSite? callSite = CallSiteFactory.GetCallSite(serviceIdentifier, new CallSiteChain());
        if (callSite != null)
        {
            // DependencyInjectionEventSource.Log.CallSiteBuilt(this, serviceIdentifier.ServiceType, callSite);
            //  OnCreate(callSite);
        
            // Optimize singleton case
            if (callSite.Cache.Location == CallSiteResultCacheLocation.Root)
            {
                object? value = CallSiteRuntimeResolver.Instance.Resolve(callSite, Root);
                return new ServiceAccessor { CallSite = callSite, RealizedService = scope => value };
            }

            Func<ServiceProviderEngineScope, object?> realizedService = _engine.RealizeService(callSite);
            return new ServiceAccessor { CallSite = callSite, RealizedService = realizedService };
        }
        return new ServiceAccessor { CallSite = callSite, RealizedService = _ => null };
    }

    public IServiceScope CreateScope()
    {
        return new ServiceProviderEngineScope(this, isRootScope: false);
    }
    
    private sealed class ServiceAccessor
    {
        public ServiceCallSite? CallSite { get; set; }
        public Func<ServiceProviderEngineScope, object?> RealizedService { get; set; }
    }
    
    public void Dispose()
    {
        DisposeCore();
        Root.Dispose();
    }
    internal bool IsDisposed() => _disposed;
     
    private void DisposeCore()
    {
        _disposed = true;
    }
    
    internal void ReplaceServiceAccessor(ServiceCallSite callSite, Func<ServiceProviderEngineScope, object?> accessor)
    {
        _serviceAccessors[new ServiceIdentifier(callSite.ServiceType)] = new ServiceAccessor
        {
            CallSite = callSite,
            RealizedService = accessor
        };
    }
    ServiceProviderEngine CreateDynamicEngine() => new DynamicServiceProviderEngine(this);
    
}