namespace Zentient.Abstractions.DependencyInjection.Scopes
{
    /// <summary>
    /// Represents a service scope that provides a mechanism to resolve scoped services.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="IServiceScope"/> is a lightweight container that disposes of all
    /// services created within it, adhering to the <see cref="ServiceLifetime.Scoped"/> lifetime.
    /// It inherits from <see cref="IServiceResolver"/> to allow for service resolution within its scope.
    /// </remarks>
    public interface IServiceScope : IDisposable, IServiceResolver { }

    /// <summary>
    /// A factory for creating new instances of <see cref="IServiceScope"/>.
    /// </summary>
    /// <remarks>
    /// This abstraction allows a consumer to create isolated service scopes, typically for
    /// handling a single logical operation, such as a web request or a message processing job.
    /// </remarks>
    public interface IServiceScopeFactory
    {
        /// <summary>
        /// Creates a new <see cref="IServiceScope"/> that can be used to resolve scoped services.
        /// </summary>
        /// <returns>A new <see cref="IServiceScope"/> instance.</returns>
        IServiceScope CreateScope();
    }
}
namespace Zentient.Abstractions.DependencyInjection.Scopes
{
    /// <summary>
    /// Represents a service scope that provides a mechanism to resolve scoped services.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="IServiceScope"/> is a lightweight container that disposes of all
    /// services created within it, adhering to the <see cref="ServiceLifetime.Scoped"/> lifetime.
    /// It inherits from <see cref="IServiceResolver"/> to allow for service resolution within its scope.
    /// </remarks>
    public interface IServiceScope : IDisposable, IServiceResolver { }

    /// <summary>
    /// A factory for creating new instances of <see cref="IServiceScope"/>.
    /// </summary>
    /// <remarks>
    /// This abstraction allows a consumer to create isolated service scopes, typically for
    /// handling a single logical operation, such as a web request or a message processing job.
    /// </remarks>
    public interface IServiceScopeFactory
    {
        /// <summary>
        /// Creates a new <see cref="IServiceScope"/> that can be used to resolve scoped services.
        /// </summary>
        /// <returns>A new <see cref="IServiceScope"/> instance.</returns>
        IServiceScope CreateScope();
    }
}
