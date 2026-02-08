using System.Collections.Concurrent;
using DICore3.Abstractions;
using DICore3.Classes.Engines;
using DICore3.Classes.Visitors;
using IServiceProvider = DICore3.Abstractions.IServiceProvider;


namespace DICore3.Classes;

public class ServiceProvider : IServiceProvider, IServiceScopeFactory
{
    private readonly ConcurrentDictionary<ServiceIdentifier, ServiceAccessor> _serviceAccessors = new ConcurrentDictionary<ServiceIdentifier, ServiceAccessor>();
    private readonly Func<ServiceIdentifier, ServiceAccessor> _createServiceAccessor;
    public readonly ServiceProviderEngineScope Root;
    public CallSiteFactory CallSiteFactory { get; }
    internal ServiceProviderEngine _engine;

    public ServiceProvider(IServiceCollection services)
    {
        _engine = CreateDynamicEngine();
        _createServiceAccessor = CreateServiceAccessor;
        CallSiteFactory = new CallSiteFactory(services);
        Root = new ServiceProviderEngineScope(this, true );
    }
    
    private sealed class ServiceAccessor
    {
        public ServiceCallSite CallSite { get; set; }
        public Func<ServiceProviderEngineScope, object> RealizedService { get; set; }
    }

    public object? GetService(ServiceIdentifier serviceIdentifier, ServiceProviderEngineScope scope)
    {
        var serviceAccessor = _serviceAccessors.GetOrAdd(serviceIdentifier, _createServiceAccessor);
        // Вызовется метод
        return serviceAccessor.RealizedService.Invoke(scope);
    }
    
    private ServiceAccessor CreateServiceAccessor(ServiceIdentifier serviceIdentifier)
    {
        // Получаем callSite из фабрики
        var callSite = CallSiteFactory.GetCallSite(serviceIdentifier);
        if (callSite != null)
        {
            if (callSite.Lifetime == ServiceLifetime.Singleton)
            {
             // Вызываем создание напрямую в обход счетчика
             return new ServiceAccessor
                 { CallSite = callSite, RealizedService = _ =>  new CallSiteRuntimeResolver().VisitCallSite(callSite, Root) };
            }
            var realizedService =  _engine.Realize(callSite);
          return new ServiceAccessor { CallSite = callSite, RealizedService = realizedService };
        }
        return new ServiceAccessor { CallSite = callSite, RealizedService = _ => null };

    }
    
    ServiceProviderEngine CreateDynamicEngine() => new DynamicServiceProviderEngine(this);
    internal void ReplaceServiceAccessor(ServiceCallSite callSite, Func<ServiceProviderEngineScope, object?> fastDelegate)
    {
        // Создаем идентификатор, по которому сервис хранится в словаре
        var serviceIdentifier = new ServiceIdentifier(callSite.ServiceType);

        // Ищем существующий аксессор
        if (_serviceAccessors.TryGetValue(serviceIdentifier, out var accessor))
        {
            // Главный момент: подменяем медленную функцию на скомпилированную быструю
            // Все последующие вызовы GetService будут использовать этот новый делегат
            accessor.RealizedService = fastDelegate;
            
            Console.WriteLine($"[DI] Аксессор для {callSite.ServiceType.Name} обновлен на скомпилированный код.");
        }
    }

    public object? GetService(Type serviceType)
    {
        return Root.GetService(new ServiceIdentifier(serviceType));
    }

    public IServiceScope CreateScope()
    {
        return new ServiceProviderEngineScope(this, isRootScope: false);
    }
}