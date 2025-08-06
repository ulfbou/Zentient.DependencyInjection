// <copyright file="ServiceRegistryExamples.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zentient.Abstractions.DependencyInjection.Registry;
using Zentient.Abstractions.DependencyInjection.Registry.Events;

namespace Zentient.DependencyInjection.Examples
{
    /// <summary>
    /// Comprehensive examples demonstrating advanced service registry capabilities.
    /// </summary>
    public class ServiceRegistryExamples
    {
        // ================================================================================
        // BASIC REGISTRY OPERATIONS
        // ================================================================================

        /// <summary>
        /// Demonstrates basic service registration and querying.
        /// </summary>
        public async Task BasicRegistryOperationsExample(IAdvancedServiceRegistry registry)
        {
            // Register services with metadata and tags
            var registrationBuilder = registry.CreateRegistrationBuilder<IDataService>()
                .ImplementedBy<DatabaseService>()
                .WithLifetime(ServiceLifetime.Scoped)
                .WithMetadata("ConnectionString", "Server=localhost;Database=MyApp")
                .WithTags("data", "primary", "production")
                .When(ctx => ctx.IsEnvironment("Production"));

            var registration = await registrationBuilder.RegisterAsync();

            // Query services by various criteria
            var dataServices = await registry.FindServicesAsync(criteria => criteria
                .ForContract<IDataService>()
                .WithTag("data")
                .OfLifetime(ServiceLifetime.Scoped));

            var productionServices = await registry.FindServicesAsync(criteria => criteria
                .WithTag("production")
                .RegisteredAfter(DateTimeOffset.Now.AddDays(-7)));

            // Check if a service is registered
            var isRegistered = await registry.IsRegisteredAsync<IDataService>();
            var hasTag = await registry.HasServiceWithTagAsync("critical");

            Console.WriteLine($"Found {dataServices.Count} data services");
            Console.WriteLine($"Found {productionServices.Count} production services");
            Console.WriteLine($"Is IDataService registered: {isRegistered}");
        }

        /// <summary>
        /// Demonstrates conditional service registration based on environment and configuration.
        /// </summary>
        public async Task ConditionalRegistrationExample(IAdvancedServiceRegistry registry)
        {
            // Development-only services
            await registry.CreateRegistrationBuilder<ILogService>()
                .ImplementedBy<ConsoleLogService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("logging", "development")
                .When(ctx => ctx.IsEnvironment("Development"))
                .RegisterAsync();

            // Production-only services with configuration conditions
            await registry.CreateRegistrationBuilder<ILogService>()
                .ImplementedBy<SerilogService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("logging", "production")
                .When(ctx => ctx.IsEnvironment("Production") && 
                            ctx.GetConfigurationValue<bool>("Logging:UseStructuredLogging", false))
                .RegisterAsync();

            // Multi-condition registration
            await registry.CreateRegistrationBuilder<ICacheService>()
                .ImplementedBy<RedisCacheService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithMetadata("CacheType", "Distributed")
                .When(ctx => ctx.IsEnvironment("Production") || ctx.IsEnvironment("Staging"))
                .When(ctx => ctx.GetConfigurationValue<string>("Cache:Provider") == "Redis")
                .RegisterAsync();
        }

        // ================================================================================
        // ASSEMBLY SCANNING EXAMPLES
        // ================================================================================

