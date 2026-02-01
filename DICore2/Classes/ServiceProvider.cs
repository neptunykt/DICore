using System.Reflection;
using DICore2.Abstractions;
using IServiceProvider = DICore2.Abstractions.IServiceProvider;


namespace DICore2.Classes;

public class ServiceProvider : IServiceProvider, IServiceScopeFactory
{
    private readonly IServiceCollection _services = new ServiceCollection();
    internal ServiceProviderEngineScope Root { get; }

    internal Dictionary<ServiceDescriptor, object> ResolvedServices { get; } =
        new Dictionary<ServiceDescriptor, object>();

    public ServiceProvider(IServiceCollection services)
    {
        _services = services;
        Root = new ServiceProviderEngineScope(this, isRootScope: true);
    }

    public object? GetService(Type serviceType)
    {
        return GetService(serviceType, Root);
    }

    internal object? GetService(Type serviceType, ServiceProviderEngineScope serviceProviderEngineScope)
    {
        Type type;
        // Если не найдено в справочнике - поиск неоптимален (пропущены хранилища типов в других классах, только для примера)
        var descriptor = _services.FirstOrDefault(f => f.ServiceType == serviceType);
        if (descriptor == null)
        {
            throw new ArgumentException();
        }

        type = descriptor.ImplementationType;
        

        switch (descriptor.Lifetime)
        {
            case ServiceLifetime.Scoped:
                if (serviceProviderEngineScope.ResolvedServices.TryGetValue(descriptor, out object? scopedValue))
                {
                    return scopedValue;
                }

                var scopedObj = GetServiceByReflection(type);
                serviceProviderEngineScope.ResolvedServices.Add(descriptor, scopedObj);
                return scopedObj;
            case ServiceLifetime.Transient:
                return GetServiceByReflection(type);
            default:
                if (ResolvedServices.TryGetValue(descriptor, out object? singletonValue))
                {
                    return singletonValue;
                }

                var singletonObj = GetServiceByReflection(type);
                ResolvedServices.Add(descriptor, singletonObj);
                return singletonObj;
        }
    }
    
    private object? GetServiceByReflection(Type type)
    {
        var constructor = GetConstructor(type);
        if (constructor == null)
        {
            return null;
        }

        // Первый конструктор помеченный артибутом
        object[] arguments = constructor.GetParameters()
            // рекурурсия по параметру
            .Select(p => GetService(p.ParameterType)).ToArray();
        // Создаем объект
        object service = constructor.Invoke(arguments);
        return service;
    }


    private ConstructorInfo GetConstructor(Type type)
    {
        ConstructorInfo[] constructors = type.GetConstructors();
        return constructors.FirstOrDefault();
    }

    public IServiceScope CreateScope()
    {
        return new ServiceProviderEngineScope(this, isRootScope: false);
    }
}