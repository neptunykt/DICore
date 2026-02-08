using IServiceProvider = DICore3.Abstractions.IServiceProvider;

namespace DICore3.Extensions;

public static class ServiceProviderServiceExtensions
{
    public static T? GetService<T>(this IServiceProvider provider)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        return (T?)provider.GetService(typeof(T));
    }
}