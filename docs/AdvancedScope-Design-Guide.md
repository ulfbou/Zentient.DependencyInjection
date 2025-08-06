# Advanced Service Scope Design Guide

## Overview

The Zentient.DependencyInjection advanced scope system provides sophisticated, DX-friendly interfaces for managing service lifetimes with comprehensive diagnostics, monitoring, and lifecycle management capabilities. This system extends basic dependency injection patterns with enterprise-grade features for modern applications.

## Key Design Principles

### 1. **Async-First Architecture**
All scope operations are designed with async/await patterns as first-class citizens, enabling non-blocking operations and better scalability.

### 2. **Rich Diagnostics and Monitoring**
Every scope provides comprehensive diagnostic information, performance metrics, and validation capabilities for production monitoring and debugging.

### 3. **Hierarchical Scope Relationships**
Support for parent-child scope relationships enables logical operation isolation while maintaining resource efficiency.

### 4. **Context-Aware Resolution**
Services can receive scope-specific context and metadata during construction, enabling sophisticated dependency injection patterns.

### 5. **Resource Management and Optimization**
Built-in support for scope pooling, timeout management, and resource monitoring helps optimize performance and prevent resource leaks.

## Core Interfaces

### IAdvancedServiceScope

The primary interface for sophisticated service scopes with the following capabilities:

#### **Identity and Metadata**
```csharp
Guid ScopeId { get; }                    // Unique identifier
string? Name { get; }                    // Human-readable name
DateTimeOffset CreatedAt { get; }        // Creation timestamp
IAdvancedServiceScope? Parent { get; }   // Parent scope reference
IReadOnlyDictionary<string, object> Metadata { get; }  // Scope metadata
```

#### **State Management**
```csharp
ScopeState State { get; }                // Current scope state
bool IsDisposed { get; }                 // Disposal status
bool IsDisposing { get; }                // Disposal in progress
event EventHandler<ScopeStateChangedEventArgs>? StateChanged;  // State notifications
```

#### **Child Scope Management**
```csharp
IAdvancedServiceScope CreateChildScope(string? name = null, Action<IScopeConfiguration>? configure = null);
Task<IAdvancedServiceScope> CreateChildScopeAsync(string? name = null, Action<IScopeConfiguration>? configure = null, CancellationToken cancellationToken = default);
IReadOnlyCollection<IAdvancedServiceScope> ChildScopes { get; }
```

#### **Enhanced Resolution**
```csharp
Task<TContract?> ResolveWithScopeContextAsync<TContract>(IScopeContext scopeContext, Func<ServiceDescriptor, bool>? predicate = null, CancellationToken cancellationToken = default);
Task<TContract?> ResolveScopeAwareAsync<TContract>(CancellationToken cancellationToken = default) where TContract : class, IScopeAware;
Task<ServiceResolutionResult<TContract>> TryResolveAsync<TContract>(CancellationToken cancellationToken = default);
IAsyncEnumerable<TContract> ResolveAllAsync<TContract>(CancellationToken cancellationToken = default);
```

#### **Diagnostics and Monitoring**
```csharp
IScopeDiagnostics Diagnostics { get; }
IScopePerformanceMetrics PerformanceMetrics { get; }
Task<IScopeValidationReport> ValidateAsync(CancellationToken cancellationToken = default);
```

#### **Lifecycle Management**
```csharp
void RegisterDisposalCallback(Action callback, bool executeAsync = false);
void RegisterAsyncDisposalCallback(Func<Task> callback);
Task DisposeServiceAsync(object service);
```

#### **Resource Management**
```csharp
IScopeResourceInfo ResourceInfo { get; }
void SetTimeout(TimeSpan timeout, Action<IAdvancedServiceScope>? callback = null);
void ClearTimeout();
```

### IAdvancedServiceScopeFactory

The factory interface for creating sophisticated service scopes:

#### **Basic Creation**
```csharp
IAdvancedServiceScope CreateScope();
IServiceScope CreateStandardScope();  // Compatibility with standard DI
```

#### **Advanced Creation**
```csharp
IAdvancedServiceScope CreateScope(string name, Action<IScopeConfiguration>? configure = null);
Task<IAdvancedServiceScope> CreateScopeAsync(string? name = null, Action<IScopeConfiguration>? configure = null, CancellationToken cancellationToken = default);
IAdvancedServiceScope CreateScopeFromTemplate(IScopeTemplate template, string? name = null);
IAdvancedServiceScope CreateChildScope(IAdvancedServiceScope parent, string? name = null, Action<IScopeConfiguration>? configure = null);
```

#### **Pooling and Optimization**
```csharp
IAdvancedServiceScope GetOrCreatePooledScope(string poolKey, Func<IAdvancedServiceScope> factory);
Task ReturnScopeToPoolAsync(IAdvancedServiceScope scope, string poolKey);
```

