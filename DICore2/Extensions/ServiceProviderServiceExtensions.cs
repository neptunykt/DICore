namespace DICore2.Extensions;
using IServiceProvider = DICore2.Abstractions.IServiceProvider;
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