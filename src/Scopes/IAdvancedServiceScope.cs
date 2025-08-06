using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zentient.Abstractions.DependencyInjection.Resolution;

namespace Zentient.DependencyInjection.Scopes
{
    /// <summary>
    /// Represents a sophisticated, async-aware service scope that provides isolated resolution
    /// of scoped services with comprehensive lifecycle management, diagnostics, and context awareness.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface extends the basic scope concept with advanced features:
    /// - Async-first disposal patterns for modern applications
    /// - Rich diagnostic information for debugging and monitoring
    /// - Context-aware resolution with strongly-typed contexts
    /// - Hierarchical scope relationships for complex scenarios
    /// - Event notifications for scope lifecycle management
    /// - Performance tracking and resource monitoring
    /// </para>
    /// <para>
    /// The scope automatically manages the lifecycle of all scoped services created within it,
    /// ensuring proper disposal in the correct order (reverse dependency order).
    /// </para>
    /// </remarks>
    public interface IAdvancedServiceScope : IServiceResolver, IDisposable, IAsyncDisposable
    {
        // ================================================================================
        // SCOPE IDENTITY AND METADATA
        // ================================================================================

        /// <summary>
        /// Gets the unique identifier for this scope instance.
        /// Useful for debugging, logging, and tracking scope relationships.
        /// </summary>
        Guid ScopeId { get; }

        /// <summary>
        /// Gets the human-readable name for this scope.
        /// Can be set during scope creation for better diagnostics.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Gets the creation timestamp for this scope.
        /// Useful for performance monitoring and lifecycle tracking.
        /// </summary>
        DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets the parent scope if this is a child scope, null for root scopes.
        /// Enables hierarchical scope relationships and dependency tracking.
        /// </summary>
        IAdvancedServiceScope? Parent { get; }

        /// <summary>
        /// Gets metadata associated with this scope.
        /// Can include request context, user information, tenant data, etc.
        /// </summary>
        IReadOnlyDictionary<string, object> Metadata { get; }

        // ================================================================================
        // SCOPE STATE AND LIFECYCLE
        // ================================================================================

        /// <summary>
        /// Gets the current state of this scope.
        /// Provides visibility into the scope lifecycle for diagnostics and validation.
        /// </summary>
        ScopeState State { get; }

        /// <summary>
        /// Gets whether this scope has been disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Gets whether this scope is currently disposing.
        /// Useful for preventing new service resolutions during disposal.
        /// </summary>
        bool IsDisposing { get; }

        /// <summary>
        /// Event raised when the scope state changes.
        /// Useful for monitoring, logging, and cleanup operations.
        /// </summary>
        event EventHandler<ScopeStateChangedEventArgs>? StateChanged;

        // ================================================================================
        // CHILD SCOPE MANAGEMENT
        // ================================================================================

        /// <summary>
        /// Creates a child scope that inherits from this scope's context and configuration.
        /// Child scopes are automatically disposed when the parent is disposed.
        /// </summary>
        /// <param name="name">Optional name for the child scope.</param>
        /// <param name="configure">Optional configuration for the child scope.</param>
        /// <returns>A new child scope instance.</returns>
        /// <example>
        /// <code>
        /// using var childScope = parentScope.CreateChildScope("operation-123", config => config
        ///     .WithMetadata("Operation", "ProcessPayment")
        ///     .WithTimeout(TimeSpan.FromMinutes(5)));
        /// </code>
        /// </example>
        IAdvancedServiceScope CreateChildScope(
            string? name = null,
            Action<IScopeConfiguration>? configure = null);

