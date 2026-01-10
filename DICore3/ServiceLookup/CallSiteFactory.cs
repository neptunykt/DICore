using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using DICore3.Abstractions;
using DICore3.Abstractions.Common.src.Extensions.ParameterDefaultValue;

namespace DICore3.ServiceLookup;

internal sealed class CallSiteFactory : IServiceProviderIsService
{
    private const int DefaultSlot = 0;

    private readonly ServiceDescriptor[] _descriptors;

    // Хранилище Service дескрипторов ServiceDescriptorCacheItem это List ServiceIdentifier с Enumerable
    // Быстрый словарь вызывается один раз при вызове BuildServiceProvider
    // Он сопоставляет тип сервиса (например, IService) с его регистрацией (ServiceDescriptor),
    // в которой указано, какой класс (ImplementationType) нужно создать.
    private readonly Dictionary<ServiceIdentifier, ServiceDescriptorCacheItem> _descriptorLookup =
        new Dictionary<ServiceIdentifier, ServiceDescriptorCacheItem>();
    // Хранилище ServiceCallSite
    // Готовый план сборки (Кеш планов)
    private readonly ConcurrentDictionary<ServiceCacheKey, ServiceCallSite> _callSiteCache =
        new ConcurrentDictionary<ServiceCacheKey, ServiceCallSite>();
    
    // Синхронизация
    private readonly ConcurrentDictionary<ServiceIdentifier, object> _callSiteLocks =
        new ConcurrentDictionary<ServiceIdentifier, object>();

    private readonly StackGuard _stackGuard;

    public CallSiteFactory(ICollection<ServiceDescriptor> descriptors)
    {
        _stackGuard = new StackGuard();
        _descriptors = new ServiceDescriptor[descriptors.Count];
        descriptors.CopyTo(_descriptors, 0);

        Populate();
    }

    internal ServiceDescriptor[] Descriptors => _descriptors;

    private void Populate()
    {
        foreach (ServiceDescriptor descriptor in _descriptors)
        {
            Type serviceType = descriptor.ServiceType;

            if (descriptor.TryGetImplementationType(out Type? implementationType))
            {
                Debug.Assert(implementationType != null);

                if (implementationType.IsGenericTypeDefinition ||
                    implementationType.IsAbstract ||
                    implementationType.IsInterface)
                {
                    throw new ArgumentException("SR.Format(SR.TypeCannotBeActivated, implementationType, serviceType)");
                }
            }

            var cacheKey = ServiceIdentifier.FromDescriptor(descriptor);
            _descriptorLookup.TryGetValue(cacheKey, out ServiceDescriptorCacheItem cacheItem);
            _descriptorLookup[cacheKey] = cacheItem.Add(descriptor);
        }
    }

    internal ServiceCallSite? GetCallSite(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain) =>
        _callSiteCache.TryGetValue(new ServiceCacheKey(serviceIdentifier, DefaultSlot), out ServiceCallSite? site)
            ? site
            : CreateCallSite(serviceIdentifier, callSiteChain);

    internal ServiceCallSite? GetCallSite(ServiceDescriptor serviceDescriptor, CallSiteChain callSiteChain)
    {
        var serviceIdentifier = ServiceIdentifier.FromDescriptor(serviceDescriptor);
        if (_descriptorLookup.TryGetValue(serviceIdentifier, out ServiceDescriptorCacheItem descriptor))
        {
            return TryCreateExact(serviceDescriptor, serviceIdentifier, callSiteChain,
                descriptor.GetSlot(serviceDescriptor));
        }

        Debug.Fail("_descriptorLookup didn't contain requested serviceDescriptor");
        return null;
    }

