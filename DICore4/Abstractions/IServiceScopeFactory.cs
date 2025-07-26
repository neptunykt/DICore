namespace DICore4.Abstractions;

public interface IServiceScopeFactory
{
    IServiceScope CreateScope();
}