using Visitor.Interfaces;

namespace Visitor.Classes;

public class Romantic : IVisitor
{
    public void Visit(Barbie barbie)
    {
        Console.WriteLine("А это ты дорогая?");
        Console.WriteLine($"Романтик читает стихи {barbie.Name} под луной.");
    }

    public void Visit(Smarty smarty)
    {
        Console.WriteLine("Привет, умная девушка.");
       Console.WriteLine($"Романтик обсуждает с {smarty.Name} квантовую физику.");
    }
}