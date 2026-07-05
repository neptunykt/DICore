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
    // Список для отслеживания disposable объектов
    private readonly List<object> _disposables = new List<object>();
    private readonly object _disposablesLock = new object();
    private bool _disposed;
    
    /// <summary>
    /// Добавляет объект в список для отслеживания disposable.
    /// Возвращает тот же объект для удобства цепочки вызовов.
    /// </summary>
    public object CaptureDisposable(object service)
    {
        if (service == null) return null;
        
        // Проверяем, реализует ли объект IDisposable или IAsyncDisposable
        if (service is IDisposable || service is IAsyncDisposable)
        {
            lock (_disposablesLock)
            {
                if (!_disposed)
                {
                    _disposables.Add(service);
                }
            }
        }
        return service;
    }
    
    public void Dispose()
    {
        lock (_disposablesLock)
        {
            if (_disposed) return;
            _disposed = true;

            // Диспозим в обратном порядке (как в оригинале MS DI)
            for (int i = _disposables.Count - 1; i >= 0; i--)
            {
                var disposable = _disposables[i];
                if (disposable is IDisposable d)
                {
                    d.Dispose();
                }
            }
            _disposables.Clear();
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