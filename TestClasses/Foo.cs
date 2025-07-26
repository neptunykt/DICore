namespace TestClasses;

public interface IFoo
{
        
}

public class Foo : IFoo
{
    private IBaz _baz;
    public Foo(IBaz baz)
    {
        _baz = baz;
    }
}