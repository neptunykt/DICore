using Visitor.Interfaces;

namespace Visitor.Classes;

// Конкретный посетитель: Богатый папик
public class SugarDaddy: IVisitor {
// 
    public void Visit(Barbie barbie) {
        Console.WriteLine("А это ты дорогая?");
        Console.WriteLine("Кольца и браслеты, юбки и жакеты разве ж я тебе не покупал?");
        Console.WriteLine($"{barbie.Name} получает ключи от спорткара и летит на шопинг в Милан с папиком.");
    }

    public void Visit(Smarty smarty)
    {
        Console.WriteLine("Привет умная девушка.");
        Console.WriteLine($"Папик консультируется с {smarty.Name} о покупке контрольных пакетов акций Tesla.");
    }
}
