# Advanced Service Registry Design Guide

## Overview

The **Advanced Service Registry** is the central component of the Zentient Dependency Injection framework, providing sophisticated service registration, discovery, and lifecycle management capabilities. It serves as the authoritative source for all service definitions in the container ecosystem.

## Architecture

### Core Components

1. **IAdvancedServiceRegistry** - Main registry interface providing comprehensive service management
2. **IAdvancedServiceRegistrationBuilder** - Fluent builder for complex service registrations
3. **ServiceRegistryTypes** - Supporting types, configurations, and metadata structures
4. **ServiceRegistryEvents** - Comprehensive event system for monitoring and diagnostics

### Key Features

- **Identity & Metadata Management** - Rich service identification with metadata and tagging
- **Conditional Registration** - Environment and configuration-based service registration
- **Assembly Scanning** - Automated service discovery with convention-based registration
- **Advanced Querying** - Sophisticated service discovery and filtering capabilities
- **Validation & Diagnostics** - Comprehensive validation, dependency analysis, and performance monitoring
- **Event System** - Real-time notifications for all registry operations
- **Import/Export** - Serialization, backup, and registry merging capabilities
- **Performance Optimization** - Caching, parallel operations, and memory management

## Service Registration

### Basic Registration

```csharp
// Simple registration
await registry.RegisterAsync<IUserService, UserService>(ServiceLifetime.Scoped);

// Registration with metadata
await registry.RegisterAsync<IDataService, DatabaseService>(
    ServiceLifetime.Scoped,
    metadata: new Dictionary<string, object>
    {
        ["ConnectionString"] = "Server=localhost;Database=MyApp",
        ["Provider"] = "SqlServer"
    },
    tags: new[] { "data", "primary", "production" }
);
```

### Fluent Registration Builder

```csharp
var registration = await registry.CreateRegistrationBuilder<IPaymentService>()
    .ImplementedBy<StripePaymentService>()
    .WithLifetime(ServiceLifetime.Scoped)
    .WithMetadata("Provider", "Stripe")
    .WithMetadata("ApiVersion", "2023-10-16")
    .WithTags("payment", "external", "production")
    .When(ctx => ctx.IsEnvironment("Production"))
    .When(ctx => ctx.GetConfigurationValue<bool>("Features:EnablePayments", false))
    .WithValidation(options => options.ValidateConstructorParameters = true)
    .WithDiagnostics(options => options.CollectCreationMetrics = true)
    .RegisterAsync();
```

### Factory Registration

```csharp
// Synchronous factory
await registry.CreateRegistrationBuilder<IEmailService>()
    .UsingFactory(provider => new EmailService(
        provider.GetRequiredService<IConfiguration>()["Email:ApiKey"]))
    .WithLifetime(ServiceLifetime.Singleton)
    .RegisterAsync();

// Asynchronous factory
await registry.CreateRegistrationBuilder<IDatabaseService>()
    .UsingAsyncFactory(async provider =>
    {
        var connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("Default");
        var service = new DatabaseService(connectionString);
        await service.InitializeAsync();
        return service;
    })
    .WithLifetime(ServiceLifetime.Scoped)
    .RegisterAsync();
```

## Assembly Scanning

### Convention-Based Registration

```csharp
await registry.ScanAssembliesAsync(
    assemblies: new[] { typeof(Program).Assembly },
    configure: scanner => scanner
        .IncludeTypes(type => !type.IsAbstract && !type.IsInterface)
        .WithAttribute<ServiceAttribute>()
        .WithConventions(conventions =>
        {
            // Services ending with "Service"
            conventions.ForTypesMatching("*Service")
                .RegisterAs(type => type.GetInterfaces().FirstOrDefault() ?? type)
                .UseLifetime(ServiceLifetime.Scoped)
                .WithTags("service", "auto-registered");

            // Repositories
            conventions.ForTypesImplementing<IRepository>()
                .RegisterAs(type => typeof(IRepository<>).MakeGenericType(
                    type.GetGenericArguments().FirstOrDefault() ?? typeof(object)))
                .UseLifetime(ServiceLifetime.Scoped)
                .WithTags("repository", "data-access");

            // Command handlers
            conventions.ForTypesImplementing<ICommandHandler>()
                .UseLifetime(ServiceLifetime.Transient)
                .WithTags("handler", "command")
                .When(ctx => ctx.GetConfigurationValue<bool>("Features:EnableCQRS", true));
        })
        .EnableParallelScanning()
        .WithProgress(progress => 
            Console.WriteLine($"Scanning: {progress.PercentComplete:P0}"))
);
```

### Multi-Assembly Scanning

