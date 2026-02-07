using System.Diagnostics;
using DIMainFrame.Classes.Visitors;

namespace DIMainFrame.Classes.ServiceProviderEngines;

public class DynamicServiceProviderEngine : CompiledServiceProviderEngine
{
    private readonly ServiceProvider _serviceProvider;
    public override Func<ServiceProviderEngineScope, object> Realize(ServiceCallSite callSite)
    {
        // Подсчет числа вызовов через замыкание
        int callCount = 0;
        return scope =>
        {
       
            var result = CallSiteRuntimeResolver.Instance.Resolve(callSite, scope);
            // если это второй вызов, заменяем делегат
            if (Interlocked.Increment(ref callCount) == 2)
            {
               
                  _ = ThreadPool.UnsafeQueueUserWorkItem(_ =>
                    {
                        try
                        {
                            Console.WriteLine("Подмена делегата");
                            _serviceProvider.ReplaceServiceAccessor(callSite, base.Realize(callSite));
                        }
                        catch (Exception ex)
                        {

                            Debug.Fail($"We should never get exceptions from the background compilation.{Environment.NewLine}{ex}");
                        }
                    },
                    null);
            }
            // возврат результата
            return result;
        };
    }
    
    public DynamicServiceProviderEngine(ServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
}