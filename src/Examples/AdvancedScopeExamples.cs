using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zentient.DependencyInjection.Scopes;

namespace Zentient.DependencyInjection.Examples
{
    /// <summary>
    /// Comprehensive examples demonstrating sophisticated DI scope usage patterns
    /// with advanced features including context-aware resolution, hierarchical scopes,
    /// diagnostics, pooling, and lifecycle management.
    /// </summary>
    public static class AdvancedScopeExamples
    {
        // ================================================================================
        // EXAMPLE 1: Basic Scope Creation and Management
        // ================================================================================

        /// <summary>
        /// Demonstrates basic scope creation with metadata and configuration.
        /// </summary>
        public static async Task BasicScopeManagementExample(IAdvancedServiceScopeFactory factory)
        {
            // Create a named scope with metadata for a web request
            using var requestScope = factory.CreateScope("web-request-123", config => config
                .WithMetadata("RequestId", "req-123")
                .WithMetadata("UserId", "user-456")
                .WithMetadata("RequestPath", "/api/orders")
                .WithTimeout(TimeSpan.FromMinutes(5))
                .EnableDiagnostics()
                .EnablePerformanceMetrics());

            // Resolve services within the scope
            var orderService = await requestScope.ResolveAsync<IOrderService>();
            var userService = await requestScope.ResolveAsync<IUserService>();

            // Access scope metadata from services if they're scope-aware
            if (orderService is IScopeAware scopeAwareService)
            {
                var requestId = scopeAwareService.Scope.Metadata["RequestId"];
                Console.WriteLine($"Processing order in request: {requestId}");
            }

            // Validate scope health before critical operations
            var validation = await requestScope.ValidateAsync();
            if (!validation.IsValid)
            {
                Console.WriteLine($"Scope validation failed: {validation.Summary}");
                return;
            }

            // Check performance metrics
            var metrics = requestScope.PerformanceMetrics;
            Console.WriteLine($"Scope memory usage: {metrics.CurrentMemoryUsage:N0} bytes");
            Console.WriteLine($"Services resolved: {requestScope.Diagnostics.ServicesResolved}");

            // Scope automatically disposes all services when disposed
        }

        // ================================================================================
        // EXAMPLE 2: Hierarchical Child Scopes
        // ================================================================================

        /// <summary>
        /// Demonstrates creating and managing hierarchical child scopes for
        /// logical operation isolation within a larger scope.
        /// </summary>
        public static async Task HierarchicalScopeExample(IAdvancedServiceScopeFactory factory)
        {
            // Create a root scope for a batch processing job
            using var jobScope = factory.CreateScope("batch-job-789", config => config
                .WithMetadata("JobId", "job-789")
                .WithMetadata("JobType", "DataImport")
                .WithTimeout(TimeSpan.FromHours(2)));

            // Process multiple batches with child scopes
            for (int batchNum = 1; batchNum <= 5; batchNum++)
            {
                using var batchScope = jobScope.CreateChildScope($"batch-{batchNum}", config => config
                    .WithMetadata("BatchNumber", batchNum)
                    .WithMetadata("StartTime", DateTimeOffset.UtcNow)
                    .WithTimeout(TimeSpan.FromMinutes(30)));

                // Resolve batch-specific services
                var dataProcessor = await batchScope.ResolveAsync<IDataProcessor>();
                var validator = await batchScope.ResolveAsync<IDataValidator>();

                // Process the batch
                Console.WriteLine($"Processing batch {batchNum} in scope {batchScope.ScopeId}");
                
                // Register cleanup for this specific batch
                batchScope.RegisterAsyncDisposalCallback(async () =>
                {
                    Console.WriteLine($"Cleaning up batch {batchNum}");
                    await Task.Delay(100); // Simulate cleanup
                });

                // Child scope automatically disposes when leaving this iteration
            }

            // Check how many child scopes were created
            Console.WriteLine($"Job processed {jobScope.ChildScopes.Count} batches");
        }

        // ================================================================================
        // EXAMPLE 3: Context-Aware Service Resolution
        // ================================================================================

