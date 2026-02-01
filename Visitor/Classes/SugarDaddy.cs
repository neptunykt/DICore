using Visitor.Interfaces;

namespace Visitor.Classes;

// Конкретный посетитель: Богатый папик
public class SugarDaddy: IVisitor {
// 
    public void Visit(Beauty beauty) {
        Console.WriteLine("А это ты дорогая?");
        Console.WriteLine("Кольца и браслеты, юбки и жакеты разве ж я тебе не покупал?");
        Console.WriteLine($"{beauty.Name} получает ключи от спорткара и летит на шопинг в Милан с папиком.");
    }
}
