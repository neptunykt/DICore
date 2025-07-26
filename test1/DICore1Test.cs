using DICore1;
using TestClasses;
using Xunit;

namespace test1;

public class DICore1Test
{
    [Fact]
    private void CreateObjectTest()
    {
        var bazConstructor = typeof(Baz).GetConstructors().FirstOrDefault();
        var bazConstructorsLength = typeof(Baz).GetConstructors().Length;
        // Создание простого объекта без параметров
        var baz = bazConstructor?.Invoke(null);
        // По умолчанию если не указан конструктор используется конструктор по умолчанию
        Assert.Equal(1, bazConstructorsLength);
        Assert.NotNull(baz);
        var fooConstructorLength = typeof(Foo).GetConstructors().Length;
        var fooConstructor = typeof(Foo).GetConstructors().FirstOrDefault();
        // Создание "сложного объекта" с параметром
        var foo = fooConstructor?.Invoke(new[] { baz });
        Assert.NotNull(foo);
        // Все равно один конструктор который указан с одним параметром
        Assert.Equal(1, fooConstructorLength);
    }

    [Fact]
    private void CreateServiceByServiceProviderTest()
    {
        var serviceProvider = new ServiceProvider();
        serviceProvider.Register<IBaz, Baz>();
        serviceProvider.Register<IFoo, Foo>();
        var baz = serviceProvider.GetService(typeof(IBaz));
        var foo = serviceProvider.GetService(typeof(IFoo));
        Assert.NotNull(baz);
        Assert.NotNull(foo);
    }
}