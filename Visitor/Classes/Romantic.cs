using Visitor.Interfaces;

namespace Visitor.Classes;

// Конкретный посетитель: Романтик
public class Romantic : IVisitor
{
    public void Visit(Beauty beauty)
    {
        Console.WriteLine("А это ты дорогая?");
            Console.WriteLine($"{beauty.Name} слушает стихи под луной и получает букет полевых цветов.");

    }
}