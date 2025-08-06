# Scope Interface Comparison

## Basic vs Advanced Service Scope Interfaces

### **Basic Scope Interfaces (Original)**

```csharp
// Simple scope interface
public interface IServiceScope : IDisposable
{
    IServiceProvider ServiceProvider { get; }
}

// Simple factory interface  
public interface IServiceScopeFactory
{
    IServiceScope CreateScope();
}
```

**Characteristics:**
- ✅ Simple and lightweight
- ✅ Compatible with standard .NET DI
- ❌ No diagnostics or monitoring
- ❌ No metadata support
- ❌ No async disposal patterns
- ❌ No lifecycle management
- ❌ No hierarchical relationships

### **Advanced Scope Interfaces (Sophisticated)**

```csharp
// Sophisticated scope interface with comprehensive features
public interface IAdvancedServiceScope : IServiceResolver, IDisposable, IAsyncDisposable
{
    // Identity and Metadata
    Guid ScopeId { get; }
    string? Name { get; }
    DateTimeOffset CreatedAt { get; }
    IAdvancedServiceScope? Parent { get; }
    IReadOnlyDictionary<string, object> Metadata { get; }
    
    // State Management
    ScopeState State { get; }
    bool IsDisposed { get; }
    bool IsDisposing { get; }
    event EventHandler<ScopeStateChangedEventArgs>? StateChanged;
    
    // Child Scope Management
    IAdvancedServiceScope CreateChildScope(string? name = null, Action<IScopeConfiguration>? configure = null);
    Task<IAdvancedServiceScope> CreateChildScopeAsync(string? name = null, Action<IScopeConfiguration>? configure = null, CancellationToken cancellationToken = default);
    IReadOnlyCollection<IAdvancedServiceScope> ChildScopes { get; }
    
    // Enhanced Resolution
    Task<TContract?> ResolveWithScopeContextAsync<TContract>(IScopeContext scopeContext, Func<ServiceDescriptor, bool>? predicate = null, CancellationToken cancellationToken = default);
    Task<TContract?> ResolveScopeAwareAsync<TContract>(CancellationToken cancellationToken = default) where TContract : class, IScopeAware;
    Task<ServiceResolutionResult<TContract>> TryResolveAsync<TContract>(CancellationToken cancellationToken = default);
    IAsyncEnumerable<TContract> ResolveAllAsync<TContract>(CancellationToken cancellationToken = default);
    
    // Diagnostics and Monitoring
    IScopeDiagnostics Diagnostics { get; }
    IScopePerformanceMetrics PerformanceMetrics { get; }
    Task<IScopeValidationReport> ValidateAsync(CancellationToken cancellationToken = default);
    
    // Advanced Lifecycle Management
    void RegisterDisposalCallback(Action callback, bool executeAsync = false);
    void RegisterAsyncDisposalCallback(Func<Task> callback);
    Task DisposeServiceAsync(object service);
    
    // Resource Management
    IScopeResourceInfo ResourceInfo { get; }
    void SetTimeout(TimeSpan timeout, Action<IAdvancedServiceScope>? callback = null);
    void ClearTimeout();
}

// Sophisticated factory with multiple creation patterns
public interface IAdvancedServiceScopeFactory
{
    // Basic Creation
    IAdvancedServiceScope CreateScope();
    IServiceScope CreateStandardScope();
    
    // Advanced Creation
    IAdvancedServiceScope CreateScope(string name, Action<IScopeConfiguration>? configure = null);
    Task<IAdvancedServiceScope> CreateScopeAsync(string? name = null, Action<IScopeConfiguration>? configure = null, CancellationToken cancellationToken = default);
    IAdvancedServiceScope CreateScopeFromTemplate(IScopeTemplate template, string? name = null);
    IAdvancedServiceScope CreateChildScope(IAdvancedServiceScope parent, string? name = null, Action<IScopeConfiguration>? configure = null);
    
    // Pooling and Optimization
    IAdvancedServiceScope GetOrCreatePooledScope(string poolKey, Func<IAdvancedServiceScope> factory);
    Task ReturnScopeToPoolAsync(IAdvancedServiceScope scope, string poolKey);
    
    // Configuration and Diagnostics
    IScopeFactoryConfiguration Configuration { get; }
    IScopeFactoryDiagnostics Diagnostics { get; }
    event EventHandler<ScopeCreatedEventArgs>? ScopeCreated;
    event EventHandler<ScopeDisposedEventArgs>? ScopeDisposed;
    
    // Template Management
    void RegisterTemplate(string name, IScopeTemplate template);
    IScopeTemplate? GetTemplate(string name);
    IReadOnlyCollection<string> TemplateNames { get; }
}
```

