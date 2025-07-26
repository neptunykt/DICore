using System.Collections.Concurrent;
using System.Reflection;

namespace DICore1;

public class ServiceProvider
{
    // Справочник сервисов (контейнер)
    // Первый Type это интерфейс, второй Type это класс
    private ConcurrentDictionary<Type, Type> _resolvedServices = new ConcurrentDictionary<Type, Type>();
    public void Register<T, U>() => Register(typeof(T), typeof(U));
    public void Register(Type from, Type to)
    {
        _resolvedServices[from] = to;
    }
        
    public object GetService(Type serviceType)
    {
        Type type;
        // Если не найдено в справочнике
        if (!_resolvedServices.TryGetValue(serviceType, out type))
        {
            type = serviceType;
        }

        if (type.IsInterface || type.IsAbstract)
        {
            return null;
        }

        var constructor = GetConstructor(type);
        if (constructor == null)
        {
            return null;
        }
        object[] arguments = constructor.GetParameters() 
            // Рекурсия по параметру
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
}