    private ServiceCallSite? CreateCallSite(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
    {
        if (!_stackGuard.TryEnterOnCurrentStack())
        {
            return _stackGuard.RunOnEmptyStack(CreateCallSite, serviceIdentifier, callSiteChain);
        }

        // We need to lock the resolution process for a single service type at a time:
        // Consider the following:
        // C -> D -> A
        // E -> D -> A
        // Resolving C and E in parallel means that they will be modifying the callsite cache concurrently
        // to add the entry for C and E, but the resolution of D and A is synchronized
        // to make sure C and E both reference the same instance of the callsite.

        // This is to make sure we can safely store singleton values on the callsites themselves

        var callsiteLock = _callSiteLocks.GetOrAdd(serviceIdentifier, static _ => new object());

        lock (callsiteLock)
        {
            callSiteChain.CheckCircularDependency(serviceIdentifier);

            ServiceCallSite? callSite = TryCreateExact(serviceIdentifier, callSiteChain);

            return callSite;
        }
    }

    private ServiceCallSite? TryCreateExact(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
    {
        if (_descriptorLookup.TryGetValue(serviceIdentifier, out ServiceDescriptorCacheItem descriptor))
        {
            return TryCreateExact(descriptor.Last, serviceIdentifier, callSiteChain, DefaultSlot);
        }

        return null;
    }

    private ServiceCallSite? TryCreateExact(ServiceDescriptor descriptor, ServiceIdentifier serviceIdentifier,
        CallSiteChain callSiteChain, int slot)
    {
        if (serviceIdentifier.ServiceType == descriptor.ServiceType)
        {
            ServiceCacheKey callSiteKey = new ServiceCacheKey(serviceIdentifier, slot);
            if (_callSiteCache.TryGetValue(callSiteKey, out ServiceCallSite? serviceCallSite))
            {
                return serviceCallSite;
            }

            ServiceCallSite callSite;
            var lifetime = new ResultCache(descriptor.Lifetime, serviceIdentifier, slot);
            if (descriptor.HasImplementationInstance())
            {
                callSite = new ConstantCallSite(descriptor.ServiceType, descriptor.GetImplementationInstance());
            }
            else if (descriptor.ImplementationFactory != null)
            {
                callSite = new FactoryCallSite(lifetime, descriptor.ServiceType, descriptor.ImplementationFactory);
            }
            else if (descriptor.HasImplementationType())
            {
                callSite = CreateConstructorCallSite(lifetime, serviceIdentifier, descriptor.GetImplementationType()!,
                    callSiteChain);
            }
            else
            {
                throw new InvalidOperationException("SR.InvalidServiceDescriptor");
            }

            return _callSiteCache[callSiteKey] = callSite;
        }

        return null;
    }

    private ConstructorCallSite CreateConstructorCallSite(
        ResultCache lifetime,
        ServiceIdentifier serviceIdentifier,
        Type implementationType,
        CallSiteChain callSiteChain)
    {
        try
        {
            callSiteChain.Add(serviceIdentifier, implementationType);
            ConstructorInfo[] constructors = implementationType.GetConstructors();

            ServiceCallSite[]? parameterCallSites = null;

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException("SR.Format(SR.NoConstructorMatch, implementationType)");
            }
            else if (constructors.Length == 1)
            {
                ConstructorInfo constructor = constructors[0];
                ParameterInfo[] parameters = constructor.GetParameters();
                if (parameters.Length == 0)
                {
                    return new ConstructorCallSite(lifetime, serviceIdentifier.ServiceType, constructor);
                }

                parameterCallSites = CreateArgumentCallSites(
                    serviceIdentifier,
                    implementationType,
                    callSiteChain,
                    parameters,
                    throwIfCallSiteNotFound: true)!;

                return new ConstructorCallSite(lifetime, serviceIdentifier.ServiceType, constructor,
                    parameterCallSites);
            }

            Array.Sort(constructors,
                (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

            ConstructorInfo? bestConstructor = null;
            HashSet<Type>? bestConstructorParameterTypes = null;
            for (int i = 0; i < constructors.Length; i++)
            {
                ParameterInfo[] parameters = constructors[i].GetParameters();

                ServiceCallSite[]? currentParameterCallSites = CreateArgumentCallSites(
                    serviceIdentifier,
                    implementationType,
                    callSiteChain,
                    parameters,
                    throwIfCallSiteNotFound: false);

                if (currentParameterCallSites != null)
                {
                    if (bestConstructor == null)
                    {
                        bestConstructor = constructors[i];
                        parameterCallSites = currentParameterCallSites;
                    }
                    else
                    {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (bestConstructorParameterTypes == null)
                        {
                            bestConstructorParameterTypes = new HashSet<Type>();
                            foreach (ParameterInfo p in bestConstructor.GetParameters())
                            {
                                bestConstructorParameterTypes.Add(p.ParameterType);
                            }
                        }

                        foreach (ParameterInfo p in parameters)
                        {
                            if (!bestConstructorParameterTypes.Contains(p.ParameterType))
                            {
                                // Ambiguous match exception
                                throw new InvalidOperationException(
                                    "string.Join(Environment.NewLine,SR.Format(SR.AmbiguousConstructorException, implementationType),bestConstructor,constructors[i])");
                            }
                        }
                    }
                }
            }

            if (bestConstructor == null)
            {
                throw new InvalidOperationException("SR.Format(SR.UnableToActivateTypeException, implementationType)");
            }
            else
            {
                Debug.Assert(parameterCallSites != null);
                return new ConstructorCallSite(lifetime, serviceIdentifier.ServiceType, bestConstructor,
                    parameterCallSites);
            }
        }
        finally
        {
            callSiteChain.Remove(serviceIdentifier);
        }
    }


    /// <returns>Not <b>null</b> if <b>throwIfCallSiteNotFound</b> is true</returns>
    private ServiceCallSite[]? CreateArgumentCallSites(
        ServiceIdentifier serviceIdentifier,
        Type implementationType,
        CallSiteChain callSiteChain,
        ParameterInfo[] parameters,
        bool throwIfCallSiteNotFound)
    {
        var parameterCallSites = new ServiceCallSite[parameters.Length];

        for (int index = 0; index < parameters.Length; index++)
        {
            ServiceCallSite? callSite = null;

            Type parameterType = parameters[index].ParameterType;

            // Всегда true
            if (ServiceProvider.s_allowNonKeyedServiceInject)
            {
                callSite ??= GetCallSite(ServiceIdentifier.FromServiceType(parameterType), callSiteChain);
            }

            if (callSite == null &&
                ParameterDefaultValue.TryGetDefaultValue(parameters[index], out object? defaultValue))
            {
                callSite = new ConstantCallSite(parameterType, defaultValue);
            }

            if (callSite == null)
            {
                if (throwIfCallSiteNotFound)
                {
                    throw new InvalidOperationException(
                        "SR.Format(SR.CannotResolveService, parameterType, implementationType)");
                }

                return null;
            }

            parameterCallSites[index] = callSite;
        }

        return parameterCallSites;
    }

    public void Add(ServiceIdentifier serviceIdentifier, ServiceCallSite serviceCallSite)
    {
        _callSiteCache[new ServiceCacheKey(serviceIdentifier, DefaultSlot)] = serviceCallSite;
    }

    private struct ServiceDescriptorCacheItem
    {
        private ServiceDescriptor _item;

        private List<ServiceDescriptor> _items;

        public ServiceDescriptor Last
        {
            get
            {
                if (_items != null && _items.Count > 0)
                {
                    return _items[_items.Count - 1];
                }

                Debug.Assert(_item != null);
                return _item;
            }
        }

        public int Count
        {
            get
            {
                if (_item == null)
                {
                    Debug.Assert(_items == null);
                    return 0;
                }

                return 1 + (_items?.Count ?? 0);
            }
        }

        public ServiceDescriptor this[int index]
        {
            get
            {
                if (index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (index == 0)
                {
                    return _item;
                }

                return _items[index - 1];
            }
        }

        public int GetSlot(ServiceDescriptor descriptor)
        {
            if (descriptor == _item)
            {
                return Count - 1;
            }

            if (_items != null)
            {
                int index = _items.IndexOf(descriptor);
                if (index != -1)
                {
                    return _items.Count - (index + 1);
                }
            }

            throw new InvalidOperationException("SR.ServiceDescriptorNotExist");
        }

        public ServiceDescriptorCacheItem Add(ServiceDescriptor descriptor)
        {
            var newCacheItem = default(ServiceDescriptorCacheItem);
            if (_item == null)
            {
                Debug.Assert(_items == null);
                newCacheItem._item = descriptor;
            }
            else
            {
                newCacheItem._item = _item;
                newCacheItem._items = _items ?? new List<ServiceDescriptor>();
                newCacheItem._items.Add(descriptor);
            }

            return newCacheItem;
        }
    }


    public bool IsService(Type serviceType)
    {
        if (serviceType is null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        // Querying for an open generic should return false (they aren't resolvable)
        if (serviceType.IsGenericTypeDefinition)
        {
            return false;
        }

        if (_descriptorLookup.ContainsKey(new ServiceIdentifier(serviceType)))
        {
            return true;
        }


        // These are the built in service types that aren't part of the list of service descriptors
        // If you update these make sure to also update the code in ServiceProvider.ctor
        return serviceType == typeof(IServiceProvider) ||
               serviceType == typeof(IServiceScopeFactory) ||
               serviceType == typeof(IServiceProviderIsService);
    }
}