        /// <summary>
        /// Demonstrates advanced assembly scanning with conventions.
        /// </summary>
        public async Task AssemblyScanningExample(IAdvancedServiceRegistry registry)
        {
            // Scan current assembly with conventions
            await registry.ScanAssembliesAsync(
                assemblies: new[] { typeof(ServiceRegistryExamples).Assembly },
                configure: scanner => scanner
                    .IncludeTypes(type => !type.IsAbstract && !type.IsInterface)
                    .ExcludeTypes(type => type.Name.EndsWith("Test"))
                    .WithDefaultLifetime(ServiceLifetime.Scoped)
                    .WithConventions(conventions =>
                    {
                        // Register services ending with "Service" as their first interface
                        conventions.ForTypesMatching("*Service")
                            .RegisterAs(type => type.GetInterfaces().FirstOrDefault() ?? type)
                            .UseLifetime(ServiceLifetime.Scoped)
                            .WithTags("service", "auto-registered");

                        // Register repositories with specific conventions
                        conventions.ForTypesImplementing<IRepository>()
                            .RegisterAs(type => typeof(IRepository<>).MakeGenericType(
                                type.GetGenericArguments().FirstOrDefault() ?? typeof(object)))
                            .UseLifetime(ServiceLifetime.Scoped)
                            .WithTags("repository", "data-access")
                            .WithMetadata("DataAccess", true);

                        // Register handlers with naming conventions
                        conventions.ForTypesImplementing<IHandler>()
                            .RegisterAs(type => type.GetInterfaces()
                                .FirstOrDefault(i => i.Name.Contains("Handler")) ?? type)
                            .UseLifetime(ServiceLifetime.Transient)
                            .WithTags("handler", "command")
                            .When(ctx => ctx.GetConfigurationValue<bool>("Features:EnableHandlers", true));
                    })
                    .EnableParallelScanning()
                    .WithProgress(progress => 
                        Console.WriteLine($"Scanning: {progress.ProcessedTypes}/{progress.TotalTypes} " +
                                        $"({progress.PercentComplete:P0})"))
            );
        }

        /// <summary>
        /// Demonstrates scanning multiple assemblies with different strategies.
        /// </summary>
        public async Task MultipleAssemblyScanningExample(IAdvancedServiceRegistry registry)
        {
            var coreAssembly = typeof(ICoreService).Assembly;
            var dataAssembly = typeof(IDataService).Assembly;
            var webAssembly = typeof(IWebService).Assembly;

            await registry.ScanAssembliesAsync(
                assemblies: new[] { coreAssembly, dataAssembly, webAssembly },
                configure: scanner => scanner
                    .IncludeTypes(type => type.IsPublic && !type.IsAbstract)
                    .WithAttribute<ServiceAttribute>()
                    .WithConventions(conventions =>
                    {
                        // Core services - singleton by default
                        conventions.ForTypesInNamespace("MyApp.Core")
                            .UseLifetime(ServiceLifetime.Singleton)
                            .WithTags("core", "infrastructure");

                        // Data services - scoped by default
                        conventions.ForTypesInNamespace("MyApp.Data")
                            .UseLifetime(ServiceLifetime.Scoped)
                            .WithTags("data", "persistence");

                        // Web services - transient by default
                        conventions.ForTypesInNamespace("MyApp.Web")
                            .UseLifetime(ServiceLifetime.Transient)
                            .WithTags("web", "api");
                    })
            );
        }

        // ================================================================================
        // VALIDATION AND DIAGNOSTICS EXAMPLES
        // ================================================================================

        /// <summary>
        /// Demonstrates comprehensive registry validation.
        /// </summary>
        public async Task ValidationExample(IAdvancedServiceRegistry registry)
        {
            // Configure validation options
            var validationOptions = new ServiceValidationOptions
            {
                ValidateConstructorParameters = true,
                ValidateInterfaceImplementation = true,
                ValidateLifetimeCompatibility = true,
                ValidateDependencyAvailability = true,
                OnValidationFailure = issue => Console.WriteLine($"Validation Issue: {issue.Message}")
            };

            // Validate the entire registry
            var validationReport = await registry.ValidateAsync(validationOptions);

            Console.WriteLine($"Registry Validation Results:");
            Console.WriteLine($"- Valid: {validationReport.IsValid}");
            Console.WriteLine($"- Errors: {validationReport.Errors.Count}");
            Console.WriteLine($"- Warnings: {validationReport.Warnings.Count}");
            Console.WriteLine($"- Services Validated: {validationReport.ServicesValidated}");

            // Display detailed validation issues
            foreach (var error in validationReport.Errors)
            {
                Console.WriteLine($"ERROR [{error.Category}]: {error.Message}");
                if (error.ServiceType != null)
                    Console.WriteLine($"  Service: {error.ServiceType.Name}");
            }

            foreach (var warning in validationReport.Warnings)
            {
                Console.WriteLine($"WARNING [{warning.Category}]: {warning.Message}");
            }

            // Validate specific services
            var dataServiceReport = await registry.ValidateServiceAsync<IDataService>(validationOptions);
            if (!dataServiceReport.IsValid)
            {
                Console.WriteLine($"IDataService validation failed:");
                foreach (var issue in dataServiceReport.Issues)
                {
                    Console.WriteLine($"  - {issue.Severity}: {issue.Message}");
                }
            }
        }

