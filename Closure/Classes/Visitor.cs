namespace Closure.Classes;

public class Visitor
{
    public object Visit(ServiceCallSite callSite, ServiceProviderEngineScope scope) {
	
        Console.WriteLine("Делаю тяжелую рекурсивную работу");
        Console.WriteLine($"Создание объекта в {scope.Name} по {callSite.Name}");
        return new ObjectFromReflection {Name="Объект" };
    }

}