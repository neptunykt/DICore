using DICore3.Abstractions;
using DICore3.Classes;
using DICore3.Extensions;
using TestClasses;
using Xunit;

namespace test3;

public class CustomDi3Test
{
    [Fact]
    private void CreateServiceByServiceProviderTest()
    {
        var serviceScopedCollection = new ServiceCollection();
        serviceScopedCollection.AddScoped<IBaz, Baz>();
        serviceScopedCollection.AddScoped<IFoo, Foo>();
        serviceScopedCollection.AddScoped<IBar, Bar>();
        var serviceProvider = serviceScopedCollection.BuildServiceProvider();
        var scope1 = serviceProvider.CreateScope();
        var scope2 = serviceProvider.CreateScope();
        var fooFromScope1 = scope1.ServiceProvider.GetService<IFoo>();
        var bazFromScope1 = scope1.ServiceProvider.GetService<IBaz>();
        var bazFromScope2 = scope2.ServiceProvider.GetService<IBaz>();
        var fooFromScope2 = scope2.ServiceProvider.GetService<IFoo>();
        var fooFromServiceProvider = serviceProvider.GetService<IFoo>();
        // Assert (У каждого свой скоуп)
        Assert.False(ReferenceEquals(fooFromScope1, fooFromScope2));
        Assert.False(ReferenceEquals(fooFromServiceProvider, fooFromScope1));
        Assert.False(ReferenceEquals(fooFromServiceProvider, fooFromScope2));
        Assert.False(ReferenceEquals(bazFromScope1, bazFromScope2));

        var serviceSingletonCollection = new ServiceCollection();
        serviceSingletonCollection.AddSingleton<IBaz, Baz>();
        serviceSingletonCollection.AddSingleton<IFoo, Foo>();
        var serviceSingletonProvider = serviceScopedCollection.BuildServiceProvider();
        var fooFromSingleton1 = serviceSingletonProvider.GetService<IFoo>();
        var fooFromSingleton2 = serviceSingletonProvider.GetService<IFoo>();
        Assert.True(ReferenceEquals(fooFromSingleton1, fooFromSingleton2));

        var serviceTransientCollection = new ServiceCollection();
        serviceTransientCollection.AddTransient<IBaz, Baz>();
        serviceTransientCollection.AddTransient<IFoo, Foo>();
        var serviceTransientProvider = serviceTransientCollection.BuildServiceProvider();
        var fooFromTransient1 = serviceTransientProvider.GetService<IFoo>();
        var fooFromTransient2 = serviceTransientProvider.GetService<IFoo>();
        Assert.False(ReferenceEquals(fooFromTransient1, fooFromTransient2));
    }
    
    
     [Fact]
    private void CheckChangeDelegate()
    {
        var serviceScopedCollection = new ServiceCollection();
        serviceScopedCollection.AddScoped<IBaz, Baz>();
        serviceScopedCollection.AddScoped<IFoo, Foo>();
        serviceScopedCollection.AddScoped<IBar, Bar>();
        var serviceProvider = serviceScopedCollection.BuildServiceProvider();
        var scopes = new List<IServiceScope>();
        var bazList = new List<IBaz>();
        var fooList = new List<IFoo>();
        for (var i = 0; i < 50; i++)
        {
            var scope = serviceProvider.CreateScope();
            scopes.Add(scope);
            var baz = scope.ServiceProvider.GetService<IBaz>();
            bazList.Add(baz!);
            var foo = scope.ServiceProvider.GetService<IFoo>();
            fooList.Add(foo!);
        }
        // Assert (У каждого свой скоуп)
        Assert.False(ReferenceEquals(fooList[0], fooList[30]));
        Assert.False(ReferenceEquals(bazList[0], bazList[30]));

        var serviceSingletonCollection = new ServiceCollection();
        serviceSingletonCollection.AddSingleton<IBaz, Baz>();
        serviceSingletonCollection.AddSingleton<IFoo, Foo>();
        var serviceSingletonProvider = serviceScopedCollection.BuildServiceProvider();
        var fooFromSingleton1 = serviceSingletonProvider.GetService<IFoo>();
        var fooFromSingleton2 = serviceSingletonProvider.GetService<IFoo>();
        Assert.True(ReferenceEquals(fooFromSingleton1, fooFromSingleton2));

        var serviceTransientCollection = new ServiceCollection();
        serviceTransientCollection.AddTransient<IBaz, Baz>();
        serviceTransientCollection.AddTransient<IFoo, Foo>();
        var serviceTransientProvider = serviceTransientCollection.BuildServiceProvider();
        var fooFromTransient1 = serviceTransientProvider.GetService<IFoo>();
        var fooFromTransient2 = serviceTransientProvider.GetService<IFoo>();
        Assert.False(ReferenceEquals(fooFromTransient1, fooFromTransient2));
    }
}