using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DICore3.Classes.Visitors;

public class ExpressionResolverBuilder : CallSiteVisitor<ParameterExpression>
{

    public Func<ServiceProviderEngineScope, object> Build(ServiceCallSite callSite)
    {
        var scopeParameter = Expression.Parameter(typeof(ServiceProviderEngineScope), "scope");
        var expression = (Expression)VisitCallSite(callSite, scopeParameter);
        var lambda = Expression.Lambda<Func<ServiceProviderEngineScope, object>>(expression, scopeParameter);
        return lambda.Compile();
    }

    protected override object VisitConstructor(ConstructorCallSite constructorCallSite, ParameterExpression scope)
    {
        // Собираем параметры конструктора
        Expression[] parameters;
        parameters = new Expression[constructorCallSite.ParameterCallSites.Length];
        for (int i = 0; i < constructorCallSite.ParameterCallSites.Length; i++)
        {
            var paramCallSite = constructorCallSite.ParameterCallSites[i];
            var paramExpression = (Expression)VisitCallSite(paramCallSite, scope);
            // Приводим к нужному типу параметра конструктора
            var constructorParamType = constructorCallSite.ConstructorInfo.GetParameters()[i].ParameterType;
            if (paramExpression.Type != constructorParamType)
            {
                paramExpression = Expression.Convert(paramExpression, constructorParamType);
            }
            parameters[i] = paramExpression;
        }

        // Создаем выражение new T(param1, param2, ...)
        var newExpression = Expression.New(constructorCallSite.ConstructorInfo, parameters);
        
        // Приводим к object для возврата
        return Expression.Convert(newExpression, typeof(object));
    }

    protected override object VisitRootCache(ServiceCallSite callSite, ParameterExpression scope)
    {
        // Для Singleton: проверяем кэш в callSite.Value, если нет - создаем и сохраняем
        var callSiteConstant = Expression.Constant(callSite);
        // получаем доступ к полю
        var valueField = Expression.Field(callSiteConstant, nameof(ServiceCallSite.Value));
        // Проверяем, не null ли уже значение
        var checkNotNull = Expression.NotEqual(valueField, Expression.Constant(null, typeof(object)));
        // Если не null, возвращаем его
        var returnCached = Expression.Convert(valueField, typeof(object));
        // Если null, создаем через VisitCallSiteMain
        var createNew = (Expression)VisitCallSiteMain(callSite, scope);
        
        // Создаем переменную для хранения созданного объекта
        var resolvedVar = Expression.Variable(typeof(object), "resolved");
        // Присваиваем результат создания в переменную
        var assignResolved = Expression.Assign(resolvedVar, createNew);
        // Вызываем CaptureDisposable на корневом скоупе
        var rootScopeProperty = Expression.Property(
            Expression.Property(scope, "RootProvider"), 
            "Root");
        var captureDisposableCall = Expression.Call(
            rootScopeProperty,
            typeof(ServiceProviderEngineScope).GetMethod("CaptureDisposable")!,
            resolvedVar);
        // Сохраняем в callSite.Value
        var assignToValue = Expression.Assign(valueField, resolvedVar);
        // Блок для формирования группы из нескольких выражений
        var lockObject = Expression.Constant(callSite);
        // Блок для формирования группы из нескольких выражений
        var lockCreateNew = Expression.Block(
            new[] { resolvedVar }, // Объявляем переменную
            // Первое выражение в блоке Входим в lock 
            Expression.Call(typeof(Monitor), "Enter", null, lockObject),
            // Второе выражение в блоке
            Expression.TryFinally(
                    // Вторая проверка на null (тело try)
                    Expression.IfThen(
                        Expression.Equal(valueField, Expression.Constant(null, typeof(object))), // Выражение Условие
                        Expression.Block(
                            assignResolved,           // resolved = createNew
                            captureDisposableCall,    // scope.CaptureDisposable(resolved)
                            assignToValue             // callSite.Value = resolved
                        )
                    ),
                // Если в try (при создании объекта) вылетело исключение — выполнение прервется,
                // мгновенно выполнится finally (отпустит замок), и только потом ошибка полетит дальше «наверх».
                Expression.Call(typeof(Monitor), "Exit", null, lockObject)
            ),
            // Третье выражение в блоке
            Expression.Convert(valueField, typeof(object))
        );
        // Возврат выражения
        return Expression.Condition(
            checkNotNull,   // проверяемое выражение
            returnCached,   // выражение если истина
            lockCreateNew,  // выражение если ложь
            typeof(object)  // тип значения которое будет возвращено в случае истина и ложь
        );
    }

    protected override object VisitCache(ServiceCallSite callSite, ParameterExpression scope)
    {
        // Для Scoped: используем словарь в scope
        var callSiteConstant = Expression.Constant(callSite);
        
        // scope.ResolvedServices.GetOrAdd(callSite, key => CreateService(key, scope))
        var resolvedServicesProperty = Expression.Property(
            scope, 
            nameof(ServiceProviderEngineScope.ResolvedServices));
        
        var getOrAddMethod = typeof(ConcurrentDictionary<ServiceCallSite, object>)
            .GetMethod("GetOrAdd", new[] { typeof(ServiceCallSite), typeof(Func<ServiceCallSite, object>) });
        
        // Лямбда для создания сервиса
        var keyParam = Expression.Parameter(typeof(ServiceCallSite), "key");
        var createServiceCall = (Expression)VisitCallSiteMain(callSite, scope);
        
        // Создаем переменную для результата
        var resolvedVar = Expression.Variable(typeof(object), "resolved");
        
        // Блок: создаем объект, вызываем CaptureDisposable, возвращаем
        var createBlock = Expression.Block(
            new[] { resolvedVar },
            Expression.Assign(resolvedVar, createServiceCall),
            Expression.Call(scope, typeof(ServiceProviderEngineScope).GetMethod("CaptureDisposable")!, resolvedVar),
            resolvedVar
        );
        
        var createLambda = Expression.Lambda<Func<ServiceCallSite, object>>(
            createBlock,
            keyParam);
        
        return Expression.Call(
            resolvedServicesProperty,
            getOrAddMethod!,
            callSiteConstant,
            createLambda);
    }

    protected override object VisitNoCache(ServiceCallSite callSite, ParameterExpression scope)
    {
        // Для Transient: просто создаем новый экземпляр
        var createServiceCall = (Expression)VisitCallSiteMain(callSite, scope);
        
        // Создаем переменную для результата
        var resolvedVar = Expression.Variable(typeof(object), "resolved");
        
        // Блок: создаем объект, вызываем CaptureDisposable, возвращаем
        return Expression.Block(
            new[] { resolvedVar },
            Expression.Assign(resolvedVar, createServiceCall),
            Expression.Call(scope, typeof(ServiceProviderEngineScope).GetMethod("CaptureDisposable")!, resolvedVar),
            resolvedVar
        );
    }
}