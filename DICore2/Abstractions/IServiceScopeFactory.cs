namespace DICore2.Abstractions;

public interface IServiceScopeFactory
{
    /// Create an <see cref="Microsoft.Extensions.DependencyInjection.IServiceScope"/> which
    /// contains an <see cref="System.IServiceProvider"/> used to resolve dependencies from a
    /// newly created scope.
    /// </summary>
    /// <returns>
    /// An <see cref="Microsoft.Extensions.DependencyInjection.IServiceScope"/> controlling the
    /// lifetime of the scope. Once this is disposed, any scoped services that have been resolved
    /// from the <see cref="Microsoft.Extensions.DependencyInjection.IServiceScope.ServiceProvider"/>
    /// will also be disposed.
    /// </returns>
    IServiceScope CreateScope();
}