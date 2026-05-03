// See https://aka.ms/new-console-template for more information

using Visitor.Classes;
using Visitor.Interfaces;

var girls = new List<Girl> { new Barbie(), new Smarty() };
var visitors = new List<IVisitor>
{
    new Romantic(),
    new SugarDaddy()
};
// Обход посетителей
foreach (var girl in girls) {
    foreach (var visitor in visitors) {
        girl.Accept(visitor);
    }
}