        /// <summary>
        /// Demonstrates dependency analysis and circular dependency detection.
        /// </summary>
        public async Task DependencyAnalysisExample(IAdvancedServiceRegistry registry)
        {
            // Perform comprehensive dependency analysis
            var analysisReport = await registry.AnalyzeDependenciesAsync();

            Console.WriteLine($"Dependency Analysis Results:");
            Console.WriteLine($"- Total Services: {analysisReport.Statistics.TotalServices}");
            Console.WriteLine($"- Total Dependencies: {analysisReport.Statistics.TotalDependencies}");
            Console.WriteLine($"- Max Dependency Depth: {analysisReport.Statistics.MaxDependencyDepth}");
            Console.WriteLine($"- Average Dependencies per Service: {analysisReport.Statistics.AverageDependenciesPerService:F2}");

            // Check for circular dependencies
            if (analysisReport.CircularDependencies.Any())
            {
                Console.WriteLine($"\nCircular Dependencies Found ({analysisReport.CircularDependencies.Count}):");
                foreach (var circular in analysisReport.CircularDependencies)
                {
                    Console.WriteLine($"  - {circular.Severity}: {circular.Description}");
                    Console.WriteLine($"    Services: {string.Join(" -> ", circular.Services.Select(s => s.ServiceType.Name))}");
                }
            }

            // Identify orphaned services
            if (analysisReport.OrphanedServices.Any())
            {
                Console.WriteLine($"\nOrphaned Services ({analysisReport.OrphanedServices.Count}):");
                foreach (var orphaned in analysisReport.OrphanedServices)
                {
                    Console.WriteLine($"  - {orphaned.ServiceType.Name} (No dependents)");
                }
            }

            // Show root services
            Console.WriteLine($"\nRoot Services ({analysisReport.RootServices.Count}):");
            foreach (var root in analysisReport.RootServices)
            {
                Console.WriteLine($"  - {root.ServiceType.Name} (No dependencies)");
            }
        }