        /// <summary>
        /// Demonstrates context-aware service resolution where services receive
        /// scope-specific context during construction.
        /// </summary>
        public static async Task ContextAwareResolutionExample(IAdvancedServiceScopeFactory factory)
        {
            using var scope = factory.CreateScope("context-demo", config => config
                .WithMetadata("TenantId", "tenant-123")
                .WithMetadata("Environment", "Production")
                .WithMetadata("Region", "US-East"));

            // Create a scope context with additional resolution-specific data
            var scopeContext = new CustomScopeContext(scope)
            {
                ContextData = new Dictionary<string, object>
                {
                    ["OperationType"] = "BulkUpdate",
                    ["Priority"] = "High",
                    ["UserRoles"] = new[] { "Admin", "DataManager" }
                }
            };

            // Resolve a service with context - the service can access both scope metadata
            // and context-specific data during construction
            var contextAwareService = await scope.ResolveWithScopeContextAsync<IContextAwareService>(
                scopeContext,
                descriptor => descriptor.Metadata.ContainsKey("SupportsContext"));

            if (contextAwareService != null)
            {
                Console.WriteLine("Service resolved with full context awareness");
            }

            // Resolve scope-aware services that receive the scope itself
            var scopeAware = await scope.ResolveScopeAwareAsync<IScopeAwareNotificationService>();
            scopeAware?.NotifyWithScopeInfo("Operation completed");
        }

        // ================================================================================
        // EXAMPLE 4: Scope Pooling for Performance
        // ================================================================================

        /// <summary>
        /// Demonstrates scope pooling for high-performance scenarios where
        /// scope creation overhead should be minimized.
        /// </summary>
        public static async Task ScopePoolingExample(IAdvancedServiceScopeFactory factory)
        {
            // Define scope pools for different operation types
            const string readOperationPool = "read-ops";
            const string writeOperationPool = "write-ops";

            // Process multiple read operations using pooled scopes
            var readTasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                readTasks.Add(ProcessReadOperation(factory, readOperationPool, i));
            }

            await Task.WhenAll(readTasks);

            // Process write operations with separate pool
            var writeTasks = new List<Task>();
            for (int i = 0; i < 20; i++)
            {
                writeTasks.Add(ProcessWriteOperation(factory, writeOperationPool, i));
            }

            await Task.WhenAll(writeTasks);

            Console.WriteLine($"Pool diagnostics - Read pool: {factory.Diagnostics.CreationStatistics.GetValueOrDefault(readOperationPool, 0)}");
            Console.WriteLine($"Pool diagnostics - Write pool: {factory.Diagnostics.CreationStatistics.GetValueOrDefault(writeOperationPool, 0)}");
        }

        private static async Task ProcessReadOperation(IAdvancedServiceScopeFactory factory, string poolKey, int operationId)
        {
            // Get a pooled scope for read operations
            var scope = factory.GetOrCreatePooledScope(poolKey, () =>
                factory.CreateScope($"read-{Guid.NewGuid()}", config => config
                    .WithMetadata("PoolType", "ReadOperations")
                    .EnablePerformanceMetrics()));

            try
            {
                // Configure scope for this specific operation
                scope.Metadata["OperationId"] = operationId;
                scope.Metadata["StartTime"] = DateTimeOffset.UtcNow;

                var dataService = await scope.ResolveAsync<IDataReadService>();
                await dataService.ReadDataAsync($"data-{operationId}");
                
                Console.WriteLine($"Read operation {operationId} completed in scope {scope.ScopeId}");
            }
            finally
            {
                // Return scope to pool for reuse
                await factory.ReturnScopeToPoolAsync(scope, poolKey);
            }
        }

        private static async Task ProcessWriteOperation(IAdvancedServiceScopeFactory factory, string poolKey, int operationId)
        {
            var scope = factory.GetOrCreatePooledScope(poolKey, () =>
                factory.CreateScope($"write-{Guid.NewGuid()}", config => config
                    .WithMetadata("PoolType", "WriteOperations")
                    .EnableDiagnostics()
                    .WithTimeout(TimeSpan.FromMinutes(10))));

            try
            {
                scope.Metadata["OperationId"] = operationId;
                var dataService = await scope.ResolveAsync<IDataWriteService>();
                await dataService.WriteDataAsync($"data-{operationId}");
                
                Console.WriteLine($"Write operation {operationId} completed");
            }
            finally
            {
                await factory.ReturnScopeToPoolAsync(scope, poolKey);
            }
        }

