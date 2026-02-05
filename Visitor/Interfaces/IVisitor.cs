
using Visitor.Classes;

namespace Visitor.Interfaces;


// интерфейс посетителя
public interface IVisitor
{
    void Visit(Barbie barbie);
    void Visit(Smarty smarty);

}