// See https://aka.ms/new-console-template for more information

using Visitor.Classes;
using Visitor.Interfaces;

var visitors = new List<IVisitor>();
var beauty = new Barbie();
visitors.Add(new SugarDaddy());
// Обход посетителей
foreach (var visitor in visitors)
{
    // Барби принимает разных посетителей
    beauty.Accept(visitor);
}