        // ================================================================================
        // EXAMPLE 5: Scope Templates for Consistency
        // ================================================================================

        /// <summary>
        /// Demonstrates using scope templates to ensure consistent configuration
        /// across different parts of the application.
        /// </summary>
        public static async Task ScopeTemplateExample(IAdvancedServiceScopeFactory factory)
        {
            // Register application-specific scope templates
            RegisterScopeTemplates(factory);

            // Create scopes from templates
            using var webApiScope = factory.CreateScopeFromTemplate(
                factory.GetTemplate("WebApiRequest")!, 
                "api-request-456");

            using var backgroundJobScope = factory.CreateScopeFromTemplate(
                factory.GetTemplate("BackgroundJob")!,
                "job-cleanup-789");

            using var integrationTestScope = factory.CreateScopeFromTemplate(
                factory.GetTemplate("IntegrationTest")!,
                "test-scenario-123");

            // Each scope has consistent configuration based on its template
            Console.WriteLine($"Web API scope timeout: {webApiScope.Metadata.GetValueOrDefault("DefaultTimeout")}");
            Console.WriteLine($"Background job diagnostics enabled: {backgroundJobScope.Diagnostics != null}");
            Console.WriteLine($"Test scope max services: {integrationTestScope.Metadata.GetValueOrDefault("MaxServices")}");

            // Use the scopes according to their intended purpose
            await SimulateWebApiRequest(webApiScope);
            await SimulateBackgroundJob(backgroundJobScope);
            await SimulateIntegrationTest(integrationTestScope);
        }

        private static void RegisterScopeTemplates(IAdvancedServiceScopeFactory factory)
        {
            // Web API request template - optimized for fast request processing
            factory.RegisterTemplate("WebApiRequest", new ScopeTemplate
            {
                Name = "WebApiRequest",
                Description = "Optimized scope for web API request processing",
                DefaultTimeout = TimeSpan.FromMinutes(2),
                EnableDiagnostics = false, // Disabled for performance
                DefaultMetadata = new Dictionary<string, object>
                {
                    ["ScopeType"] = "WebApi",
                    ["DefaultTimeout"] = TimeSpan.FromMinutes(2),
                    ["OptimizedForPerformance"] = true
                }
            });

            // Background job template - optimized for long-running operations
            factory.RegisterTemplate("BackgroundJob", new ScopeTemplate
            {
                Name = "BackgroundJob",
                Description = "Scope for background processing with extensive monitoring",
                DefaultTimeout = TimeSpan.FromHours(4),
                EnableDiagnostics = true, // Enabled for monitoring
                DefaultMetadata = new Dictionary<string, object>
                {
                    ["ScopeType"] = "BackgroundJob",
                    ["EnableDetailedLogging"] = true,
                    ["AllowLongRunning"] = true
                }
            });

            // Integration test template - configured for test scenarios
            factory.RegisterTemplate("IntegrationTest", new ScopeTemplate
            {
                Name = "IntegrationTest",
                Description = "Scope for integration testing with validation",
                DefaultTimeout = TimeSpan.FromMinutes(30),
                EnableDiagnostics = true,
                DefaultMetadata = new Dictionary<string, object>
                {
                    ["ScopeType"] = "IntegrationTest",
                    ["MaxServices"] = 1000,
                    ["ValidateOnDispose"] = true
                }
            });
        }

        // ================================================================================
        // EXAMPLE 6: Advanced Scope Monitoring and Diagnostics
        // ================================================================================

