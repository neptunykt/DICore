namespace DICore3.Abstractions;

public static class ServiceProviderServiceExtensions
{
    /// <summary>
    /// Get service of type <typeparamref name="T"/> from the <see cref="Abstractions.IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="provider">The <see cref="Abstractions.IServiceProvider"/> to retrieve the service object from.</param>
    /// <returns>A service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
    public static T? GetService<T>(this IServiceProvider provider)
    {

        return (T?)provider.GetService(typeof(T));
    }
        
}