        /// <summary>
        /// Demonstrates registry diagnostics and performance monitoring.
        /// </summary>
        public async Task DiagnosticsExample(IAdvancedServiceRegistry registry)
        {
            // Get comprehensive diagnostics
            var diagnostics = await registry.GetDiagnosticsAsync();

            Console.WriteLine($"Registry Diagnostics:");
            Console.WriteLine($"- Registry ID: {diagnostics.RegistryId}");
            Console.WriteLine($"- Total Services: {diagnostics.TotalServices}");

            // Services by lifetime
            Console.WriteLine($"\nServices by Lifetime:");
            foreach (var (lifetime, count) in diagnostics.ServicesByLifetime)
            {
                Console.WriteLine($"  - {lifetime}: {count}");
            }

            // Services by source
            Console.WriteLine($"\nServices by Source:");
            foreach (var (source, count) in diagnostics.ServicesBySource)
            {
                Console.WriteLine($"  - {source}: {count}");
            }

            // Tag usage statistics
            Console.WriteLine($"\nMost Used Tags:");
            foreach (var (tag, count) in diagnostics.TagUsageStatistics.Take(10))
            {
                Console.WriteLine($"  - {tag}: {count}");
            }

            // Memory information
            var memoryInfo = diagnostics.MemoryInfo;
            Console.WriteLine($"\nMemory Usage:");
            Console.WriteLine($"  - Registry Memory: {memoryInfo.RegistryMemoryUsage:N0} bytes");
            Console.WriteLine($"  - Service Descriptors: {memoryInfo.ServiceDescriptorMemoryUsage:N0} bytes");
            Console.WriteLine($"  - Total Estimated: {memoryInfo.TotalEstimatedMemoryUsage:N0} bytes");

            // Performance metrics
            var performance = await registry.GetPerformanceMetricsAsync();
            Console.WriteLine($"\nPerformance Metrics:");
            Console.WriteLine($"  - Total Registration Time: {performance.TotalRegistrationTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"  - Average Registration Time: {performance.AverageRegistrationTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"  - Total Lookup Time: {performance.TotalLookupTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"  - Average Lookup Time: {performance.AverageLookupTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"  - Cache Hit Rate: {performance.CacheHitRate:P2}");
        }

        // ================================================================================
        // EVENT HANDLING EXAMPLES
        // ================================================================================

        /// <summary>
        /// Demonstrates comprehensive event handling for registry operations.
        /// </summary>
        public async Task EventHandlingExample(IAdvancedServiceRegistry registry)
        {
            // Subscribe to registration events
            await registry.SubscribeToRegistrationEventsAsync(async eventArgs =>
            {
                var service = eventArgs.ServiceDescriptor;
                Console.WriteLine($"Service Registered: {service.ServiceType.Name}");
                Console.WriteLine($"  - ID: {service.Id}");
                Console.WriteLine($"  - Lifetime: {service.Lifetime}");
                Console.WriteLine($"  - Tags: {string.Join(", ", service.Tags)}");
                Console.WriteLine($"  - Is Update: {eventArgs.IsUpdate}");

                if (eventArgs.IsUpdate && eventArgs.PreviousDescriptor != null)
                {
                    Console.WriteLine($"  - Previous Lifetime: {eventArgs.PreviousDescriptor.Lifetime}");
                }
            });

            // Subscribe to validation events
            await registry.SubscribeToValidationEventsAsync(async eventArgs =>
            {
                var report = eventArgs.ValidationReport;
                if (!report.IsValid)
                {
                    Console.WriteLine($"Service Validation Failed: {eventArgs.ServiceDescriptor.ServiceType.Name}");
                    foreach (var issue in report.Issues)
                    {
                        Console.WriteLine($"  - {issue.Severity}: {issue.Message}");
                    }
                }
            });

            // Subscribe to registry validation events
            await registry.SubscribeToRegistryValidationEventsAsync(async eventArgs =>
            {
                var report = eventArgs.ValidationReport;
                Console.WriteLine($"Registry Validation Complete:");
                Console.WriteLine($"  - Valid: {report.IsValid}");
                Console.WriteLine($"  - Services: {eventArgs.ServicesValidated}");
                Console.WriteLine($"  - Duration: {eventArgs.ValidationDuration.TotalMilliseconds:F2}ms");
                Console.WriteLine($"  - Errors: {report.Errors.Count}");
                Console.WriteLine($"  - Warnings: {report.Warnings.Count}");
            });

            // Subscribe to performance events
            await registry.SubscribeToPerformanceEventsAsync(async eventArgs =>
            {
                var metrics = eventArgs.PerformanceMetrics;
                Console.WriteLine($"Performance Metrics Updated:");
                Console.WriteLine($"  - Cache Hit Rate: {metrics.CacheHitRate:P2}");
                Console.WriteLine($"  - Average Lookup Time: {metrics.AverageLookupTime.TotalMilliseconds:F2}ms");

                if (eventArgs.ExceededThreshold != null)
                {
                    var threshold = eventArgs.ExceededThreshold;
                    Console.WriteLine($"  - THRESHOLD EXCEEDED: {threshold.ThresholdType}");
                    Console.WriteLine($"    Expected: {threshold.ThresholdValue} {threshold.Unit}");
                    Console.WriteLine($"    Actual: {threshold.CurrentValue} {threshold.Unit}");
                }
            });

            // Subscribe to error events
            await registry.SubscribeToErrorEventsAsync(async eventArgs =>
            {
                Console.WriteLine($"Registry Error: {eventArgs.ErrorCategory}");
                Console.WriteLine($"  - Operation: {eventArgs.Operation}");
                Console.WriteLine($"  - Error: {eventArgs.Error.Message}");
                Console.WriteLine($"  - Recoverable: {eventArgs.IsRecoverable}");
                
                if (eventArgs.ServiceDescriptor != null)
                {
                    Console.WriteLine($"  - Service: {eventArgs.ServiceDescriptor.ServiceType.Name}");
                }
            });
        }

        // ================================================================================
        // REGISTRY EXPORT/IMPORT EXAMPLES
        // ================================================================================

        /// <summary>
        /// Demonstrates registry export and import operations.
        /// </summary>
        public async Task ExportImportExample(IAdvancedServiceRegistry registry)
        {
            // Export entire registry
            var exportData = await registry.ExportAsync();
            Console.WriteLine($"Exported {exportData.Services.Count} services");

            // Export with filtering
            var filteredExport = await registry.ExportAsync(criteria => criteria
                .WithTag("production")
                .OfLifetime(ServiceLifetime.Singleton));
            Console.WriteLine($"Exported {filteredExport.Services.Count} production singleton services");

            // Create a new registry and import
            var newRegistry = CreateNewRegistry();
            
            var importResult = await newRegistry.ImportAsync(exportData, options =>
            {
                options.MergeStrategy = RegistryMergeStrategy.ReplaceExisting;
                options.ValidateAfterImport = true;
                options.PreserveIds = true;
            });

            Console.WriteLine($"Import Results:");
            Console.WriteLine($"  - Services Imported: {importResult.ImportedServices.Count}");
            Console.WriteLine($"  - Conflicts: {importResult.Conflicts.Count}");
            Console.WriteLine($"  - Validation Success: {importResult.ValidationReport?.IsValid ?? false}");

            // Create and restore from snapshot
            var snapshot = await registry.CreateSnapshotAsync("production-snapshot");
            Console.WriteLine($"Created snapshot: {snapshot.Id} with {snapshot.ServiceCount} services");

            // Restore from snapshot
            await registry.RestoreFromSnapshotAsync(snapshot.Id);
            Console.WriteLine($"Restored from snapshot: {snapshot.Name}");
        }

        /// <summary>
        /// Demonstrates registry merging operations.
        /// </summary>
        public async Task RegistryMergingExample(IAdvancedServiceRegistry mainRegistry)
        {
            // Create additional registries for different modules
            var coreRegistry = CreateNewRegistry();
            var dataRegistry = CreateNewRegistry();
            var webRegistry = CreateNewRegistry();

            // Register services in each registry
            await RegisterCoreServices(coreRegistry);
            await RegisterDataServices(dataRegistry);
            await RegisterWebServices(webRegistry);

            // Merge registries with different strategies
            await mainRegistry.MergeAsync(coreRegistry, RegistryMergeStrategy.FailOnConflict);
            await mainRegistry.MergeAsync(dataRegistry, RegistryMergeStrategy.ReplaceExisting);
            await mainRegistry.MergeAsync(webRegistry, RegistryMergeStrategy.MergeMetadata);

            // Get merge statistics
            var statistics = await mainRegistry.GetStatisticsAsync();
            Console.WriteLine($"Merged Registry Statistics:");
            Console.WriteLine($"  - Total Services: {statistics.TotalServices}");
            foreach (var (lifetime, count) in statistics.ServicesByLifetime)
            {
                Console.WriteLine($"  - {lifetime}: {count}");
            }
        }

        // ================================================================================
        // ADVANCED QUERYING EXAMPLES
        // ================================================================================

        /// <summary>
        /// Demonstrates advanced service querying capabilities.
        /// </summary>
        public async Task AdvancedQueryingExample(IAdvancedServiceRegistry registry)
        {
            // Complex queries with multiple criteria
            var complexQuery = await registry.FindServicesAsync(criteria => criteria
                .WithTag("api")
                .OfLifetime(ServiceLifetime.Scoped)
                .WithMetadata("Version", "2.0")
                .RegisteredAfter(DateTimeOffset.Now.AddDays(-30))
                .ImplementedBy(type => type.Namespace?.StartsWith("MyApp.Api") == true));

            // Query by implementation patterns
            var repositoryServices = await registry.FindServicesAsync(criteria => criteria
                .ImplementedBy(type => type.Name.EndsWith("Repository"))
                .WithMetadataKey("EntityType"));

            // Query services with specific metadata values
            var databaseServices = await registry.FindServicesAsync(criteria => criteria
                .WithMetadata("DataSource", "Database")
                .WithMetadata("Provider", "SqlServer"));

            // Get services by multiple tags (AND logic)
            var criticalServices = await registry.FindServicesByTagsAsync(
                tags: new[] { "critical", "production" }, 
                matchAll: true);

            // Get services by any of multiple tags (OR logic)
            var loggingServices = await registry.FindServicesByTagsAsync(
                tags: new[] { "logging", "audit", "trace" }, 
                matchAll: false);

            // Query with custom predicates
            var recentServices = await registry.QueryAsync(service => 
                service.CreatedAt > DateTimeOffset.Now.AddHours(-24) &&
                service.Tags.Contains("auto-registered") &&
                service.Lifetime == ServiceLifetime.Transient);

            Console.WriteLine($"Found {complexQuery.Count} services matching complex criteria");
            Console.WriteLine($"Found {repositoryServices.Count} repository services");
            Console.WriteLine($"Found {databaseServices.Count} database services");
            Console.WriteLine($"Found {criticalServices.Count} critical production services");
            Console.WriteLine($"Found {loggingServices.Count} logging-related services");
            Console.WriteLine($"Found {recentServices.Count} recently auto-registered transient services");
        }

        // ================================================================================
        // HELPER METHODS
        // ================================================================================

        private IAdvancedServiceRegistry CreateNewRegistry()
        {
            // Implementation would create a new registry instance
            throw new NotImplementedException("Registry factory implementation required");
        }

        private async Task RegisterCoreServices(IAdvancedServiceRegistry registry)
        {
            await registry.CreateRegistrationBuilder<ILogService>()
                .ImplementedBy<SerilogService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("core", "logging", "infrastructure")
                .RegisterAsync();

            await registry.CreateRegistrationBuilder<IConfigurationService>()
                .ImplementedBy<ConfigurationService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("core", "configuration", "infrastructure")
                .RegisterAsync();
        }

        private async Task RegisterDataServices(IAdvancedServiceRegistry registry)
        {
            await registry.CreateRegistrationBuilder<IUserRepository>()
                .ImplementedBy<UserRepository>()
                .WithLifetime(ServiceLifetime.Scoped)
                .WithTags("data", "repository", "user")
                .WithMetadata("EntityType", "User")
                .RegisterAsync();
        }

        private async Task RegisterWebServices(IAdvancedServiceRegistry registry)
        {
            await registry.CreateRegistrationBuilder<IUserController>()
                .ImplementedBy<UserController>()
                .WithLifetime(ServiceLifetime.Transient)
                .WithTags("web", "controller", "api")
                .WithMetadata("Version", "2.0")
                .RegisterAsync();
        }
    }

    // ================================================================================
    // EXAMPLE SERVICE INTERFACES AND CLASSES
    // ================================================================================

    // Core service interfaces
    public interface ICoreService { }
    public interface IDataService { }
    public interface IWebService { }
    public interface ILogService { }
    public interface IConfigurationService { }
    public interface ICacheService { }

    // Data interfaces
    public interface IRepository { }
    public interface IRepository<T> : IRepository { }
    public interface IUserRepository : IRepository<User> { }

    // Handler interfaces
    public interface IHandler { }

    // Web interfaces
    public interface IUserController { }

    // Example implementations
    public class DatabaseService : IDataService { }
    public class ConsoleLogService : ILogService { }
    public class SerilogService : ILogService { }
    public class RedisCacheService : ICacheService { }
    public class ConfigurationService : IConfigurationService { }
    public class UserRepository : IUserRepository { }
    public class UserController : IUserController { }

    // Example entities
    public class User { }

    // Example attributes
    public class ServiceAttribute : Attribute { }

    // Example configuration options
    public class ServiceValidationOptions : IServiceValidationOptions
    {
        public bool ValidateConstructorParameters { get; set; } = true;
        public bool ValidateInterfaceImplementation { get; set; } = true;
        public bool ValidateLifetimeCompatibility { get; set; } = true;
        public bool ValidateDependencyAvailability { get; set; } = true;
        public Action<IValidationIssue>? OnValidationFailure { get; set; }
    }
}
