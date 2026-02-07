namespace DIMainFrame.Classes.Visitors;

public abstract class CallSiteVisitor<TArg>
{
 
    public virtual object VisitCallSite(ServiceCallSite callSite, TArg scope)
    {

        Console.WriteLine("Делаю тяжелую рекурсивную работу двухуровневым switch");
        // УРОВЕНЬ 1: Switch по Lifetime
        Console.WriteLine("Первый switch по времени жизни");
        return callSite.Lifetime switch
        {
            ServiceLifetime.Singleton => VisitRootCache(callSite, scope),
            ServiceLifetime.Scoped => VisitCache(callSite, scope),
            ServiceLifetime.Transient => VisitNoCache(callSite, scope),
            _ => throw new ArgumentOutOfRangeException()
        };

    }
    
    
    
    // УРОВЕНЬ 2: Switch по Типу
    protected virtual object VisitCallSiteMain(ServiceCallSite callSite, TArg scope)
    {
        Console.WriteLine("Второй switch по типу");
        switch (callSite.Kind)
        {
            case CallSiteKind.Constructor:
               return VisitConstructor((ConstructorCallSite) callSite, scope);
            default:
                throw new NotSupportedException("supported exception");
        }
    }
    
    protected abstract object VisitConstructor(ConstructorCallSite constructorCallSite, TArg scope);

    protected virtual object VisitRootCache(ServiceCallSite callSite, TArg scope)
    {
        return VisitCallSiteMain(callSite, scope);
    }
    protected virtual object VisitCache(ServiceCallSite callSite, TArg scope)
    {
        return VisitCallSiteMain(callSite, scope);
    }
    
    protected virtual object VisitNoCache(ServiceCallSite callSite, TArg scope)
    {
        return VisitCallSiteMain(callSite, scope);
    }
    
 
    
}