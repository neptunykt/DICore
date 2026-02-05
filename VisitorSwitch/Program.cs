// See https://aka.ms/new-console-template for more information

// 1. Создаем отдельные товары

using VisitorSwitch.Classes;

var iPhone = new Node { Kind = ItemKind.SingleProduct, Price = 1000m };
var cases = new Node { Kind = ItemKind.SingleProduct, Price = 50m };
var cable = new Node { Kind = ItemKind.SingleProduct, Price = 25m };

// 2. Создаем коробку с аксессуарами
var accessoriesBox = new Node 
{ 
    Kind = ItemKind.Box, 
    Children = new List<Node> { cases, cable } 
};

// 3. Создаем главную коробку (товар + коробка аксессуаров)
var mainOrder = new Node
{
    Kind = ItemKind.Box,
    Children = new List<Node> { iPhone, accessoriesBox }
};

Console.WriteLine($"Общая сумма товаров в коробке: ${new PriceVisitor().VisitPrice(mainOrder)}");
