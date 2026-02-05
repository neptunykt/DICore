using Visitor.Interfaces;

namespace Visitor.Classes;

// Класс Барби
public class Barbie : Girl
{
    public override string Name => "Barbie";

    // Метод принятия посетителя (Double Dispatch)
    public override void Accept(IVisitor visitor){
        Console.WriteLine("Я не разбираюсь в мужчинах, вот она я - сделай мне какой-нибудь подарок!");
        visitor.Visit(this);
    }
    
}