#### **Template Management**
```csharp
void RegisterTemplate(string name, IScopeTemplate template);
IScopeTemplate? GetTemplate(string name);
IReadOnlyCollection<string> TemplateNames { get; }
```

## Supporting Types and Interfaces

### IScopeConfiguration
Fluent configuration interface for scope creation:
```csharp
IScopeConfiguration WithMetadata(string key, object value);
IScopeConfiguration WithMetadata(IReadOnlyDictionary<string, object> metadata);
IScopeConfiguration WithTimeout(TimeSpan timeout);
IScopeConfiguration EnableDiagnostics(bool enable = true);
IScopeConfiguration EnablePerformanceMetrics(bool enable = true);
IScopeConfiguration WithMaxServices(int maxServices);
IScopeConfiguration WithDisposalCallback(Action<IAdvancedServiceScope> callback);
IScopeConfiguration WithAsyncDisposalCallback(Func<IAdvancedServiceScope, Task> callback);
```

### IScopeTemplate
Template for consistent scope configuration:
```csharp
string Name { get; }
string? Description { get; }
IReadOnlyDictionary<string, object> DefaultMetadata { get; }
TimeSpan? DefaultTimeout { get; }
bool EnableDiagnostics { get; }
void ApplyTo(IScopeConfiguration configuration);
```

### IScopeContext
Context interface for scope-aware resolution:
```csharp
IAdvancedServiceScope Scope { get; }
IReadOnlyDictionary<string, object> ScopeMetadata { get; }
IReadOnlyDictionary<string, object> ContextData { get; }
T? GetValue<T>(string key);
void SetValue<T>(string key, T value);
```

### IScopeAware
Interface for services that need scope information:
```csharp
IAdvancedServiceScope Scope { get; }
```

## Usage Patterns

### 1. **Basic Scope with Metadata**
```csharp
using var scope = factory.CreateScope("web-request-123", config => config
    .WithMetadata("RequestId", "req-123")
    .WithMetadata("UserId", "user-456")
    .WithTimeout(TimeSpan.FromMinutes(5))
    .EnableDiagnostics());

var result = await scope.Resolve<IOrderService>();
if (result.IsSuccess)
{
    var service = result.Value;
    // Use service
}
```

### 2. **Hierarchical Child Scopes**
```csharp
using var parentScope = factory.CreateScope("batch-job");

for (int i = 1; i <= 5; i++)
{
    using var childScope = parentScope.CreateChildScope($"batch-{i}", config => config
        .WithMetadata("BatchNumber", i)
        .WithTimeout(TimeSpan.FromMinutes(30)));
    
    var processor = await childScope.Resolve<IDataProcessor>();
    // Process batch
}
```

### 3. **Context-Aware Resolution**
```csharp
var scopeContext = new CustomScopeContext(scope)
{
    ContextData = new Dictionary<string, object>
    {
        ["OperationType"] = "BulkUpdate",
        ["Priority"] = "High"
    }
};

var service = await scope.ResolveWithScopeContextAsync<IContextAwareService>(
    scopeContext,
    descriptor => descriptor.Metadata.ContainsKey("SupportsContext"));
```

### 4. **Scope Pooling for Performance**
```csharp
var scope = factory.GetOrCreatePooledScope("read-operations", () =>
    factory.CreateScope("read-scope", config => config
        .WithMetadata("PoolType", "ReadOperations")
        .EnablePerformanceMetrics()));

try
{
    var service = await scope.Resolve<IDataService>();
    // Use service
}
finally
{
    await factory.ReturnScopeToPoolAsync(scope, "read-operations");
}
```

### 5. **Template-Based Scope Creation**
```csharp
// Register template
factory.RegisterTemplate("WebApiRequest", new ScopeTemplate
{
    Name = "WebApiRequest",
    DefaultTimeout = TimeSpan.FromMinutes(2),
    EnableDiagnostics = false,
    DefaultMetadata = new Dictionary<string, object>
    {
        ["ScopeType"] = "WebApi",
        ["OptimizedForPerformance"] = true
    }
});

// Use template
using var scope = factory.CreateScopeFromTemplate(
    factory.GetTemplate("WebApiRequest")!, 
    "api-request-456");
```

### 6. **Comprehensive Monitoring**
```csharp
using var scope = factory.CreateScope("monitoring-demo", config => config
    .EnableDiagnostics()
    .EnablePerformanceMetrics());

// Subscribe to events
scope.StateChanged += (sender, args) =>
{
    Console.WriteLine($"Scope state changed to {args.NewState}");
};

// Check metrics
var metrics = scope.PerformanceMetrics;
Console.WriteLine($"Memory usage: {metrics.CurrentMemoryUsage:N0} bytes");
Console.WriteLine($"Resolution time: {metrics.TotalResolutionTime.TotalMilliseconds:F2}ms");

// Validate scope health
var validation = await scope.ValidateAsync();
if (!validation.IsValid)
{
    Console.WriteLine($"Scope issues: {validation.Summary}");
}
```

