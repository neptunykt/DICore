using DICore4.Abstractions;
using test4.TestClasses;
using Xunit;

namespace test4;

public class ServiceProviderCompilationTest
{
    [Theory]
    [InlineData(ServiceProviderMode.Dynamic, typeof(I625))]
    [InlineData(ServiceProviderMode.Runtime, typeof(I625))]
    [InlineData(ServiceProviderMode.Expressions, typeof(I625))]
    private async Task CompilesInLimitedStackSpace(ServiceProviderMode mode, Type serviceType)
    {
        // Arrange
        var stackSize = 1024;
        var serviceCollection = new ServiceCollection();
        CompilationTestDataProvider.Register(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider(mode);

        // Act + Assert

        var tsc = new TaskCompletionSource<object>();
        var thread = new Thread(() =>
        {
            try
            {
                object service = null;
                for (int i = 0; i < 10; i++)
                {
                    service = serviceProvider.GetService(serviceType);
                    var hashCode = service.GetHashCode();
                    var sss = 0;
                    sss++;
                }

                tsc.SetResult(service);
            }
            catch (Exception ex)
            {
                tsc.SetException(ex);
            }
        }, stackSize);

        thread.Start();
        thread.Join();
        await tsc.Task;
    }
}