```csharp
var assemblies = new[]
{
    typeof(ICoreService).Assembly,      // Core layer
    typeof(IDataService).Assembly,      // Data layer
    typeof(IWebService).Assembly        // Web layer
};

await registry.ScanAssembliesAsync(assemblies, scanner => scanner
    .FromNamespace("MyApp.Core", includeSubNamespaces: true)
    .FromNamespace("MyApp.Data.Repositories")
    .FromNamespace("MyApp.Web.Controllers")
    .WithConventions(conventions =>
    {
        conventions.ForTypesInNamespace("MyApp.Core")
            .UseLifetime(ServiceLifetime.Singleton)
            .WithTags("core", "infrastructure");
            
        conventions.ForTypesInNamespace("MyApp.Data")
            .UseLifetime(ServiceLifetime.Scoped)
            .WithTags("data", "persistence");
    }));
```

## Service Discovery and Querying

### Advanced Querying

```csharp
// Find services by criteria
var apiServices = await registry.FindServicesAsync(criteria => criteria
    .WithTag("api")
    .OfLifetime(ServiceLifetime.Scoped)
    .WithMetadata("Version", "2.0")
    .RegisteredAfter(DateTimeOffset.Now.AddDays(-7))
    .ImplementedBy(type => type.Namespace?.StartsWith("MyApp.Api") == true));

// Query with custom predicates
var recentServices = await registry.QueryAsync(service => 
    service.CreatedAt > DateTimeOffset.Now.AddHours(-1) &&
    service.Tags.Contains("critical") &&
    service.ValidationEnabled);

// Find services by multiple tags
var productionServices = await registry.FindServicesByTagsAsync(
    tags: new[] { "production", "critical" },
    matchAll: true);
```

### Service Existence Checks

```csharp
// Check if service is registered
var hasUserService = await registry.IsRegisteredAsync<IUserService>();
var hasTaggedServices = await registry.HasServiceWithTagAsync("payment");

// Count services
var serviceCount = await registry.GetServiceCountAsync();
var scopedCount = await registry.GetServiceCountAsync(ServiceLifetime.Scoped);
```

## Conditional Registration

### Environment-Based Registration

```csharp
// Development services
await registry.CreateRegistrationBuilder<IEmailService>()
    .ImplementedBy<ConsoleEmailService>()
    .WithTags("email", "development")
    .When(ctx => ctx.IsEnvironment("Development"))
    .RegisterAsync();

// Production services
await registry.CreateRegistrationBuilder<IEmailService>()
    .ImplementedBy<SendGridEmailService>()
    .WithTags("email", "production")
    .When(ctx => ctx.IsEnvironment("Production"))
    .When(ctx => !string.IsNullOrEmpty(ctx.GetConfigurationValue<string>("SendGrid:ApiKey")))
    .RegisterAsync();
```

### Configuration-Based Registration

```csharp
// Redis cache when enabled
await registry.CreateRegistrationBuilder<ICacheService>()
    .ImplementedBy<RedisCacheService>()
    .When(ctx => ctx.GetConfigurationValue<string>("Cache:Provider") == "Redis")
    .When(ctx => ctx.GetConfigurationValue<bool>("Cache:Enabled", false))
    .WithMetadata("CacheType", "Distributed")
    .RegisterAsync();

// In-memory cache as fallback
await registry.CreateRegistrationBuilder<ICacheService>()
    .ImplementedBy<MemoryCacheService>()
    .When(ctx => ctx.GetConfigurationValue<string>("Cache:Provider") != "Redis")
    .WithMetadata("CacheType", "InMemory")
    .RegisterAsync();
```

## Validation and Diagnostics

### Registry Validation

```csharp
var validationOptions = new ServiceValidationOptions
{
    ValidateConstructorParameters = true,
    ValidateInterfaceImplementation = true,
    ValidateLifetimeCompatibility = true,
    ValidateDependencyAvailability = true,
    OnValidationFailure = issue => logger.LogWarning("Validation Issue: {Message}", issue.Message)
};

var validationReport = await registry.ValidateAsync(validationOptions);

if (!validationReport.IsValid)
{
    foreach (var error in validationReport.Errors)
    {
        logger.LogError("Validation Error [{Category}]: {Message}", error.Category, error.Message);
    }
}
```

### Dependency Analysis

```csharp
var analysisReport = await registry.AnalyzeDependenciesAsync();

// Check for circular dependencies
if (analysisReport.CircularDependencies.Any())
{
    foreach (var circular in analysisReport.CircularDependencies)
    {
        logger.LogWarning("Circular Dependency: {Description}", circular.Description);
    }
}

// Identify orphaned services
var orphaned = analysisReport.OrphanedServices;
logger.LogInformation("Found {Count} orphaned services", orphaned.Count);

// Get dependency statistics
var stats = analysisReport.Statistics;
logger.LogInformation("Dependency Analysis: {TotalServices} services, {TotalDependencies} dependencies, max depth {MaxDepth}",
    stats.TotalServices, stats.TotalDependencies, stats.MaxDependencyDepth);
```