### 7. **Advanced Lifecycle Management**
```csharp
using var scope = factory.CreateScope("lifecycle-demo", config => config
    .WithTimeout(TimeSpan.FromMinutes(5))
    .WithDisposalCallback(scope => Console.WriteLine("Cleanup started"))
    .WithAsyncDisposalCallback(async scope =>
    {
        await CleanupResourcesAsync();
        Console.WriteLine("Async cleanup completed");
    }));

// Register additional callbacks
scope.RegisterAsyncDisposalCallback(async () =>
{
    await NotifySystemOfScopeDisposal();
});

// Set custom timeout with callback
scope.SetTimeout(TimeSpan.FromMinutes(10), timedOutScope =>
{
    Logger.Warning($"Scope {timedOutScope.ScopeId} timed out");
});
```

## Diagnostic and Monitoring Features

### Performance Metrics
- Total and average resolution times
- Memory usage tracking (current and peak)
- Garbage collection impact monitoring
- Service resolution counts

### Scope Diagnostics
- Resolution history with timestamps
- Active service information
- Dependency graph analysis
- Circular dependency detection

### Resource Information
- Memory consumption tracking
- Active service counts
- Child scope monitoring
- Scope depth analysis

### Validation Reports
- Scope health validation
- Error and warning detection
- Comprehensive summary reporting
- Service-specific issue identification

## Best Practices

### 1. **Use Named Scopes for Debugging**
Always provide meaningful names for scopes to improve diagnostic capabilities:
```csharp
var scope = factory.CreateScope($"user-{userId}-request-{requestId}");
```

### 2. **Enable Diagnostics in Development**
Use comprehensive diagnostics during development and testing:
```csharp
var scope = factory.CreateScope("dev-scope", config => config
    .EnableDiagnostics()
    .EnablePerformanceMetrics());
```

### 3. **Use Scope Pooling for High-Frequency Operations**
For operations that create many short-lived scopes, use pooling:
```csharp
var scope = factory.GetOrCreatePooledScope("api-requests", scopeFactory);
```

### 4. **Implement Scope-Aware Services When Needed**
Services that need scope information should implement `IScopeAware`:
```csharp
public class ScopeAwareLogger : IScopeAware, ILogger
{
    public IAdvancedServiceScope Scope { get; }
    
    public void Log(string message)
    {
        var requestId = Scope.Metadata.GetValueOrDefault("RequestId");
        // Log with scope context
    }
}
```

### 5. **Use Templates for Consistent Configuration**
Define templates for common scope patterns:
```csharp
// Define once
factory.RegisterTemplate("BackgroundJob", template);

// Use everywhere
var scope = factory.CreateScopeFromTemplate(factory.GetTemplate("BackgroundJob")!);
```

### 6. **Monitor Scope Health in Production**
Implement monitoring for scope metrics and validation:
```csharp
var validation = await scope.ValidateAsync();
if (!validation.IsValid)
{
    _telemetry.RecordScopeIssue(scope.ScopeId, validation);
}
```

### 7. **Set Appropriate Timeouts**
Always set timeouts to prevent resource leaks:
```csharp
scope.SetTimeout(TimeSpan.FromMinutes(30), timedOutScope =>
{
    _logger.Warning($"Scope {timedOutScope.ScopeId} timed out and was disposed");
});
```

## Advanced Scenarios

### Multi-Tenant Applications
```csharp
var scope = factory.CreateScope($"tenant-{tenantId}", config => config
    .WithMetadata("TenantId", tenantId)
    .WithMetadata("TenantPlan", tenantPlan));

var tenantService = await scope.ResolveWithScopeContextAsync<ITenantService>(
    new TenantScopeContext(scope, tenantId));
```

### Request-Response Pipelines
```csharp
using var requestScope = factory.CreateScope($"request-{requestId}", config => config
    .WithMetadata("RequestId", requestId)
    .WithMetadata("UserId", userId)
    .WithTimeout(requestTimeout));

// Process through pipeline stages
using var authScope = requestScope.CreateChildScope("auth");
using var validationScope = requestScope.CreateChildScope("validation");
using var processingScope = requestScope.CreateChildScope("processing");
```

### Background Job Processing
```csharp
using var jobScope = factory.CreateScopeFromTemplate(
    factory.GetTemplate("BackgroundJob")!,
    $"job-{jobId}");

jobScope.RegisterAsyncDisposalCallback(async () =>
{
    await _jobTracker.MarkJobCompleted(jobId);
});

var processor = await jobScope.Resolve<IJobProcessor>();
```

This sophisticated scope system provides enterprise-grade dependency injection capabilities while maintaining developer-friendly APIs and comprehensive monitoring features for production applications.
