namespace DICore3.Classes.Visitors;

public class CallSiteRuntimeResolver : CallSiteVisitor<ServiceProviderEngineScope>
{
    private readonly object _lock = new object();
    public static CallSiteRuntimeResolver Instance = new CallSiteRuntimeResolver();
    protected override object VisitConstructor(ConstructorCallSite constructorCallSite, ServiceProviderEngineScope scope)
    {
        if (constructorCallSite.ParameterCallSites == null || constructorCallSite.ParameterCallSites.Length == 0)
        {
            return constructorCallSite.ConstructorInfo.Invoke(null);
        }

        var parameterValues = new List<object>();
        foreach (var dependency in constructorCallSite.ParameterCallSites)
        {
            var value = VisitCallSite(dependency, scope);
            parameterValues.Add(value);
        }
        // 2. Вызываем конструктор (в демо - через упрощенную рефлексию)
        return constructorCallSite.ConstructorInfo.Invoke(parameterValues.ToArray());
    }
    // Получение Синглтона
    protected override object VisitRootCache(ServiceCallSite callSite, ServiceProviderEngineScope scope)
    {
        if (callSite.Value != null) return callSite.Value;

        lock (_lock)
        {
            if (callSite.Value == null)
            {
                callSite.Value = VisitCallSiteMain(callSite, scope);
            }
            return callSite.Value;
        }
    }
    // Получение Scoped
    protected override object VisitCache(ServiceCallSite callSite, ServiceProviderEngineScope scope)
    {
        return scope.ResolvedServices.GetOrAdd(callSite, s => VisitCallSiteMain(callSite, scope));
    }
    
    protected override object VisitNoCache(ServiceCallSite callSite, ServiceProviderEngineScope scope)
    {
        return VisitCallSiteMain(callSite, scope);
    }
    public object Resolve(ServiceCallSite callSite, ServiceProviderEngineScope scope)
    {
        // Fast path to avoid virtual calls if we already have the cached value in the root scope
        if (scope.IsRootScope && callSite.Value is object cached)
        {
            return cached;
        }

        return VisitCallSite(callSite, scope);
    }

}