### Performance Monitoring

```csharp
var performanceMetrics = await registry.GetPerformanceMetricsAsync();

logger.LogInformation("Registry Performance:");
logger.LogInformation("  - Average Registration Time: {Time}ms", 
    performanceMetrics.AverageRegistrationTime.TotalMilliseconds);
logger.LogInformation("  - Average Lookup Time: {Time}ms", 
    performanceMetrics.AverageLookupTime.TotalMilliseconds);
logger.LogInformation("  - Cache Hit Rate: {Rate:P2}", 
    performanceMetrics.CacheHitRate);

// Configure performance monitoring
var monitoringOptions = new PerformanceMonitoringOptions
{
    MonitorCreationTime = true,
    MonitorMemoryUsage = true,
    SamplingRate = 0.1, // 10% sampling
    RetentionPeriod = TimeSpan.FromHours(24)
};

await registry.ConfigurePerformanceMonitoringAsync(monitoringOptions);
```

## Event System

### Event Subscription

```csharp
// Service registration events
await registry.SubscribeToRegistrationEventsAsync(async eventArgs =>
{
    var service = eventArgs.ServiceDescriptor;
    logger.LogInformation("Service registered: {ServiceType} with ID {ServiceId}",
        service.ServiceType.Name, service.Id);
        
    if (service.Tags.Contains("critical"))
    {
        await notificationService.NotifyAsync($"Critical service registered: {service.ServiceType.Name}");
    }
});

// Validation events
await registry.SubscribeToValidationEventsAsync(async eventArgs =>
{
    if (!eventArgs.ValidationReport.IsValid)
    {
        logger.LogWarning("Service validation failed: {ServiceType}",
            eventArgs.ServiceDescriptor.ServiceType.Name);
    }
});

// Performance events
await registry.SubscribeToPerformanceEventsAsync(async eventArgs =>
{
    if (eventArgs.ExceededThreshold != null)
    {
        var threshold = eventArgs.ExceededThreshold;
        logger.LogWarning("Performance threshold exceeded: {ThresholdType} - Expected: {Expected}, Actual: {Actual}",
            threshold.ThresholdType, threshold.ThresholdValue, threshold.CurrentValue);
    }
});

// Error events
await registry.SubscribeToErrorEventsAsync(async eventArgs =>
{
    logger.LogError(eventArgs.Error, "Registry error in operation {Operation}: {Category}",
        eventArgs.Operation, eventArgs.ErrorCategory);
        
    if (!eventArgs.IsRecoverable)
    {
        await emergencyService.HandleCriticalErrorAsync(eventArgs.Error);
    }
});
```

## Import/Export Operations

### Registry Export

```csharp
// Export entire registry
var exportData = await registry.ExportAsync();
await File.WriteAllTextAsync("registry-backup.json", exportData.ToJson());

// Export with filtering
var productionExport = await registry.ExportAsync(criteria => criteria
    .WithTag("production")
    .OfLifetime(ServiceLifetime.Singleton)
    .RegisteredAfter(DateTimeOffset.Now.AddDays(-30)));
```

### Registry Import

```csharp
var importData = await File.ReadAllTextAsync("registry-backup.json");
var registryData = RegistryData.FromJson(importData);

var importResult = await registry.ImportAsync(registryData, options =>
{
    options.MergeStrategy = RegistryMergeStrategy.ReplaceExisting;
    options.ValidateAfterImport = true;
    options.PreserveIds = false; // Generate new IDs
    options.OnConflict = conflict => 
    {
        logger.LogWarning("Import conflict: {ContractType}", conflict.ContractType.Name);
        return ConflictResolution.ReplaceExisting;
    };
});

logger.LogInformation("Import completed: {ImportedCount} services imported, {ConflictCount} conflicts",
    importResult.ImportedServices.Count, importResult.Conflicts.Count);
```

### Registry Merging

```csharp
// Merge multiple registries
await mainRegistry.MergeAsync(moduleRegistry, RegistryMergeStrategy.FailOnConflict);
await mainRegistry.MergeAsync(pluginRegistry, RegistryMergeStrategy.MergeMetadata);

// Bulk merge with custom strategy
var registries = new[] { coreRegistry, dataRegistry, webRegistry };
await mainRegistry.BulkMergeAsync(registries, strategy => strategy
    .UseStrategy(RegistryMergeStrategy.Custom)
    .WithConflictResolver(conflict =>
    {
        // Custom conflict resolution logic
        if (conflict.ExistingService.Tags.Contains("core"))
            return ConflictResolution.KeptExisting;
        return ConflictResolution.ReplacedWithIncoming;
    }));
```

## Snapshot Management

