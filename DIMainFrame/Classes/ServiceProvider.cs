using System.Collections.Concurrent;
using DIMainFrame.Classes.ServiceProviderEngines;
using DIMainFrame.Classes.Visitors;

namespace DIMainFrame.Classes;

public class ServiceProvider
{
    private readonly ConcurrentDictionary<ServiceIdentifier, ServiceAccessor> _serviceAccessors = new ConcurrentDictionary<ServiceIdentifier, ServiceAccessor>();
    private readonly Func<ServiceIdentifier, ServiceAccessor> _createServiceAccessor;
    public readonly ServiceProviderEngineScope Root;
    internal ServiceProviderEngine _engine;

    public ServiceProvider()
    {
        _engine = CreateDynamicEngine();
        _createServiceAccessor = CreateServiceAccessor;
        Root = new ServiceProviderEngineScope(this);
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
        var serviceIdentifier = new ServiceIdentifier { ServiceType = callSite.ServiceType };

        // Ищем существующий аксессор
        if (_serviceAccessors.TryGetValue(serviceIdentifier, out var accessor))
        {
            // Главный момент: подменяем медленную функцию на скомпилированную быструю
            // Все последующие вызовы GetService будут использовать этот новый делегат
            accessor.RealizedService = fastDelegate;
            
            Console.WriteLine($"[DI] Аксессор для {callSite.ServiceType.Name} обновлен на скомпилированный код.");
        }
    }
}