        /// <summary>
        /// Demonstrates comprehensive scope monitoring, diagnostics, and health checking.
        /// </summary>
        public static async Task ScopeMonitoringExample(IAdvancedServiceScopeFactory factory)
        {
            // Create a scope with full monitoring enabled
            using var scope = factory.CreateScope("monitoring-demo", config => config
                .WithMetadata("Application", "OrderProcessing")
                .WithMetadata("Version", "2.1.0")
                .EnableDiagnostics()
                .EnablePerformanceMetrics()
                .WithMaxServices(100));

            // Subscribe to scope state changes for monitoring
            scope.StateChanged += (sender, args) =>
            {
                Console.WriteLine($"Scope {args.Scope.ScopeId} changed from {args.PreviousState} to {args.NewState} at {args.Timestamp}");
            };

            // Resolve multiple services to generate diagnostic data
            var services = new object[]
            {
                await scope.ResolveAsync<IOrderService>(),
                await scope.ResolveAsync<IUserService>(),
                await scope.ResolveAsync<IInventoryService>(),
                await scope.ResolveAsync<INotificationService>()
            };

            // Analyze scope diagnostics
            var diagnostics = scope.Diagnostics;
            Console.WriteLine($"\n=== Scope Diagnostics ===");
            Console.WriteLine($"Scope ID: {diagnostics.ScopeId}");
            Console.WriteLine($"Services Resolved: {diagnostics.ServicesResolved}");
            Console.WriteLine($"Active Services: {diagnostics.ActiveServices}");
            Console.WriteLine($"Disposed Services: {diagnostics.DisposedServices}");

            // Analyze performance metrics
            var metrics = scope.PerformanceMetrics;
            Console.WriteLine($"\n=== Performance Metrics ===");
            Console.WriteLine($"Total Resolution Time: {metrics.TotalResolutionTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"Average Resolution Time: {metrics.AverageResolutionTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"Current Memory Usage: {metrics.CurrentMemoryUsage:N0} bytes");
            Console.WriteLine($"Peak Memory Usage: {metrics.PeakMemoryUsage:N0} bytes");

            // Analyze resource usage
            var resources = scope.ResourceInfo;
            Console.WriteLine($"\n=== Resource Info ===");
            Console.WriteLine($"Memory Usage: {resources.MemoryUsage:N0} bytes");
            Console.WriteLine($"Active Service Count: {resources.ActiveServiceCount}");
            Console.WriteLine($"Child Scope Count: {resources.ChildScopeCount}");
            Console.WriteLine($"Scope Depth: {resources.ScopeDepth}");

            // Check resolution history
            Console.WriteLine($"\n=== Resolution History ===");
            foreach (var record in diagnostics.ResolutionHistory.Take(5))
            {
                Console.WriteLine($"  {record.ServiceType.Name}: {record.ResolutionTime.TotalMilliseconds:F2}ms " +
                                $"(Success: {record.Success}) at {record.ResolvedAt:HH:mm:ss.fff}");
            }

            // Analyze dependency graph for circular dependencies
            var dependencyGraph = diagnostics.DependencyGraph;
            var circularDependencies = dependencyGraph.FindCircularDependencies();
            if (circularDependencies.Any())
            {
                Console.WriteLine($"\n=== Warning: Circular Dependencies Found ===");
                foreach (var circular in circularDependencies)
                {
                    Console.WriteLine($"  {circular.Description}");
                }
            }

            // Perform comprehensive scope validation
            var validation = await scope.ValidateAsync();
            Console.WriteLine($"\n=== Scope Validation ===");
            Console.WriteLine($"Is Valid: {validation.IsValid}");
            Console.WriteLine($"Summary: {validation.Summary}");

            if (validation.Errors.Any())
            {
                Console.WriteLine("Errors:");
                foreach (var error in validation.Errors)
                {
                    Console.WriteLine($"  {error.Severity}: {error.Message}");
                }
            }

            if (validation.Warnings.Any())
            {
                Console.WriteLine("Warnings:");
                foreach (var warning in validation.Warnings)
                {
                    Console.WriteLine($"  {warning.Severity}: {warning.Message}");
                }
            }
        }

        // ================================================================================
        // EXAMPLE 7: Scope Lifecycle and Cleanup Management
        // ================================================================================

        /// <summary>
        /// Demonstrates advanced scope lifecycle management including custom disposal
        /// callbacks, timeout handling, and graceful cleanup.
        /// </summary>
        public static async Task ScopeLifecycleExample(IAdvancedServiceScopeFactory factory)
        {
            using var scope = factory.CreateScope("lifecycle-demo", config => config
                .WithMetadata("Purpose", "LifecycleDemo")
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithDisposalCallback(scope => Console.WriteLine($"Synchronous cleanup for scope {scope.ScopeId}"))
                .WithAsyncDisposalCallback(async scope =>
                {
                    Console.WriteLine($"Starting async cleanup for scope {scope.ScopeId}");
                    await Task.Delay(100); // Simulate async cleanup
                    Console.WriteLine($"Async cleanup completed for scope {scope.ScopeId}");
                }));

            // Set a timeout with custom callback
            scope.SetTimeout(TimeSpan.FromSeconds(45), timedOutScope =>
            {
                Console.WriteLine($"Scope {timedOutScope.ScopeId} timed out and will be disposed");
            });

            // Register additional disposal callbacks
            scope.RegisterDisposalCallback(() =>
            {
                Console.WriteLine("Custom disposal callback executed");
            });

            scope.RegisterAsyncDisposalCallback(async () =>
            {
                Console.WriteLine("Starting custom async disposal callback");
                await Task.Delay(50);
                Console.WriteLine("Custom async disposal callback completed");
            });

            // Resolve some services
            var service1 = await scope.ResolveAsync<IOrderService>();
            var service2 = await scope.ResolveAsync<IUserService>();

            // Simulate work
            await Task.Delay(100);

            // Manually dispose a specific service early if needed
            if (service1 != null)
            {
                await scope.DisposeServiceAsync(service1);
                Console.WriteLine("Manually disposed service1 early");
            }

            // Clear the timeout to prevent automatic disposal
            scope.ClearTimeout();

            Console.WriteLine("Scope will now dispose with all registered callbacks");
            // Disposal happens automatically when leaving using block
        }

        // ================================================================================
        // SUPPORTING CLASSES AND INTERFACES
        // ================================================================================

        // Sample custom scope context implementation
        private class CustomScopeContext : IScopeContext
        {
            public IAdvancedServiceScope Scope { get; }
            public IReadOnlyDictionary<string, object> ScopeMetadata => Scope.Metadata;
            public IReadOnlyDictionary<string, object> ContextData { get; set; } = new Dictionary<string, object>();

            private readonly Dictionary<string, object> _values = new();

            public CustomScopeContext(IAdvancedServiceScope scope)
            {
                Scope = scope;
            }

            public T? GetValue<T>(string key) =>
                _values.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default;

            public void SetValue<T>(string key, T value) =>
                _values[key] = value!;
        }

        // Sample scope template implementation
        private class ScopeTemplate : IScopeTemplate
        {
            public string Name { get; init; } = string.Empty;
            public string? Description { get; init; }
            public IReadOnlyDictionary<string, object> DefaultMetadata { get; init; } = new Dictionary<string, object>();
            public TimeSpan? DefaultTimeout { get; init; }
            public bool EnableDiagnostics { get; init; }

            public void ApplyTo(IScopeConfiguration configuration)
            {
                if (DefaultTimeout.HasValue)
                    configuration.WithTimeout(DefaultTimeout.Value);

                configuration.EnableDiagnostics(EnableDiagnostics);
                configuration.WithMetadata(DefaultMetadata);
            }
        }

        // Helper methods for examples
        private static async Task SimulateWebApiRequest(IAdvancedServiceScope scope)
        {
            var controller = await scope.ResolveAsync<IApiController>();
            // Simulate API request processing
            await Task.Delay(50);
            Console.WriteLine("Web API request processed");
        }

        private static async Task SimulateBackgroundJob(IAdvancedServiceScope scope)
        {
            var jobProcessor = await scope.ResolveAsync<IJobProcessor>();
            // Simulate background job processing
            await Task.Delay(200);
            Console.WriteLine("Background job processed");
        }

        private static async Task SimulateIntegrationTest(IAdvancedServiceScope scope)
        {
            var testRunner = await scope.ResolveAsync<ITestRunner>();
            // Simulate test execution
            await Task.Delay(100);
            Console.WriteLine("Integration test executed");
        }
    }

    // ================================================================================
    // SAMPLE SERVICE INTERFACES FOR EXAMPLES
    // ================================================================================

    public interface IOrderService { }
    public interface IUserService { }
    public interface IInventoryService { }
    public interface INotificationService { }
    public interface IDataProcessor { }
    public interface IDataValidator { }
    public interface IDataReadService { Task ReadDataAsync(string dataId); }
    public interface IDataWriteService { Task WriteDataAsync(string dataId); }
    public interface IContextAwareService { }
    public interface IScopeAwareNotificationService : IScopeAware { void NotifyWithScopeInfo(string message); }
    public interface IApiController { }
    public interface IJobProcessor { }
    public interface ITestRunner { }
}
