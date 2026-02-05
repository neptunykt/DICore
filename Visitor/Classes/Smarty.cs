using Visitor.Interfaces;

namespace Visitor.Classes;

// Умная девушка
public class Smarty : Girl
{
    public override string Name => "Smarty";

    // Метод принятия посетителя (Double Dispatch)
    public override void Accept(IVisitor visitor){
        Console.WriteLine("Я не разбираюсь в мужчинах, вот она я - сделай мне какой-нибудь подарок!");
        visitor.Visit(this);
    }
    
}
