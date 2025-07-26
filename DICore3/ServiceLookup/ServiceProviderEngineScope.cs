using DICore3.Abstractions;

namespace DICore3.ServiceLookup;

internal sealed class ServiceProviderEngineScope : IServiceScope, IServiceProvider, IServiceScopeFactory
{
    public ServiceProviderEngineScope(ServiceProvider provider, bool isRootScope)
    {
        ResolvedServices = new Dictionary<ServiceCacheKey, object>();
        RootProvider = provider;
        IsRootScope = isRootScope;
    }

    /// <summary>
    /// Хранилище объектов в скоупе
    /// </summary>
    internal Dictionary<ServiceCacheKey, object?> ResolvedServices { get; }
    internal IList<object> Disposables => _disposables ?? (IList<object>)Array.Empty<object>();

    internal object Sync => ResolvedServices;
    private bool _disposed;
    private List<object> _disposables;

    public IServiceProvider ServiceProvider => this;

    public bool IsRootScope { get; }

    internal ServiceProvider RootProvider { get; }

    public object? GetService(Type serviceType)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException("ThrowHelper.ThrowObjectDisposedException()");
        }

        return RootProvider.GetService(ServiceIdentifier.FromServiceType(serviceType), this);
    }

    public IServiceScope CreateScope()
    {
        return RootProvider.CreateScope();
    }
    
            

        internal object CaptureDisposable(object service)
        {
            if (ReferenceEquals(this, service) || !(service is IDisposable ))
            {
                return service;
            }

            bool disposed = false;
            lock (Sync)
            {
                if (_disposed)
                {
                    disposed = true;
                }
                else
                {
                    _disposables ??= new List<object>();

                    _disposables.Add(service);
                }
            }

            // Don't run customer code under the lock
            if (disposed)
            {
                if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                throw new ObjectDisposedException("_disposed");
            }

            return service;
        }

        public void Dispose()
        {
            List<object> toDispose = BeginDispose();

            if (toDispose != null)
            {
                for (int i = toDispose.Count - 1; i >= 0; i--)
                {
                    if (toDispose[i] is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid operation");
                    }
                }
            }
        }
        

        private List<object> BeginDispose()
        {
            lock (Sync)
            {
                if (_disposed)
                {
                    return null;
                }

                // We've transitioned to the disposed state, so future calls to
                // CaptureDisposable will immediately dispose the object.
                // No further changes to _state.Disposables, are allowed.
                _disposed = true;

            }

            if (IsRootScope && !RootProvider.IsDisposed())
            {
                // If this ServiceProviderEngineScope instance is a root scope, disposing this instance will need to dispose the RootProvider too.
                // Otherwise the RootProvider will never get disposed and will leak.
                // Note, if the RootProvider get disposed first, it will automatically dispose all attached ServiceProviderEngineScope objects.
                RootProvider.Dispose();
            }

            // ResolvedServices is never cleared for singletons because there might be a compilation running in background
            // trying to get a cached singleton service. If it doesn't find it
            // it will try to create a new one which will result in an ObjectDisposedException.
            return _disposables;
        }
    
}