using DICore4;
using DICore4.Abstractions;
using TestClasses;
using Xunit;

namespace test4;

    public class CustomDi4Test
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
    }