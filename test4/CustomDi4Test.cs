using DICore4;
using DICore4.Abstractions;
using TestClasses;
using Xunit;
using Xunit.Abstractions;

namespace test4;

    public class CustomDi4Test
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public CustomDi4Test(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
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
        private void CreateDriveObject()
        {
            var serviceScopedCollection = new ServiceCollection();
            serviceScopedCollection.AddScoped<IMovable, Movable>();
            serviceScopedCollection.AddScoped<ICar, Car>();
            var serviceProvider = serviceScopedCollection.BuildServiceProvider();
            var scope1 = serviceProvider.CreateScope();
            var car = scope1.ServiceProvider.GetService<ICar>();
            car.Drive();
        }
        
        
        internal interface IMovable
        {
            void Move();

        }

        internal class Movable : IMovable
        {
            public void Move()
            {
                System.Diagnostics.Debug.WriteLine("Hello");
            }
        }

        internal interface ICar
        {
            public void Drive();
        }

        internal class Car : ICar
        {
            private readonly IMovable _movable;
            public Car(IMovable movable)
            {
                _movable = movable;
            }

            public void Drive()
            {
                _movable.Move();
            }
        }
    }