**Characteristics:**
- ✅ Comprehensive diagnostics and monitoring
- ✅ Rich metadata and context support
- ✅ Async-first design patterns
- ✅ Hierarchical scope relationships
- ✅ Advanced lifecycle management
- ✅ Resource monitoring and optimization
- ✅ Scope pooling for performance
- ✅ Template-based configuration
- ✅ Context-aware service resolution
- ✅ Event-driven notifications
- ✅ Validation and health checking
- ✅ Timeout and resource management

## Feature Comparison Matrix

| Feature | Basic Scope | Advanced Scope |
|---------|-------------|----------------|
| **Basic Service Resolution** | ✅ | ✅ |
| **Disposal Management** | ✅ (Sync only) | ✅ (Sync + Async) |
| **Scope Identity & Naming** | ❌ | ✅ |
| **Metadata Support** | ❌ | ✅ |
| **Parent-Child Relationships** | ❌ | ✅ |
| **State Tracking** | ❌ | ✅ |
| **Event Notifications** | ❌ | ✅ |
| **Diagnostics** | ❌ | ✅ |
| **Performance Metrics** | ❌ | ✅ |
| **Memory Monitoring** | ❌ | ✅ |
| **Validation & Health Checks** | ❌ | ✅ |
| **Context-Aware Resolution** | ❌ | ✅ |
| **Scope-Aware Services** | ❌ | ✅ |
| **Custom Disposal Callbacks** | ❌ | ✅ |
| **Timeout Management** | ❌ | ✅ |
| **Scope Pooling** | ❌ | ✅ |
| **Template Configuration** | ❌ | ✅ |
| **Async Resolution Patterns** | ❌ | ✅ |
| **Dependency Graph Analysis** | ❌ | ✅ |
| **Resource Usage Tracking** | ❌ | ✅ |
| **Factory Diagnostics** | ❌ | ✅ |

## Usage Comparison

### Basic Scope Usage
```csharp
// Simple scope creation
using var scope = factory.CreateScope();
var service = scope.ServiceProvider.GetService<IMyService>();

// Limited lifecycle management
// Scope disposes when using block exits
```

### Advanced Scope Usage
```csharp
// Rich scope creation with configuration
using var scope = factory.CreateScope("user-request-123", config => config
    .WithMetadata("RequestId", "req-123")
    .WithMetadata("UserId", "user-456")
    .WithTimeout(TimeSpan.FromMinutes(5))
    .EnableDiagnostics()
    .EnablePerformanceMetrics()
    .WithDisposalCallback(s => Logger.Info($"Scope {s.ScopeId} disposed")));

// Enhanced service resolution
var result = await scope.Resolve<IMyService>();
if (result.IsSuccess)
{
    var service = result.Value;
    
    // Scope-aware services receive scope information
    if (service is IScopeAware scopeAware)
    {
        var requestId = scopeAware.Scope.Metadata["RequestId"];
    }
}

// Child scope creation
using var childScope = scope.CreateChildScope("operation-abc", config => config
    .WithMetadata("Operation", "ProcessOrder")
    .WithTimeout(TimeSpan.FromMinutes(2)));

// Comprehensive monitoring
var metrics = scope.PerformanceMetrics;
Console.WriteLine($"Memory: {metrics.CurrentMemoryUsage:N0} bytes");
Console.WriteLine($"Services: {scope.Diagnostics.ServicesResolved}");

// Health validation
var validation = await scope.ValidateAsync();
if (!validation.IsValid)
{
    Logger.Warning($"Scope issues: {validation.Summary}");
}

// Advanced resource management
scope.SetTimeout(TimeSpan.FromMinutes(10), timedOutScope =>
{
    Logger.Warning($"Scope {timedOutScope.ScopeId} timed out");
});

scope.RegisterAsyncDisposalCallback(async () =>
{
    await CleanupExternalResourcesAsync();
});
```

## When to Use Each Approach

### Use **Basic Scopes** When:
- Building simple applications with minimal DI requirements
- Performance is critical and overhead must be minimized
- Compatibility with standard .NET DI containers is required
- No advanced monitoring or diagnostics are needed
- Scope relationships are simple and flat

### Use **Advanced Scopes** When:
- Building enterprise applications with complex DI requirements
- Comprehensive monitoring and diagnostics are essential
- Resource management and optimization are important
- Scope relationships are hierarchical or complex
- Context-aware service resolution is needed
- Advanced lifecycle management is required
- Performance metrics and validation are important for production monitoring

## Migration Path

If you're currently using basic scopes and want to migrate to advanced scopes:

1. **Phase 1**: Replace `IServiceScopeFactory` with `IAdvancedServiceScopeFactory`
2. **Phase 2**: Use `CreateStandardScope()` for backward compatibility
3. **Phase 3**: Gradually migrate to `CreateScope()` with configuration
4. **Phase 4**: Add diagnostics and monitoring where needed
5. **Phase 5**: Implement scope-aware services and context resolution
6. **Phase 6**: Add advanced features like pooling and templates

The advanced scope system provides a clear upgrade path while maintaining compatibility with existing code through the `CreateStandardScope()` method.
