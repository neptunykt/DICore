namespace Closure.Classes;

public class ServiceProviderEngine
{
    // замыкается параметр callSite при вызове метода
    public Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite) {
        // замыкается переменная счетчика callCount
        var callCount = 0;
        return scope =>
        {
            // наследник вызывает методы со свитчами абстрактного визитора
            callCount++;
            Console.WriteLine($"Счетчик числа созданий для {callSite.Name}: {callCount}");
            return new Visitor().Visit(callSite, scope);
        };
    }
}
