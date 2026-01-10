using System.Diagnostics;
using DICore4.ServiceLookup.Expressions;
using DICore4.ServiceLookup.Runtime;

namespace DICore4.ServiceLookup.Dynamic;

internal sealed class DynamicServiceProviderEngine : CompiledServiceProviderEngine
{
    private readonly ServiceProvider _serviceProvider;

    // [RequiresDynamicCode("Creates DynamicMethods")]
    public DynamicServiceProviderEngine(ServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite)
    {
        // Подсчет числа вызовов через замыкание
        int callCount = 0;
        return scope =>
        {
            // Resolve the result before we increment the call count, this ensures that singletons
            // won't cause any side effects during the compilation of the resolve function.
            // здесь мы в любом случае получаем объект
            var result = CallSiteRuntimeResolver.Instance.Resolve(callSite, scope);
            // если это второй вызов, заменяем делегат
            if (Interlocked.Increment(ref callCount) == 2)
            {
                // Don't capture the ExecutionContext when forking to build the compiled version of the
                // resolve function
                _ = ThreadPool.UnsafeQueueUserWorkItem(_ =>
                    {
                        try
                        {
                            // мы знаем что сюда заходит только один вызов (второй), другие вызовы сюда не заходят
                            // они получают старый делегат пока делегат ServiceAccessor на основе Expression не подменится
                            _serviceProvider.ReplaceServiceAccessor(callSite, base.RealizeService(callSite));
                        }
                        catch (Exception ex)
                        {
                            // DependencyInjectionEventSource.Log.ServiceRealizationFailed(ex, _serviceProvider.GetHashCode());

                            Debug.Fail($"We should never get exceptions from the background compilation.{Environment.NewLine}{ex}");
                        }
                    },
                    null);
            }
            // возврат результата
            return result;
        };
    }
}