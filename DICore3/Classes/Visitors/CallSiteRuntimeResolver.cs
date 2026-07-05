namespace DICore3.Classes.Visitors;

public class CallSiteRuntimeResolver : CallSiteVisitor<ServiceProviderEngineScope>
{
    public static CallSiteRuntimeResolver Instance = new CallSiteRuntimeResolver();
    protected override object VisitConstructor(ConstructorCallSite constructorCallSite, ServiceProviderEngineScope scope)
    {
        if (constructorCallSite.ParameterCallSites.Length == 0)
        {
            return constructorCallSite.ConstructorInfo.Invoke(null);
        }
        var parameterValues = new List<object>();
        foreach (var dependency in constructorCallSite.ParameterCallSites)
        {
            var value = VisitCallSite(dependency, scope);
            parameterValues.Add(value);
        }
        return constructorCallSite.ConstructorInfo.Invoke(parameterValues.ToArray());
    }
    // Получение Синглтона
    protected override object VisitRootCache(ServiceCallSite callSite, ServiceProviderEngineScope scope)
    {
        
        // Fast path - первая проверка без лока
        if (callSite.Value != null) return callSite.Value;

        // ВАЖНО: Лочим именно callSite, а не отдельный объект!
        // Это позволяет избежать конфликтов между разными синглтонами
        lock (callSite)
        {
            // Вторая проверка под локом (Double-Checked Locking)
            if (callSite.Value == null)
            {
                // Создаем объект
                var resolved = VisitCallSiteMain(callSite, scope);
                
                // ВАЖНО: Добавляем в _disposables КОРНЕВОГО скоупа!
                scope.RootProvider.Root.CaptureDisposable(resolved);
                
                // Сохраняем в callSite.Value
                callSite.Value = resolved;
            }
            return callSite.Value;
        }
    }
    // Получение Scoped
    protected override object VisitCache(ServiceCallSite callSite, ServiceProviderEngineScope scope)
    {
        return scope.ResolvedServices.GetOrAdd(callSite, key =>
        {
            var resolved = VisitCallSiteMain(callSite, scope);
            // Добавляем в _disposables ТЕКУЩЕГО скоупа
            scope.CaptureDisposable(resolved);
            return resolved;
        });
    }
    
    protected override object VisitNoCache(ServiceCallSite callSite, ServiceProviderEngineScope scope)
    {
        var resolved = VisitCallSiteMain(callSite, scope);
        // Для Transient тоже нужно трекировать disposable, если скоуп еще не диспознут
        scope.CaptureDisposable(resolved);
        return resolved;
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