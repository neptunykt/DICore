using DICore4;
using DICore4.Abstractions;
using TestClasses;
using Xunit;

namespace test4;

public class CreateServiceByExpressionTest
{
    [Fact]
    private void CreateServiceByServiceProviderTest()
    {
        var serviceScopedCollection = new ServiceCollection();
        serviceScopedCollection.AddScoped<IBaz, Baz>();
        var serviceProvider = serviceScopedCollection.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        // First call
        var bazFromScopeFirst = scope.ServiceProvider.GetService<IBaz>();
        // Second call
        var bazFromScopeSecond = scope.ServiceProvider.GetService<IBaz>();
        Assert.True(ReferenceEquals(bazFromScopeFirst, bazFromScopeSecond));
    }
    
    public interface IBaz
    {
        
    }

    public class Baz : IBaz
    {
        
    }
}