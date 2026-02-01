using Visitor.Interfaces;

namespace Visitor.Classes;

// Класс Красотка
public class Beauty
{
    public string Name => "Barbie";

    // Метод принятия посетителя (Double Dispatch)
    public void Accept(IVisitor visitor){
        Console.WriteLine("Я не разбираюсь в мужчинах, вот она я - сделай мне какой-нибудь подарок!");
        visitor.Visit(this);
    }

}