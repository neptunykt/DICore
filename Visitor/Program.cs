// See https://aka.ms/new-console-template for more information

using Visitor.Classes;
using Visitor.Interfaces;

var visitors = new List<IVisitor>();
var beauty = new Beauty();
visitors.Add(new Romantic());
visitors.Add(new SugarDaddy());
// Обход посетителей
foreach (var visitor in visitors)
{
    // Барби принимает разных посетителей
    beauty.Accept(visitor);
}