### Creating Snapshots

```csharp
// Create named snapshot
var snapshot = await registry.CreateSnapshotAsync("pre-deployment-snapshot");
logger.LogInformation("Created snapshot {SnapshotId} with {ServiceCount} services",
    snapshot.Id, snapshot.ServiceCount);

// Create snapshot with metadata
var deploymentSnapshot = await registry.CreateSnapshotAsync(
    name: "deployment-v2.1.0",
    metadata: new Dictionary<string, object>
    {
        ["Version"] = "2.1.0",
        ["Environment"] = "Production",
        ["DeploymentId"] = Guid.NewGuid()
    });
```

### Snapshot Operations

```csharp
// List available snapshots
var snapshots = await registry.GetSnapshotsAsync();
foreach (var snapshot in snapshots.OrderByDescending(s => s.CreatedAt))
{
    logger.LogInformation("Snapshot: {Name} ({Id}) - {ServiceCount} services, created {CreatedAt}",
        snapshot.Name, snapshot.Id, snapshot.ServiceCount, snapshot.CreatedAt);
}

// Restore from snapshot
await registry.RestoreFromSnapshotAsync(snapshot.Id);
logger.LogInformation("Restored registry from snapshot: {SnapshotName}", snapshot.Name);

// Compare snapshots
var comparison = await registry.CompareSnapshotsAsync(oldSnapshotId, newSnapshotId);
logger.LogInformation("Snapshot comparison: {Added} added, {Removed} removed, {Modified} modified",
    comparison.AddedServices.Count, comparison.RemovedServices.Count, comparison.ModifiedServices.Count);
```

## Best Practices

### Performance Optimization

1. **Use Parallel Scanning** for large assemblies
2. **Enable Caching** for frequently accessed services
3. **Configure Sampling** for performance monitoring in production
4. **Implement Event Handlers** asynchronously to avoid blocking

### Memory Management

1. **Monitor Memory Usage** regularly with diagnostics
2. **Use Appropriate Lifetimes** to avoid memory leaks
3. **Clean Up Event Subscriptions** when no longer needed
4. **Limit Metadata Size** for services with high instance counts

### Validation Strategy

1. **Enable Validation** during development and testing
2. **Use Lightweight Validation** in production
3. **Monitor Validation Events** for runtime issues
4. **Implement Custom Validators** for domain-specific rules

### Event Handling

1. **Handle Events Asynchronously** to maintain performance
2. **Implement Error Handling** in event handlers
3. **Use Structured Logging** for event data
4. **Consider Event Aggregation** for high-volume scenarios

## Integration Examples

### ASP.NET Core Integration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Create and configure advanced registry
    var registry = new AdvancedServiceRegistry();
    
    // Scan assemblies
    await registry.ScanAssembliesAsync(new[] { typeof(Startup).Assembly });
    
    // Register with ASP.NET Core DI
    services.AddSingleton<IAdvancedServiceRegistry>(registry);
    
    // Configure validation
    await registry.ValidateAsync(new ServiceValidationOptions
    {
        ValidateConstructorParameters = true,
        ValidateLifetimeCompatibility = true
    });
}
```

### Background Service Integration

```csharp
public class RegistryMonitoringService : BackgroundService
{
    private readonly IAdvancedServiceRegistry _registry;
    private readonly ILogger<RegistryMonitoringService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Subscribe to performance events
        await _registry.SubscribeToPerformanceEventsAsync(async eventArgs =>
        {
            if (eventArgs.ExceededThreshold != null)
            {
                _logger.LogWarning("Performance threshold exceeded: {Threshold}",
                    eventArgs.ExceededThreshold.ThresholdType);
            }
        });

        // Periodic validation
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var validationReport = await _registry.ValidateAsync();
            if (!validationReport.IsValid)
            {
                _logger.LogWarning("Registry validation failed with {ErrorCount} errors",
                    validationReport.Errors.Count);
            }
        }
    }
}
```

## Conclusion

The Advanced Service Registry provides a comprehensive foundation for enterprise-grade dependency injection systems. Its sophisticated feature set enables complex scenarios while maintaining excellent developer experience through fluent APIs, comprehensive diagnostics, and extensive customization options.

Key benefits include:

- **Sophisticated Registration** - Conditional, metadata-rich service registration
- **Powerful Discovery** - Advanced querying and filtering capabilities  
- **Comprehensive Monitoring** - Validation, diagnostics, and performance tracking
- **Enterprise Features** - Import/export, merging, and snapshot management
- **Event-Driven Architecture** - Real-time notifications for all operations
- **High Performance** - Optimized for large-scale applications with caching and parallel operations

The registry serves as the central nervous system of the DI container, providing the intelligence and flexibility needed for modern, complex applications while maintaining simplicity for common scenarios.
