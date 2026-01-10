using System.Collections.Concurrent;
using System.Reflection;

namespace DICore1;


public class ServiceProvider
{
    // Справочник сервисов - первый Type это интерфейс, второй Type это класс
    private Dictionary<Type, Type> _registeredServices = new Dictionary<Type, Type>();

    // Регистрация сервиса
    public void Register(Type from, Type to)
    {
        _registeredServices[from] = to;
    }
    // Регистрация сервиса через обобщенные параметры
    public void Register<T, U>() => Register(typeof(T), typeof(U));

    // Получение сервиса
    public object GetService(Type serviceType)
    {
        Type type;
        // Если не найдено в справочнике
        var result = _registeredServices.TryGetValue(serviceType, out type);
        if (!result)
        {
            throw new Exception("NOT_FOUND_SERVICE");
        }

        var constructor = GetConstructor(type);
        if (constructor == null)
        {
            throw new Exception("NOT_FOUND_CONSTRUCTOR");
        }

        object[] arguments = constructor.GetParameters()
            // Рекурсия по параметру
            .Select(p => GetService(p.ParameterType)).ToArray();
        // Создаем объект
        object service = constructor.Invoke(arguments);
        return service;
    }

    // Получение конструктора
    private ConstructorInfo GetConstructor(Type type)
    {
        ConstructorInfo[] constructors = type.GetConstructors();
        return constructors.FirstOrDefault();
    }
}