        /// <summary>
        /// Creates a child scope asynchronously with advanced initialization.
        /// </summary>
        /// <param name="name">Optional name for the child scope.</param>
        /// <param name="configure">Optional configuration for the child scope.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task containing the new child scope instance.</returns>
        Task<IAdvancedServiceScope> CreateChildScopeAsync(
            string? name = null,
            Action<IScopeConfiguration>? configure = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a read-only collection of active child scopes.
        /// Useful for diagnostics and understanding scope hierarchies.
        /// </summary>
        IReadOnlyCollection<IAdvancedServiceScope> ChildScopes { get; }

        // ================================================================================
        // ENHANCED RESOLUTION WITH SCOPE CONTEXT
        // ================================================================================

        /// <summary>
        /// Resolves a service with scope-specific context information.
        /// This allows services to access scope metadata during construction.
        /// </summary>
        /// <typeparam name="TContract">The contract type to resolve.</typeparam>
        /// <param name="scopeContext">Scope-specific context for resolution.</param>
        /// <param name="predicate">Optional predicate for service filtering.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The resolved service with scope context applied.</returns>
        Task<TContract?> ResolveWithScopeContextAsync<TContract>(
            IScopeContext scopeContext,
            Func<ServiceDescriptor, bool>? predicate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves services that are explicitly registered as scope-aware.
        /// These services receive the current scope as a dependency.
        /// </summary>
        /// <typeparam name="TContract">The contract type to resolve.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The resolved scope-aware service.</returns>
        Task<TContract?> ResolveScopeAwareAsync<TContract>(
            CancellationToken cancellationToken = default)
            where TContract : class, IScopeAware;

        /// <summary>
        /// Attempts to resolve a service, returning a result indicating success or failure.
        /// </summary>
        /// <typeparam name="TContract">The contract type to resolve.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A resolution result with the service instance or error information.</returns>
        Task<ServiceResolutionResult<TContract>> TryResolveAsync<TContract>(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves all registered implementations of a service contract.
        /// </summary>
        /// <typeparam name="TContract">The contract type to resolve.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An enumerable of all resolved service instances.</returns>
        IAsyncEnumerable<TContract> ResolveAllAsync<TContract>(
            CancellationToken cancellationToken = default);

        // ================================================================================
        // DIAGNOSTICS AND MONITORING
        // ================================================================================

        /// <summary>
        /// Gets comprehensive diagnostic information about this scope.
        /// Includes service resolution statistics, memory usage, and performance metrics.
        /// </summary>
        IScopeDiagnostics Diagnostics { get; }

        /// <summary>
        /// Gets the current performance metrics for this scope.
        /// Useful for monitoring and optimization.
        /// </summary>
        IScopePerformanceMetrics PerformanceMetrics { get; }

        /// <summary>
        /// Validates the current state of the scope and all its services.
        /// Can help identify potential issues before they cause problems.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A validation report with any issues found.</returns>
        Task<IScopeValidationReport> ValidateAsync(CancellationToken cancellationToken = default);

        // ================================================================================
        // ADVANCED LIFECYCLE MANAGEMENT
        // ================================================================================

        /// <summary>
        /// Registers a callback to be executed when the scope is disposed.
        /// Callbacks are executed in reverse registration order (LIFO).
        /// </summary>
        /// <param name="callback">The callback to execute on disposal.</param>
        /// <param name="executeAsync">Whether to execute the callback asynchronously.</param>
        void RegisterDisposalCallback(Action callback, bool executeAsync = false);

        /// <summary>
        /// Registers an async callback to be executed when the scope is disposed.
        /// </summary>
        /// <param name="callback">The async callback to execute on disposal.</param>
        void RegisterAsyncDisposalCallback(Func<Task> callback);

        /// <summary>
        /// Manually triggers disposal of a specific service within the scope.
        /// Useful for early cleanup of expensive resources.
        /// </summary>
        /// <param name="service">The service instance to dispose.</param>
        /// <returns>A task representing the disposal operation.</returns>
        Task DisposeServiceAsync(object service);

        // ================================================================================
        // RESOURCE MANAGEMENT
        // ================================================================================

        /// <summary>
        /// Gets the current resource usage information for this scope.
        /// Includes memory consumption, active services count, etc.
        /// </summary>
        IScopeResourceInfo ResourceInfo { get; }

        /// <summary>
        /// Sets a timeout for this scope. The scope will be automatically disposed
        /// when the timeout expires to prevent resource leaks.
        /// </summary>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="callback">Optional callback to execute on timeout.</param>
        void SetTimeout(TimeSpan timeout, Action<IAdvancedServiceScope>? callback = null);

        /// <summary>
        /// Clears the timeout previously set for this scope.
        /// </summary>
        void ClearTimeout();
    }

    /// <summary>
    /// Sophisticated factory for creating advanced service scopes with comprehensive
    /// configuration options, monitoring capabilities, and lifecycle management.
    /// </summary>
    /// <remarks>
    /// This factory provides multiple creation patterns:
    /// - Simple scope creation for basic scenarios
    /// - Configured scope creation with rich options
    /// - Async scope creation for complex initialization
    /// - Template-based scope creation for consistency
    /// - Scope pooling for performance optimization
    /// </remarks>
    public interface IAdvancedServiceScopeFactory
    {
        // ================================================================================
        // BASIC SCOPE CREATION
        // ================================================================================

        /// <summary>
        /// Creates a new service scope with default configuration.
        /// </summary>
        /// <returns>A new service scope instance.</returns>
        IAdvancedServiceScope CreateScope();

        /// <summary>
        /// Creates a scope from the standard IServiceScopeFactory interface for compatibility.
        /// </summary>
        /// <returns>A standard service scope instance.</returns>
        IServiceScope CreateStandardScope();

        // ================================================================================
        // ADVANCED SCOPE CREATION
        // ================================================================================

        /// <summary>
        /// Creates a named scope with custom configuration options.
        /// Named scopes are easier to identify in diagnostics and logging.
        /// </summary>
        /// <param name="name">The name for the scope.</param>
        /// <param name="configure">Optional configuration action.</param>
        /// <returns>A new configured service scope.</returns>
        /// <example>
        /// <code>
        /// var scope = factory.CreateScope("user-request-123", config => config
        ///     .WithMetadata("UserId", userId)
        ///     .WithMetadata("RequestId", requestId)
        ///     .WithTimeout(TimeSpan.FromMinutes(5))
        ///     .EnableDiagnostics());
        /// </code>
        /// </example>
        IAdvancedServiceScope CreateScope(
            string name,
            Action<IScopeConfiguration>? configure = null);

        /// <summary>
        /// Creates a scope asynchronously with advanced initialization support.
        /// Useful when scope creation requires async operations like loading configuration.
        /// </summary>
        /// <param name="name">Optional name for the scope.</param>
        /// <param name="configure">Optional configuration action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the new service scope.</returns>
        Task<IAdvancedServiceScope> CreateScopeAsync(
            string? name = null,
            Action<IScopeConfiguration>? configure = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a scope from a predefined template.
        /// Templates provide consistent configuration across multiple scopes.
        /// </summary>
        /// <param name="template">The scope template to use.</param>
        /// <param name="name">Optional name override for the scope.</param>
        /// <returns>A new service scope based on the template.</returns>
        IAdvancedServiceScope CreateScopeFromTemplate(
            IScopeTemplate template,
            string? name = null);

        /// <summary>
        /// Creates a child scope from an existing parent scope.
        /// Child scopes inherit configuration and context from their parent.
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        /// <param name="name">Optional name for the child scope.</param>
        /// <param name="configure">Optional additional configuration.</param>
        /// <returns>A new child service scope.</returns>
        IAdvancedServiceScope CreateChildScope(
            IAdvancedServiceScope parent,
            string? name = null,
            Action<IScopeConfiguration>? configure = null);

        // ================================================================================
        // SCOPE POOLING AND OPTIMIZATION
        // ================================================================================

        /// <summary>
        /// Gets or creates a pooled scope for improved performance.
        /// Pooled scopes are reused to reduce allocation overhead.
        /// </summary>
        /// <param name="poolKey">Key identifying the scope pool.</param>
        /// <param name="factory">Factory function to create new scopes when pool is empty.</param>
        /// <returns>A pooled service scope.</returns>
        IAdvancedServiceScope GetOrCreatePooledScope(
            string poolKey,
            Func<IAdvancedServiceScope> factory);

        /// <summary>
        /// Returns a scope to the pool for reuse.
        /// The scope is reset to a clean state before being pooled.
        /// </summary>
        /// <param name="scope">The scope to return to the pool.</param>
        /// <param name="poolKey">The pool key where the scope should be returned.</param>
        /// <returns>A task representing the pooling operation.</returns>
        Task ReturnScopeToPoolAsync(IAdvancedServiceScope scope, string poolKey);

        // ================================================================================
        // FACTORY CONFIGURATION AND DIAGNOSTICS
        // ================================================================================

        /// <summary>
        /// Gets configuration options for the scope factory.
        /// Allows customization of default scope behavior.
        /// </summary>
        IScopeFactoryConfiguration Configuration { get; }

        /// <summary>
        /// Gets diagnostic information about the factory and all created scopes.
        /// Useful for monitoring and performance analysis.
        /// </summary>
        IScopeFactoryDiagnostics Diagnostics { get; }

        /// <summary>
        /// Event raised when a new scope is created.
        /// Useful for monitoring and logging scope creation patterns.
        /// </summary>
        event EventHandler<ScopeCreatedEventArgs>? ScopeCreated;

        /// <summary>
        /// Event raised when a scope is disposed.
        /// Useful for cleanup and monitoring scope lifetimes.
        /// </summary>
        event EventHandler<ScopeDisposedEventArgs>? ScopeDisposed;

        // ================================================================================
        // TEMPLATE MANAGEMENT
        // ================================================================================

        /// <summary>
        /// Registers a scope template for reuse across the application.
        /// Templates provide consistent scope configuration.
        /// </summary>
        /// <param name="name">The template name.</param>
        /// <param name="template">The template configuration.</param>
        void RegisterTemplate(string name, IScopeTemplate template);

        /// <summary>
        /// Gets a registered scope template by name.
        /// </summary>
        /// <param name="name">The template name.</param>
        /// <returns>The scope template, or null if not found.</returns>
        IScopeTemplate? GetTemplate(string name);

        /// <summary>
        /// Gets all registered template names.
        /// </summary>
        IReadOnlyCollection<string> TemplateNames { get; }
    }

    // ================================================================================
    // SUPPORTING INTERFACES AND TYPES
    // ================================================================================

    /// <summary>
    /// Standard service scope interface for compatibility with existing DI containers.
    /// </summary>
    public interface IServiceScope : IDisposable
    {
        /// <summary>Gets the service provider for this scope.</summary>
        IServiceProvider ServiceProvider { get; }
    }

    /// <summary>
    /// Configuration interface for advanced scope creation.
    /// Provides a fluent API for configuring scope behavior and metadata.
    /// </summary>
    public interface IScopeConfiguration
    {
        /// <summary>Adds metadata to the scope.</summary>
        IScopeConfiguration WithMetadata(string key, object value);

        /// <summary>Adds multiple metadata entries.</summary>
        IScopeConfiguration WithMetadata(IReadOnlyDictionary<string, object> metadata);

        /// <summary>Sets a timeout for the scope.</summary>
        IScopeConfiguration WithTimeout(TimeSpan timeout);

        /// <summary>Enables detailed diagnostics for the scope.</summary>
        IScopeConfiguration EnableDiagnostics(bool enable = true);

        /// <summary>Enables performance metrics collection.</summary>
        IScopeConfiguration EnablePerformanceMetrics(bool enable = true);

        /// <summary>Sets the maximum number of services that can be resolved in this scope.</summary>
        IScopeConfiguration WithMaxServices(int maxServices);

        /// <summary>Configures disposal callbacks to execute on scope disposal.</summary>
        IScopeConfiguration WithDisposalCallback(Action<IAdvancedServiceScope> callback);

        /// <summary>Configures async disposal callbacks.</summary>
        IScopeConfiguration WithAsyncDisposalCallback(Func<IAdvancedServiceScope, Task> callback);
    }

    /// <summary>
    /// Template for creating scopes with consistent configuration.
    /// </summary>
    public interface IScopeTemplate
    {
        /// <summary>Gets the template name.</summary>
        string Name { get; }

        /// <summary>Gets the template description.</summary>
        string? Description { get; }

        /// <summary>Gets the default metadata for scopes created from this template.</summary>
        IReadOnlyDictionary<string, object> DefaultMetadata { get; }

        /// <summary>Gets the default timeout for scopes created from this template.</summary>
        TimeSpan? DefaultTimeout { get; }

        /// <summary>Gets whether diagnostics should be enabled by default.</summary>
        bool EnableDiagnostics { get; }

        /// <summary>Applies this template's configuration to a scope configuration.</summary>
        void ApplyTo(IScopeConfiguration configuration);
    }

    /// <summary>
    /// Context interface for scope-aware resolution.
    /// Provides access to scope information during service construction.
    /// </summary>
    public interface IScopeContext
    {
        /// <summary>Gets the current scope.</summary>
        IAdvancedServiceScope Scope { get; }

        /// <summary>Gets scope-specific metadata.</summary>
        IReadOnlyDictionary<string, object> ScopeMetadata { get; }

        /// <summary>Gets additional context data.</summary>
        IReadOnlyDictionary<string, object> ContextData { get; }

        /// <summary>Gets a context value by key.</summary>
        T? GetValue<T>(string key);

        /// <summary>Sets a context value.</summary>
        void SetValue<T>(string key, T value);
    }

    /// <summary>
    /// Interface for services that are aware of their containing scope.
    /// Scope-aware services receive the current scope during construction.
    /// </summary>
    public interface IScopeAware
    {
        /// <summary>Gets the scope this service was created in.</summary>
        IAdvancedServiceScope Scope { get; }
    }

    /// <summary>
    /// Result of a service resolution operation.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    public class ServiceResolutionResult<T>
    {
        /// <summary>Gets whether the resolution was successful.</summary>
        public bool Success { get; }

        /// <summary>Gets the resolved service instance, if successful.</summary>
        public T? Service { get; }

        /// <summary>Gets the error that occurred during resolution, if any.</summary>
        public Exception? Error { get; }

        /// <summary>Gets the time taken to resolve the service.</summary>
        public TimeSpan ResolutionTime { get; }

        /// <summary>Gets additional metadata about the resolution.</summary>
        public IReadOnlyDictionary<string, object> Metadata { get; }

        private ServiceResolutionResult(bool success, T? service, Exception? error, TimeSpan resolutionTime, IReadOnlyDictionary<string, object>? metadata = null)
        {
            Success = success;
            Service = service;
            Error = error;
            ResolutionTime = resolutionTime;
            Metadata = metadata ?? new Dictionary<string, object>();
        }

        /// <summary>Creates a successful resolution result.</summary>
        public static ServiceResolutionResult<T> CreateSuccess(T service, TimeSpan resolutionTime, IReadOnlyDictionary<string, object>? metadata = null) =>
            new(true, service, null, resolutionTime, metadata);

        /// <summary>Creates a failed resolution result.</summary>
        public static ServiceResolutionResult<T> CreateFailure(Exception error, TimeSpan resolutionTime, IReadOnlyDictionary<string, object>? metadata = null) =>
            new(false, default, error, resolutionTime, metadata);
    }

    /// <summary>
    /// Describes a service registration.
    /// </summary>
    public class ServiceDescriptor
    {
        /// <summary>Gets the service type.</summary>
        public Type ServiceType { get; init; } = typeof(object);

        /// <summary>Gets the implementation type.</summary>
        public Type? ImplementationType { get; init; }

        /// <summary>Gets the service lifetime.</summary>
        public ServiceLifetime Lifetime { get; init; }

        /// <summary>Gets the implementation factory, if any.</summary>
        public Func<IServiceProvider, object>? ImplementationFactory { get; init; }

        /// <summary>Gets the implementation instance, if any.</summary>
        public object? ImplementationInstance { get; init; }

        /// <summary>Gets service metadata.</summary>
        public IReadOnlyDictionary<string, object> Metadata { get; init; } = 
            new Dictionary<string, object>();
    }

    /// <summary>
    /// Comprehensive diagnostic information about a service scope.
    /// </summary>
    public interface IScopeDiagnostics
    {
        /// <summary>Gets the scope this diagnostic information belongs to.</summary>
        Guid ScopeId { get; }

        /// <summary>Gets the total number of services resolved in this scope.</summary>
        int ServicesResolved { get; }

        /// <summary>Gets the number of currently active services.</summary>
        int ActiveServices { get; }

        /// <summary>Gets the number of disposed services.</summary>
        int DisposedServices { get; }

        /// <summary>Gets the resolution history for this scope.</summary>
        IReadOnlyList<ServiceResolutionRecord> ResolutionHistory { get; }

        /// <summary>Gets information about currently active services.</summary>
        IReadOnlyList<ActiveServiceInfo> ActiveServiceInfo { get; }

        /// <summary>Gets the dependency graph for this scope.</summary>
        IDependencyGraph DependencyGraph { get; }
    }

    /// <summary>
    /// Performance metrics for a service scope.
    /// </summary>
    public interface IScopePerformanceMetrics
    {
        /// <summary>Gets the total time spent resolving services.</summary>
        TimeSpan TotalResolutionTime { get; }

        /// <summary>Gets the average time per service resolution.</summary>
        TimeSpan AverageResolutionTime { get; }

        /// <summary>Gets the peak memory usage for this scope.</summary>
        long PeakMemoryUsage { get; }

        /// <summary>Gets the current memory usage for this scope.</summary>
        long CurrentMemoryUsage { get; }

        /// <summary>Gets the number of garbage collections triggered during scope lifetime.</summary>
        int GarbageCollections { get; }
    }

    /// <summary>
    /// Resource usage information for a service scope.
    /// </summary>
    public interface IScopeResourceInfo
    {
        /// <summary>Gets the current memory usage in bytes.</summary>
        long MemoryUsage { get; }

        /// <summary>Gets the number of active service instances.</summary>
        int ActiveServiceCount { get; }

        /// <summary>Gets the number of active child scopes.</summary>
        int ChildScopeCount { get; }

        /// <summary>Gets the scope depth (0 for root scopes).</summary>
        int ScopeDepth { get; }
    }

    /// <summary>
    /// Validation report for a service scope.
    /// </summary>
    public interface IScopeValidationReport
    {
        /// <summary>Gets whether the scope is in a valid state.</summary>
        bool IsValid { get; }

        /// <summary>Gets validation errors found.</summary>
        IReadOnlyList<ScopeValidationIssue> Errors { get; }

        /// <summary>Gets validation warnings found.</summary>
        IReadOnlyList<ScopeValidationIssue> Warnings { get; }

        /// <summary>Gets a summary of the validation results.</summary>
        string Summary { get; }
    }

    /// <summary>
    /// Configuration for the scope factory.
    /// </summary>
    public interface IScopeFactoryConfiguration
    {
        /// <summary>Gets the default timeout for scopes.</summary>
        TimeSpan? DefaultScopeTimeout { get; }

        /// <summary>Gets whether diagnostics are enabled by default.</summary>
        bool DiagnosticsEnabledByDefault { get; }

        /// <summary>Gets the maximum scope depth allowed.</summary>
        int MaxScopeDepth { get; }

        /// <summary>Gets the scope pool configuration.</summary>
        IScopePoolConfiguration PoolConfiguration { get; }
    }

    /// <summary>
    /// Diagnostic information about the scope factory.
    /// </summary>
    public interface IScopeFactoryDiagnostics
    {
        /// <summary>Gets the total number of scopes created.</summary>
        int TotalScopesCreated { get; }

        /// <summary>Gets the number of currently active scopes.</summary>
        int ActiveScopes { get; }

        /// <summary>Gets the number of pooled scopes.</summary>
        int PooledScopes { get; }

        /// <summary>Gets scope creation statistics.</summary>
        IReadOnlyDictionary<string, int> CreationStatistics { get; }
    }

    // ================================================================================
    // ENUMS AND VALUE TYPES
    // ================================================================================

    /// <summary>
    /// Represents the state of a service scope.
    /// </summary>
    public enum ScopeState
    {
        /// <summary>The scope is being created.</summary>
        Creating,

        /// <summary>The scope is active and ready for use.</summary>
        Active,

        /// <summary>The scope is being disposed.</summary>
        Disposing,

        /// <summary>The scope has been disposed.</summary>
        Disposed,

        /// <summary>The scope encountered an error.</summary>
        Error
    }

    /// <summary>
    /// Event arguments for scope state changes.
    /// </summary>
    public class ScopeStateChangedEventArgs : EventArgs
    {
        /// <summary>Gets the scope that changed state.</summary>
        public IAdvancedServiceScope Scope { get; init; } = null!;

        /// <summary>Gets the previous state.</summary>
        public ScopeState PreviousState { get; init; }

        /// <summary>Gets the new state.</summary>
        public ScopeState NewState { get; init; }

        /// <summary>Gets the timestamp of the state change.</summary>
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Event arguments for scope creation.
    /// </summary>
    public class ScopeCreatedEventArgs : EventArgs
    {
        /// <summary>Gets the created scope.</summary>
        public IAdvancedServiceScope Scope { get; init; } = null!;

        /// <summary>Gets the creation timestamp.</summary>
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Event arguments for scope disposal.
    /// </summary>
    public class ScopeDisposedEventArgs : EventArgs
    {
        /// <summary>Gets the disposed scope.</summary>
        public IAdvancedServiceScope Scope { get; init; } = null!;

        /// <summary>Gets the disposal timestamp.</summary>
        public DateTimeOffset DisposedAt { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>Gets the scope lifetime duration.</summary>
        public TimeSpan Lifetime { get; init; }
    }

    // ================================================================================
    // ADDITIONAL SUPPORTING TYPES
    // ================================================================================

    /// <summary>
    /// Record of a service resolution operation.
    /// </summary>
    public class ServiceResolutionRecord
    {
        /// <summary>Gets the service type that was resolved.</summary>
        public Type ServiceType { get; init; } = typeof(object);

        /// <summary>Gets the timestamp of the resolution.</summary>
        public DateTimeOffset ResolvedAt { get; init; }

        /// <summary>Gets the time taken to resolve the service.</summary>
        public TimeSpan ResolutionTime { get; init; }

        /// <summary>Gets whether the resolution was successful.</summary>
        public bool Success { get; init; }

        /// <summary>Gets any error that occurred during resolution.</summary>
        public Exception? Error { get; init; }
    }

    /// <summary>
    /// Information about an active service in a scope.
    /// </summary>
    public class ActiveServiceInfo
    {
        /// <summary>Gets the service type.</summary>
        public Type ServiceType { get; init; } = typeof(object);

        /// <summary>Gets the service instance.</summary>
        public object Instance { get; init; } = null!;

        /// <summary>Gets when the service was created.</summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>Gets whether the service implements IDisposable.</summary>
        public bool IsDisposable { get; init; }

        /// <summary>Gets the estimated memory usage of the service.</summary>
        public long EstimatedMemoryUsage { get; init; }
    }

    /// <summary>
    /// Validation issue found in a scope.
    /// </summary>
    public class ScopeValidationIssue
    {
        /// <summary>Gets the issue message.</summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>Gets the issue severity.</summary>
        public ValidationSeverity Severity { get; init; }

        /// <summary>Gets the service type related to the issue, if any.</summary>
        public Type? ServiceType { get; init; }
    }

    /// <summary>
    /// Configuration for scope pooling.
    /// </summary>
    public interface IScopePoolConfiguration
    {
        /// <summary>Gets the maximum number of scopes per pool.</summary>
        int MaxScopesPerPool { get; }

        /// <summary>Gets the maximum time a scope can stay in the pool.</summary>
        TimeSpan MaxPoolTime { get; }

        /// <summary>Gets whether pooling is enabled.</summary>
        bool PoolingEnabled { get; }
    }

    /// <summary>
    /// Dependency graph for analyzing service relationships.
    /// </summary>
    public interface IDependencyGraph
    {
        /// <summary>Gets all nodes in the dependency graph.</summary>
        IReadOnlyList<DependencyNode> Nodes { get; }

        /// <summary>Gets all edges in the dependency graph.</summary>
        IReadOnlyList<DependencyEdge> Edges { get; }

        /// <summary>Finds circular dependencies in the graph.</summary>
        IReadOnlyList<CircularDependency> FindCircularDependencies();
    }

    /// <summary>
    /// Node in a dependency graph.
    /// </summary>
    public class DependencyNode
    {
        /// <summary>Gets the service type for this node.</summary>
        public Type ServiceType { get; init; } = typeof(object);

        /// <summary>Gets the service lifetime.</summary>
        public ServiceLifetime Lifetime { get; init; }

        /// <summary>Gets when this service was resolved.</summary>
        public DateTimeOffset? ResolvedAt { get; init; }
    }

    /// <summary>
    /// Edge in a dependency graph representing a dependency relationship.
    /// </summary>
    public class DependencyEdge
    {
        /// <summary>Gets the dependent service type.</summary>
        public Type From { get; init; } = typeof(object);

        /// <summary>Gets the dependency service type.</summary>
        public Type To { get; init; } = typeof(object);

        /// <summary>Gets the relationship type.</summary>
        public DependencyType Type { get; init; }
    }

    /// <summary>
    /// Circular dependency in the dependency graph.
    /// </summary>
    public class CircularDependency
    {
        /// <summary>Gets the chain of types involved in the circular dependency.</summary>
        public IReadOnlyList<Type> Chain { get; init; } = Array.Empty<Type>();

        /// <summary>Gets a description of the circular dependency.</summary>
        public string Description { get; init; } = string.Empty;
    }

    /// <summary>
    /// Type of dependency relationship.
    /// </summary>
    public enum DependencyType
    {
        /// <summary>Constructor parameter dependency.</summary>
        Constructor,

        /// <summary>Property injection dependency.</summary>
        Property,

        /// <summary>Method injection dependency.</summary>
        Method,

        /// <summary>Factory dependency.</summary>
        Factory
    }

    /// <summary>
    /// Validation severity levels.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>Informational message.</summary>
        Info,

        /// <summary>Warning that should be addressed.</summary>
        Warning,

        /// <summary>Error that must be fixed.</summary>
        Error,

        /// <summary>Critical error preventing scope operation.</summary>
        Critical
    }

    /// <summary>
    /// Service lifetime enumeration.
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>A new instance is created every time it's requested.</summary>
        Transient,

        /// <summary>A single instance per scope.</summary>
        Scoped,

        /// <summary>A single instance for the entire application lifetime.</summary>
        Singleton
    }
}
