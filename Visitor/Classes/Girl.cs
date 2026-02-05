using Visitor.Interfaces;

namespace Visitor.Classes;

public abstract class Girl
{
    public abstract string Name { get; }
    public abstract void Accept(IVisitor visitor);

}