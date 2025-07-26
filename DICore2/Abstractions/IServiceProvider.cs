namespace DICore2.Abstractions;

public interface IServiceProvider
{
    /// <summary>Gets the service object of the specified type.</summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>A service object of type <paramref name="serviceType" />.
    /// 
    /// -or-
    /// 
    /// <see langword="null" /> if there is no service object of type <paramref name="serviceType" />.</returns>
    object? GetService(Type serviceType);
}