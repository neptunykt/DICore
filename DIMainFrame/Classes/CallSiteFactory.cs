namespace DIMainFrame.Classes;

internal sealed class CallSiteFactory
{
    public static ServiceCallSite GetCallSite(ServiceIdentifier serviceIdentifier)
    {
        // Пример: Если просят ICloset, создаем Closet, которому нужен Door и Hinges
        if (serviceIdentifier.ServiceType == typeof(ICloset))
        {
            return new ConstructorCallSite
            {
                Name = "ICloset",
                ServiceType = typeof(ICloset),
                ImplementationType = typeof(Closet),
                Kind = CallSiteKind.Constructor,
                Lifetime = ServiceLifetime.Scoped,
                ConstructorInfo = typeof(Closet).GetConstructors().First(),
                ParameterCallSites = new ServiceCallSite [] 
                { 
                    // Рекурсивно создаем CallSite для зависимости
                    GetCallSite(new ServiceIdentifier { ServiceType = typeof(IDoor) }),
                    GetCallSite(new ServiceIdentifier {ServiceType = typeof(IHinges)})
                }
            };
        }

        if (serviceIdentifier.ServiceType == typeof(IDoor))
        {
            // Базовый случай (зависимостей нет)
            return new ConstructorCallSite
            {
                Name = "IDoor",
                ServiceType = typeof(IDoor),
                ImplementationType = typeof(Door),
                Kind = CallSiteKind.Constructor,
                Lifetime = ServiceLifetime.Scoped,
                ConstructorInfo = typeof(Door).GetConstructors().First()
            };
        }

        if (serviceIdentifier.ServiceType == typeof(IHinges))
        {
            return new ConstructorCallSite
            {
                Name = "IHinges",
                ServiceType = typeof(IHinges),
                ImplementationType = typeof(Hinges),
                Kind = CallSiteKind.Constructor,
                Lifetime = ServiceLifetime.Scoped,
                ConstructorInfo = typeof(Hinges).GetConstructors().First()
            };
        }

        throw new ArgumentException();
    } 
}