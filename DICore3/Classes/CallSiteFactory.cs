using DICore3.Abstractions;
using System.Collections.Concurrent;



namespace DICore3.Classes;

public class CallSiteFactory
{
    private readonly ConcurrentDictionary<ServiceIdentifier, ServiceDescriptor?> _descriptorLookup = new();
    private readonly ConcurrentDictionary<ServiceIdentifier, ServiceCallSite> _callSiteCache = new();
    private readonly ServiceDescriptor[] _descriptors;

    // Стек для предотвращения бесконечной рекурсии (циклических зависимостей)
    private readonly ThreadLocal<Stack<ServiceIdentifier>> _callSiteStack = new(() => new Stack<ServiceIdentifier>());

    public CallSiteFactory(ICollection<ServiceDescriptor> descriptors)
    {
        _descriptors = new ServiceDescriptor[descriptors.Count];
        descriptors.CopyTo(_descriptors, 0);
        Populate();
    }

    private void Populate()
    {
        foreach (ServiceDescriptor descriptor in _descriptors)
        {
            var serviceIdentifier = ServiceIdentifier.FromDescriptor(descriptor);
            // В DI .NET последняя регистрация перезаписывает предыдущую для одиночного разрешения
            _descriptorLookup[serviceIdentifier] = descriptor;
        }
    }

    /// <summary>
    /// Основной метод получения CallSite. Использует кэш и вызывает рекурсивное создание.
    /// </summary>
    public ServiceCallSite? GetCallSite(ServiceIdentifier serviceIdentifier)
    {
        // 1. Проверяем кэш
        if (_callSiteCache.TryGetValue(serviceIdentifier, out var cachedCallSite))
        {
            return cachedCallSite;
        }

        // 2. Проверка на циклические зависимости
        var stack = _callSiteStack.Value!;
        if (stack.Contains(serviceIdentifier))
        {
            throw new InvalidOperationException($"Circular dependency detected for service: {serviceIdentifier.ServiceType}");
        }

        stack.Push(serviceIdentifier);
        try
        {
            // 3. Создаем CallSite
            var callSite = CreateCallSite(serviceIdentifier);
            
            if (callSite != null)
            {
                // Сохраняем в кэш для повторного использования
                return _callSiteCache.GetOrAdd(serviceIdentifier, callSite);
            }

            return null;
        }
        finally
        {
            stack.Pop();
        }
    }

    private ServiceCallSite? CreateCallSite(ServiceIdentifier serviceIdentifier)
    {
        // Ищем дескриптор для данного типа
        if (!_descriptorLookup.TryGetValue(serviceIdentifier, out var descriptor) || descriptor == null)
        {
            return null;
        }

        // В рамках данного примера мы всегда создаем ConstructorCallSite
        return CreateConstructorCallSite(serviceIdentifier, descriptor);
    }

    private ServiceCallSite CreateConstructorCallSite(ServiceIdentifier serviceIdentifier, ServiceDescriptor descriptor)
    {
        Type implementationType = descriptor.ImplementationType;
        
        // Берем самый "жадный" конструктор (с наибольшим числом параметров)
        var constructor = implementationType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (constructor == null)
        {
            throw new InvalidOperationException($"No public constructor found for type: {implementationType}");
        }

        var parameters = constructor.GetParameters();
        var parameterCallSites = new ServiceCallSite[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameterIdentifier = new ServiceIdentifier(parameters[i].ParameterType);
            
            // РЕКУРСИВНЫЙ ВЫЗОВ: Пытаемся получить CallSite для каждого параметра конструктора
            var paramCallSite = GetCallSite(parameterIdentifier);

            if (paramCallSite == null)
            {
                throw new InvalidOperationException(
                    $"Unable to resolve service for type '{parameters[i].ParameterType}' while attempting to activate '{implementationType}'.");
            }

            parameterCallSites[i] = paramCallSite;
        }

        return new ConstructorCallSite
        {
            ServiceType = descriptor.ServiceType,
            ImplementationType = implementationType,
            ConstructorInfo = constructor,
            ParameterCallSites = parameterCallSites,
            Lifetime = descriptor.Lifetime
        };
    }
}
