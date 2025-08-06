// <copyright file="ComprehensiveDIIntegration.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zentient.Abstractions.DependencyInjection;
using Zentient.Abstractions.DependencyInjection.Registry;
using Zentient.Abstractions.DependencyInjection.Registry.Events;
using Zentient.DependencyInjection.Registrations;

namespace Zentient.DependencyInjection.Integration
{
    /// <summary>
    /// Comprehensive demonstration of the complete Zentient DI system integration.
    /// Shows how all components work together: Attributes → Container Builder → Service Registry → Resolver → Scopes
    /// </summary>
    public class ComprehensiveDIIntegration
    {
        private readonly ILogger<ComprehensiveDIIntegration> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAdvancedServiceRegistry _registry;
        private readonly IModernContainerBuilder _containerBuilder;

        public ComprehensiveDIIntegration(
            ILogger<ComprehensiveDIIntegration> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _registry = CreateAdvancedRegistry();
            _containerBuilder = CreateModernContainerBuilder();
        }

        /// <summary>
        /// Demonstrates the complete DI system setup and integration.
        /// </summary>
        public async Task<IServiceProvider> SetupCompleteSystemAsync()
        {
            _logger.LogInformation("🚀 Starting Comprehensive DI System Setup");

            // Step 1: Configure Registry with Events
            await ConfigureRegistryWithEventsAsync();

            // Step 2: Register Services Using Attributes
            await RegisterServicesWithAttributesAsync();

            // Step 3: Use Container Builder for Complex Registrations
            await UseContainerBuilderForComplexRegistrationsAsync();

            // Step 4: Perform Assembly Scanning
            await PerformAssemblyScanningAsync();

            // Step 5: Validate Complete Registry
            await ValidateCompleteRegistryAsync();

            // Step 6: Build Service Provider with Advanced Features
            var serviceProvider = await BuildAdvancedServiceProviderAsync();

            // Step 7: Demonstrate Advanced Scope Operations
            await DemonstrateAdvancedScopeOperationsAsync(serviceProvider);

            // Step 8: Show Runtime Service Resolution
            await DemonstrateRuntimeServiceResolutionAsync(serviceProvider);

            _logger.LogInformation("✅ Complete DI System Setup Finished Successfully");
            return serviceProvider;
        }

        // ================================================================================
        // STEP 1: REGISTRY CONFIGURATION WITH EVENTS
        // ================================================================================

        private async Task ConfigureRegistryWithEventsAsync()
        {
            _logger.LogInformation("📋 Configuring Advanced Service Registry with Events");

            // Subscribe to all registry events for comprehensive monitoring
            await _registry.SubscribeToRegistrationEventsAsync(async eventArgs =>
            {
                var service = eventArgs.ServiceDescriptor;
                _logger.LogInformation("🔧 Service Registered: {ServiceType} [{Lifetime}] - Tags: {Tags}",
                    service.ServiceType.Name,
                    service.Lifetime,
                    string.Join(", ", service.Tags));

                // Log conditional registrations
                if (service.Conditions.Any())
                {
                    _logger.LogInformation("   ⚙️ Conditional registration with {ConditionCount} conditions",
                        service.Conditions.Count);
                }

                // Alert for critical services
                if (service.Tags.Contains("critical"))
                {
                    _logger.LogWarning("⚠️ Critical service registered: {ServiceType}", service.ServiceType.Name);
                }
            });

            await _registry.SubscribeToValidationEventsAsync(async eventArgs =>
            {
                if (!eventArgs.ValidationReport.IsValid)
                {
                    _logger.LogError("❌ Service validation failed: {ServiceType} - {IssueCount} issues",
                        eventArgs.ServiceDescriptor.ServiceType.Name,
                        eventArgs.ValidationReport.Issues.Count);
                }
            });

            await _registry.SubscribeToPerformanceEventsAsync(async eventArgs =>
            {
                if (eventArgs.ExceededThreshold != null)
                {
                    var threshold = eventArgs.ExceededThreshold;
                    _logger.LogWarning("⚡ Performance threshold exceeded: {ThresholdType} - Expected: {Expected}, Actual: {Actual}",
                        threshold.ThresholdType, threshold.ThresholdValue, threshold.CurrentValue);
                }
            });

            await _registry.SubscribeToErrorEventsAsync(async eventArgs =>
            {
                _logger.LogError(eventArgs.Error, "💥 Registry error in {Operation}: {Category}",
                    eventArgs.Operation, eventArgs.ErrorCategory);
            });

            _logger.LogInformation("✅ Registry event monitoring configured");
        }

        // ================================================================================
        // STEP 2: ATTRIBUTE-BASED SERVICE REGISTRATION
        // ================================================================================

        private async Task RegisterServicesWithAttributesAsync()
        {
            _logger.LogInformation("🏷️ Registering Services with Attribute System");

            // Register services using the sophisticated attribute system
            await RegisterCoreServicesWithAttributesAsync();
            await RegisterDataServicesWithAttributesAsync();
            await RegisterWebServicesWithAttributesAsync();
            await RegisterInfrastructureServicesWithAttributesAsync();

            _logger.LogInformation("✅ Attribute-based registration completed");
        }

        private async Task RegisterCoreServicesWithAttributesAsync()
        {
            // Logging Service with environment-specific implementations
            await _registry.CreateRegistrationBuilder<IAdvancedLoggingService>()
                .ImplementedBy<ConsoleLoggingService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("core", "logging", "development")
                .WithMetadata("Provider", "Console")
                .When(ctx => ctx.IsEnvironment("Development"))
                .RegisterAsync();

            await _registry.CreateRegistrationBuilder<IAdvancedLoggingService>()
                .ImplementedBy<StructuredLoggingService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("core", "logging", "production", "critical")
                .WithMetadata("Provider", "Structured")
                .When(ctx => ctx.IsEnvironment("Production"))
                .RegisterAsync();

            // Configuration Service
            await _registry.CreateRegistrationBuilder<IConfigurationManager>()
                .ImplementedBy<AdvancedConfigurationManager>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("core", "configuration", "critical")
                .WithMetadata("Source", "Multiple")
                .RegisterAsync();
        }

        private async Task RegisterDataServicesWithAttributesAsync()
        {
            // Database Service with connection string from configuration
            await _registry.CreateRegistrationBuilder<IDatabaseService>()
                .UsingAsyncFactory(async provider =>
                {
                    var config = provider.GetRequiredService<IConfiguration>();
                    var connectionString = config.GetConnectionString("DefaultConnection");
                    var service = new AdvancedDatabaseService(connectionString);
                    await service.InitializeAsync();
                    return service;
                })
                .WithLifetime(ServiceLifetime.Scoped)
                .WithTags("data", "database", "primary")
                .WithMetadata("Provider", "SqlServer")
                .WithMetadata("Features", new[] { "Transactions", "BulkOperations", "Async" })
                .RegisterAsync();

            // Repository pattern with generic constraints
            await _registry.CreateRegistrationBuilder<IUserRepository>()
                .ImplementedBy<AdvancedUserRepository>()
                .WithLifetime(ServiceLifetime.Scoped)
                .WithTags("data", "repository", "user")
                .WithMetadata("EntityType", typeof(User))
                .WithMetadata("Capabilities", new[] { "CRUD", "Search", "Audit" })
                .RegisterAsync();
        }

        private async Task RegisterWebServicesWithAttributesAsync()
        {
            // API Controllers with versioning
            await _registry.CreateRegistrationBuilder<IUserApiController>()
                .ImplementedBy<UserApiControllerV2>()
                .WithLifetime(ServiceLifetime.Transient)
                .WithTags("web", "api", "controller", "v2")
                .WithMetadata("Version", "2.0")
                .WithMetadata("ApiPath", "/api/v2/users")
                .When(ctx => ctx.GetConfigurationValue<string>("Api:Version", "1.0") == "2.0")
                .RegisterAsync();

            // Middleware services
            await _registry.CreateRegistrationBuilder<IAuthenticationMiddleware>()
                .ImplementedBy<JwtAuthenticationMiddleware>()
                .WithLifetime(ServiceLifetime.Scoped)
                .WithTags("web", "middleware", "authentication")
                .WithMetadata("AuthType", "JWT")
                .When(ctx => ctx.GetConfigurationValue<bool>("Authentication:EnableJWT", false))
                .RegisterAsync();
        }

        private async Task RegisterInfrastructureServicesWithAttributesAsync()
        {
            // Cache Service with multiple implementations
            await _registry.CreateRegistrationBuilder<ICacheService>()
                .ImplementedBy<RedisCacheService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("infrastructure", "cache", "distributed", "production")
                .WithMetadata("CacheType", "Distributed")
                .WithMetadata("Provider", "Redis")
                .When(ctx => ctx.IsEnvironment("Production"))
                .When(ctx => ctx.GetConfigurationValue<string>("Cache:Provider") == "Redis")
                .RegisterAsync();

            await _registry.CreateRegistrationBuilder<ICacheService>()
                .ImplementedBy<MemoryCacheService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("infrastructure", "cache", "memory", "development")
                .WithMetadata("CacheType", "InMemory")
                .When(ctx => ctx.IsEnvironment("Development") || 
                            ctx.GetConfigurationValue<string>("Cache:Provider") != "Redis")
                .RegisterAsync();

            // Message Queue Service
            await _registry.CreateRegistrationBuilder<IMessageQueueService>()
                .ImplementedBy<RabbitMQService>()
                .WithLifetime(ServiceLifetime.Singleton)
                .WithTags("infrastructure", "messaging", "queue")
                .WithMetadata("Provider", "RabbitMQ")
                .WithMetadata("Features", new[] { "Reliable", "Persistent", "Clustered" })
                .When(ctx => ctx.GetConfigurationValue<bool>("Messaging:Enabled", false))
                .RegisterAsync();
        }

        // ================================================================================
        // STEP 3: CONTAINER BUILDER FOR COMPLEX REGISTRATIONS
        // ================================================================================

        private async Task UseContainerBuilderForComplexRegistrationsAsync()
        {
            _logger.LogInformation("🏗️ Using Container Builder for Complex Registrations");

            // Use the modern container builder for sophisticated scenarios
            await _containerBuilder
                .ConfigureLogging(logging => logging
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole()
                    .AddStructuredLogging())
                
                .ConfigureValidation(validation => validation
                    .EnableConstructorValidation()
                    .EnableLifetimeValidation()
                    .EnableDependencyValidation()
                    .OnValidationFailure(issue => 
                        _logger.LogWarning("Validation Issue: {Message}", issue.Message)))
                
                .ConfigureDiagnostics(diagnostics => diagnostics
                    .EnablePerformanceMonitoring()
                    .EnableMemoryTracking()
                    .EnableDependencyAnalysis()
                    .CollectMetrics(TimeSpan.FromMinutes(5)))

                .ConfigureResolution(resolution => resolution
                    .EnableAsyncResolution()
                    .EnableContextualResolution()
                    .SetResolutionTimeout(TimeSpan.FromSeconds(30))
                    .OnResolutionFailure(async (context, error) =>
                    {
                        _logger.LogError(error, "Resolution failed for {ServiceType}", context.ServiceType);
                        await HandleResolutionFailureAsync(context, error);
                    }))

                .RegisterDecorator<IUserRepository, CachedUserRepository>(
                    condition: ctx => ctx.GetConfigurationValue<bool>("Features:EnableCaching", false))

                .RegisterInterceptor<ILoggingInterceptor, PerformanceLoggingInterceptor>(
                    selector: type => type.GetCustomAttributes<ProvidesContractAttribute>().Any())

                .RegisterFactory<IEmailService>(async provider =>
                {
                    var config = provider.GetRequiredService<IConfiguration>();
                    var emailProvider = config["Email:Provider"];
                    
                    return emailProvider?.ToLowerInvariant() switch
                    {
                        "sendgrid" => new SendGridEmailService(config["Email:SendGrid:ApiKey"]),
                        "smtp" => new SmtpEmailService(config["Email:Smtp:Host"], 
                                                      config.GetValue<int>("Email:Smtp:Port")),
                        _ => new MockEmailService()
                    };
                })

                .BuildAsync();

            _logger.LogInformation("✅ Container Builder configuration completed");
        }

        // ================================================================================
        // STEP 4: ASSEMBLY SCANNING
        // ================================================================================

        private async Task PerformAssemblyScanningAsync()
        {
            _logger.LogInformation("🔍 Performing Assembly Scanning with Conventions");

            var assemblies = new[]
            {
                typeof(ComprehensiveDIIntegration).Assembly,  // Current assembly
                typeof(ServiceAttribute).Assembly,            // Abstractions assembly
                Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly() // Entry assembly
            }.Where(a => a != null).Distinct().ToArray();

            await _registry.ScanAssembliesAsync(assemblies, scanner => scanner
                .IncludeTypes(type => !type.IsAbstract && 
                                    !type.IsInterface && 
                                    !type.IsGenericTypeDefinition &&
                                    type.IsPublic)
                .ExcludeTypes(type => type.Name.EndsWith("Test") || 
                                    type.Name.EndsWith("Mock") ||
                                    type.Namespace?.Contains("Test") == true)
                .WithDefaultLifetime(ServiceLifetime.Scoped)
                .WithConventions(conventions =>
                {
                    // Services with [Service] attribute
                    conventions.ForTypesWithAttribute<ServiceAttribute>()
                        .RegisterAs(type => type.GetInterfaces().FirstOrDefault() ?? type)
                        .UseLifetime(ServiceLifetime.Scoped)
                        .WithTags("auto-registered", "attributed");

                    // Repository pattern
                    conventions.ForTypesMatching("*Repository")
                        .RegisterAs(type => type.GetInterfaces()
                            .FirstOrDefault(i => i.Name.Contains("Repository")) ?? type)
                        .UseLifetime(ServiceLifetime.Scoped)
                        .WithTags("repository", "data", "auto-registered")
                        .WithMetadata("Pattern", "Repository");

                    // Service pattern
                    conventions.ForTypesMatching("*Service")
                        .RegisterAs(type => type.GetInterfaces()
                            .FirstOrDefault(i => i.Name.Contains("Service")) ?? type)
                        .UseLifetime(ServiceLifetime.Scoped)
                        .WithTags("service", "auto-registered")
                        .WithMetadata("Pattern", "Service");

                    // Controller pattern
                    conventions.ForTypesMatching("*Controller")
                        .RegisterAs(type => type.GetInterfaces()
                            .FirstOrDefault(i => i.Name.Contains("Controller")) ?? type)
                        .UseLifetime(ServiceLifetime.Transient)
                        .WithTags("controller", "web", "auto-registered")
                        .WithMetadata("Pattern", "Controller");

                    // Handler pattern (CQRS)
                    conventions.ForTypesMatching("*Handler")
                        .RegisterAs(type => type.GetInterfaces()
                            .FirstOrDefault(i => i.Name.Contains("Handler")) ?? type)
                        .UseLifetime(ServiceLifetime.Transient)
                        .WithTags("handler", "cqrs", "auto-registered")
                        .WithMetadata("Pattern", "Handler")
                        .When(ctx => ctx.GetConfigurationValue<bool>("Features:EnableCQRS", true));

                    // Factory pattern
                    conventions.ForTypesMatching("*Factory")
                        .RegisterAs(type => type.GetInterfaces()
                            .FirstOrDefault(i => i.Name.Contains("Factory")) ?? type)
                        .UseLifetime(ServiceLifetime.Singleton)
                        .WithTags("factory", "creational", "auto-registered")
                        .WithMetadata("Pattern", "Factory");

                    // Manager pattern
                    conventions.ForTypesMatching("*Manager")
                        .RegisterAs(type => type.GetInterfaces()
                            .FirstOrDefault(i => i.Name.Contains("Manager")) ?? type)
                        .UseLifetime(ServiceLifetime.Singleton)
                        .WithTags("manager", "coordination", "auto-registered")
                        .WithMetadata("Pattern", "Manager");
                })
                .EnableParallelScanning()
                .WithProgress(progress =>
                {
                    if (progress.ProcessedTypes % 10 == 0 || progress.PercentComplete >= 1.0)
                    {
                        _logger.LogInformation("📊 Scanning Progress: {Processed}/{Total} ({Percent:P0}) - Registered: {Registered}",
                            progress.ProcessedTypes, progress.TotalTypes, progress.PercentComplete, progress.RegisteredTypes);
                    }
                }));

            _logger.LogInformation("✅ Assembly scanning completed");
        }

        // ================================================================================
        // STEP 5: COMPREHENSIVE VALIDATION
        // ================================================================================

        private async Task ValidateCompleteRegistryAsync()
        {
            _logger.LogInformation("🔍 Performing Comprehensive Registry Validation");

            var validationOptions = new ServiceValidationOptions
            {
                ValidateConstructorParameters = true,
                ValidateInterfaceImplementation = true,
                ValidateLifetimeCompatibility = true,
                ValidateDependencyAvailability = true,
                OnValidationFailure = issue => 
                    _logger.LogWarning("⚠️ Validation Issue [{Category}]: {Message}", issue.Category, issue.Message)
            };

            var validationReport = await _registry.ValidateAsync(validationOptions);

            _logger.LogInformation("📋 Validation Results:");
            _logger.LogInformation("   ✅ Valid: {IsValid}", validationReport.IsValid);
            _logger.LogInformation("   📊 Services Validated: {Count}", validationReport.ServicesValidated);
            _logger.LogInformation("   ❌ Errors: {Count}", validationReport.Errors.Count);
            _logger.LogInformation("   ⚠️ Warnings: {Count}", validationReport.Warnings.Count);
            _logger.LogInformation("   ℹ️ Information: {Count}", validationReport.Information.Count);
            _logger.LogInformation("   ⏱️ Duration: {Duration}ms", validationReport.ValidationDuration.TotalMilliseconds);

            // Log validation issues
            foreach (var error in validationReport.Errors.Take(5))
            {
                _logger.LogError("❌ ERROR [{Category}]: {Message}", error.Category, error.Message);
            }

            foreach (var warning in validationReport.Warnings.Take(5))
            {
                _logger.LogWarning("⚠️ WARNING [{Category}]: {Message}", warning.Category, warning.Message);
            }

            // Perform dependency analysis
            var dependencyReport = await _registry.AnalyzeDependenciesAsync();
            
            _logger.LogInformation("🔗 Dependency Analysis:");
            _logger.LogInformation("   📊 Total Services: {Total}", dependencyReport.Statistics.TotalServices);
            _logger.LogInformation("   🔗 Total Dependencies: {Total}", dependencyReport.Statistics.TotalDependencies);
            _logger.LogInformation("   📏 Max Depth: {Depth}", dependencyReport.Statistics.MaxDependencyDepth);
            _logger.LogInformation("   📈 Avg Dependencies/Service: {Avg:F2}", dependencyReport.Statistics.AverageDependenciesPerService);

            if (dependencyReport.CircularDependencies.Any())
            {
                _logger.LogWarning("🔄 Circular Dependencies Found: {Count}", dependencyReport.CircularDependencies.Count);
                foreach (var circular in dependencyReport.CircularDependencies.Take(3))
                {
                    _logger.LogWarning("   🔄 {Severity}: {Description}", circular.Severity, circular.Description);
                }
            }

            if (dependencyReport.OrphanedServices.Any())
            {
                _logger.LogInformation("🏝️ Orphaned Services: {Count}", dependencyReport.OrphanedServices.Count);
            }

            _logger.LogInformation("✅ Registry validation completed");
        }

        // ================================================================================
        // STEP 6: BUILD ADVANCED SERVICE PROVIDER
        // ================================================================================

        private async Task<IServiceProvider> BuildAdvancedServiceProviderAsync()
        {
            _logger.LogInformation("🏭 Building Advanced Service Provider");

            // Create service collection and populate from registry
            var services = new ServiceCollection();

            // Add framework services
            services.AddLogging();
            services.AddSingleton(_configuration);
            services.AddSingleton(_registry);

            // Add all registered services from the registry
            var registeredServices = await _registry.GetAllServicesAsync();
            foreach (var service in registeredServices)
            {
                // Convert advanced service descriptor to Microsoft.Extensions.DI format
                services.Add(new ServiceDescriptor(
                    service.ServiceType,
                    service.ImplementationType ?? service.ServiceType,
                    service.Lifetime));
            }

            var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

            _logger.LogInformation("✅ Service Provider built with {Count} services", registeredServices.Count);
            return serviceProvider;
        }

        // ================================================================================
        // STEP 7: ADVANCED SCOPE OPERATIONS
        // ================================================================================

        private async Task DemonstrateAdvancedScopeOperationsAsync(IServiceProvider serviceProvider)
        {
            _logger.LogInformation("🎯 Demonstrating Advanced Scope Operations");

            // Get the advanced scope factory
            var scopeFactory = serviceProvider.GetService<IAdvancedServiceScopeFactory>();
            if (scopeFactory == null)
            {
                _logger.LogWarning("Advanced scope factory not available, using basic scopes");
                return;
            }

            // Create hierarchical scopes
            await using var parentScope = await scopeFactory.CreateScopeAsync(options => options
                .WithName("ParentScope")
                .WithMetadata("Level", "Parent")
                .WithTags("demo", "parent")
                .EnableDiagnostics()
                .EnableValidation());

            _logger.LogInformation("📦 Created parent scope: {ScopeId}", parentScope.Id);

            await using var childScope = await scopeFactory.CreateChildScopeAsync(parentScope, options => options
                .WithName("ChildScope")
                .WithMetadata("Level", "Child")
                .WithTags("demo", "child")
                .InheritServicesFromParent()
                .EnableIsolation());

            _logger.LogInformation("📦 Created child scope: {ScopeId} (Parent: {ParentId})", 
                childScope.Id, childScope.ParentScope?.Id);

            // Demonstrate scoped service resolution
            try
            {
                var userService = await childScope.GetServiceAsync<IUserRepository>();
                if (userService != null)
                {
                    _logger.LogInformation("✅ Successfully resolved IUserRepository from child scope");
                }

                var scopedServices = await childScope.GetServicesAsync<ICacheService>();
                _logger.LogInformation("📦 Found {Count} ICacheService implementations in scope", scopedServices.Count());

                // Get scope diagnostics
                var diagnostics = await childScope.GetDiagnosticsAsync();
                _logger.LogInformation("📊 Scope Diagnostics: {ResolvedServices} services resolved, {CreationTime}ms creation time",
                    diagnostics.ResolvedServices.Count, diagnostics.CreationTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error during scoped service resolution");
            }

            _logger.LogInformation("✅ Advanced scope operations demonstrated");
        }

        // ================================================================================
        // STEP 8: RUNTIME SERVICE RESOLUTION
        // ================================================================================

        private async Task DemonstrateRuntimeServiceResolutionAsync(IServiceProvider serviceProvider)
        {
            _logger.LogInformation("🎯 Demonstrating Runtime Service Resolution");

            try
            {
                // Resolve core services
                var loggingService = serviceProvider.GetService<IAdvancedLoggingService>();
                if (loggingService != null)
                {
                    _logger.LogInformation("✅ Resolved IAdvancedLoggingService: {Type}", loggingService.GetType().Name);
                    await loggingService.LogAsync("Test message from resolved service");
                }

                // Resolve data services
                var userRepository = serviceProvider.GetService<IUserRepository>();
                if (userRepository != null)
                {
                    _logger.LogInformation("✅ Resolved IUserRepository: {Type}", userRepository.GetType().Name);
                    var users = await userRepository.GetAllUsersAsync();
                    _logger.LogInformation("📊 Retrieved {Count} users from repository", users?.Count() ?? 0);
                }

                // Resolve configuration-dependent services
                var cacheService = serviceProvider.GetService<ICacheService>();
                if (cacheService != null)
                {
                    _logger.LogInformation("✅ Resolved ICacheService: {Type}", cacheService.GetType().Name);
                    await cacheService.SetAsync("test-key", "test-value");
                    var value = await cacheService.GetAsync<string>("test-key");
                    _logger.LogInformation("💾 Cache test: Set and retrieved value: {Value}", value);
                }

                // Resolve multiple implementations
                var emailServices = serviceProvider.GetServices<IEmailService>();
                _logger.LogInformation("📧 Found {Count} IEmailService implementations", emailServices.Count());

                // Demonstrate factory-created services
                var databaseService = serviceProvider.GetService<IDatabaseService>();
                if (databaseService != null)
                {
                    _logger.LogInformation("✅ Resolved IDatabaseService: {Type}", databaseService.GetType().Name);
                    var isConnected = await databaseService.TestConnectionAsync();
                    _logger.LogInformation("🔌 Database connection test: {Result}", isConnected ? "Success" : "Failed");
                }

                // Show service statistics
                await ShowServiceStatisticsAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during runtime service resolution");
            }

            _logger.LogInformation("✅ Runtime service resolution demonstrated");
        }

        // ================================================================================
        // HELPER METHODS
        // ================================================================================

        private async Task ShowServiceStatisticsAsync()
        {
            var statistics = await _registry.GetStatisticsAsync();
            var diagnostics = await _registry.GetDiagnosticsAsync();
            var performance = await _registry.GetPerformanceMetricsAsync();

            _logger.LogInformation("📊 Final System Statistics:");
            _logger.LogInformation("   🔧 Total Services: {Total}", statistics.TotalServices);
            
            foreach (var (lifetime, count) in statistics.ServicesByLifetime)
            {
                _logger.LogInformation("   📦 {Lifetime}: {Count}", lifetime, count);
            }

            _logger.LogInformation("   🏷️ Most Used Tags:");
            foreach (var (tag, count) in statistics.MostUsedTags.Take(5))
            {
                _logger.LogInformation("      - {Tag}: {Count}", tag, count);
            }

            _logger.LogInformation("   ⚡ Performance:");
            _logger.LogInformation("      - Avg Registration: {Time:F2}ms", performance.AverageRegistrationTime.TotalMilliseconds);
            _logger.LogInformation("      - Avg Lookup: {Time:F2}ms", performance.AverageLookupTime.TotalMilliseconds);
            _logger.LogInformation("      - Cache Hit Rate: {Rate:P2}", performance.CacheHitRate);

            _logger.LogInformation("   💾 Memory:");
            _logger.LogInformation("      - Registry: {Memory:N0} bytes", diagnostics.MemoryInfo.RegistryMemoryUsage);
            _logger.LogInformation("      - Total Estimated: {Memory:N0} bytes", diagnostics.MemoryInfo.TotalEstimatedMemoryUsage);
        }

        private async Task HandleResolutionFailureAsync(IResolutionContext context, Exception error)
        {
            _logger.LogError(error, "🚨 Resolution failure for {ServiceType}", context.ServiceType);
            
            // Could implement fallback strategies, notification systems, etc.
            // For now, just log additional context
            _logger.LogInformation("🔍 Resolution Context: Scope={Scope}, RequestedBy={RequestedBy}",
                context.Scope?.GetType().Name ?? "None",
                context.RequestingType?.Name ?? "Unknown");
        }

        private IAdvancedServiceRegistry CreateAdvancedRegistry()
        {
            // In a real implementation, this would create the actual registry instance
            // For now, return a mock or throw NotImplementedException
            throw new NotImplementedException("Advanced registry factory implementation required");
        }

        private IModernContainerBuilder CreateModernContainerBuilder()
        {
            // In a real implementation, this would create the actual container builder
            // For now, return a mock or throw NotImplementedException  
            throw new NotImplementedException("Modern container builder factory implementation required");
        }
    }

    // ================================================================================
    // BACKGROUND SERVICE FOR CONTINUOUS MONITORING
    // ================================================================================

    /// <summary>
    /// Background service that continuously monitors the DI system health and performance.
    /// </summary>
    public class DISystemMonitoringService : BackgroundService
    {
        private readonly IAdvancedServiceRegistry _registry;
        private readonly ILogger<DISystemMonitoringService> _logger;
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(5);

        public DISystemMonitoringService(
            IAdvancedServiceRegistry registry,
            ILogger<DISystemMonitoringService> logger)
        {
            _registry = registry;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔍 Starting DI System Monitoring Service");

            using var timer = new PeriodicTimer(_monitoringInterval);
            
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await PerformHealthCheckAsync();
                    await MonitorPerformanceAsync();
                    await CheckMemoryUsageAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during DI system monitoring");
                }
            }

            _logger.LogInformation("🛑 DI System Monitoring Service stopped");
        }

        private async Task PerformHealthCheckAsync()
        {
            var statistics = await _registry.GetStatisticsAsync();
            var isHealthy = statistics.TotalServices > 0;

            if (isHealthy)
            {
                _logger.LogDebug("✅ DI System Health Check: OK - {ServiceCount} services registered", 
                    statistics.TotalServices);
            }
            else
            {
                _logger.LogWarning("⚠️ DI System Health Check: No services registered");
            }
        }

        private async Task MonitorPerformanceAsync()
        {
            var performance = await _registry.GetPerformanceMetricsAsync();
            
            // Log performance metrics
            _logger.LogDebug("⚡ Performance Metrics: Avg Registration {RegTime:F2}ms, Avg Lookup {LookupTime:F2}ms, Cache Hit {CacheHit:P1}",
                performance.AverageRegistrationTime.TotalMilliseconds,
                performance.AverageLookupTime.TotalMilliseconds,
                performance.CacheHitRate);

            // Alert on performance thresholds
            if (performance.AverageRegistrationTime.TotalMilliseconds > 100)
            {
                _logger.LogWarning("🐌 Slow registration performance: {Time:F2}ms", 
                    performance.AverageRegistrationTime.TotalMilliseconds);
            }

            if (performance.CacheHitRate < 0.8)
            {
                _logger.LogWarning("📉 Low cache hit rate: {Rate:P1}", performance.CacheHitRate);
            }
        }

        private async Task CheckMemoryUsageAsync()
        {
            var diagnostics = await _registry.GetDiagnosticsAsync();
            var memoryInfo = diagnostics.MemoryInfo;

            _logger.LogDebug("💾 Memory Usage: Registry {RegistryMem:N0} bytes, Total {TotalMem:N0} bytes",
                memoryInfo.RegistryMemoryUsage,
                memoryInfo.TotalEstimatedMemoryUsage);

            // Alert on high memory usage (>100MB)
            if (memoryInfo.TotalEstimatedMemoryUsage > 100_000_000)
            {
                _logger.LogWarning("🚨 High memory usage detected: {Memory:N0} bytes", 
                    memoryInfo.TotalEstimatedMemoryUsage);
            }
        }
    }

    // ================================================================================
    // EXAMPLE SERVICE INTERFACES AND IMPLEMENTATIONS
    // ================================================================================

    // Core Services
    public interface IAdvancedLoggingService
    {
        Task LogAsync(string message);
    }

    public interface IConfigurationManager
    {
        Task<T> GetValueAsync<T>(string key);
    }

    public class ConsoleLoggingService : IAdvancedLoggingService
    {
        public Task LogAsync(string message)
        {
            Console.WriteLine($"[CONSOLE] {DateTime.Now:HH:mm:ss} - {message}");
            return Task.CompletedTask;
        }
    }

    public class StructuredLoggingService : IAdvancedLoggingService
    {
        public Task LogAsync(string message)
        {
            Console.WriteLine($"[STRUCTURED] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}");
            return Task.CompletedTask;
        }
    }

    public class AdvancedConfigurationManager : IConfigurationManager
    {
        public Task<T> GetValueAsync<T>(string key)
        {
            return Task.FromResult(default(T));
        }
    }

    // Data Services
    public interface IDatabaseService
    {
        Task InitializeAsync();
        Task<bool> TestConnectionAsync();
    }

    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
    }

    public class AdvancedDatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public AdvancedDatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task<bool> TestConnectionAsync() => Task.FromResult(true);
    }

    public class AdvancedUserRepository : IUserRepository
    {
        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return Task.FromResult(Enumerable.Empty<User>());
        }
    }

    public class CachedUserRepository : IUserRepository
    {
        private readonly IUserRepository _inner;
        private readonly ICacheService _cache;

        public CachedUserRepository(IUserRepository inner, ICacheService cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var cached = await _cache.GetAsync<IEnumerable<User>>("all-users");
            if (cached != null) return cached;

            var users = await _inner.GetAllUsersAsync();
            await _cache.SetAsync("all-users", users, TimeSpan.FromMinutes(5));
            return users;
        }
    }

    // Web Services
    public interface IUserApiController
    {
        Task<IEnumerable<User>> GetUsersAsync();
    }

    public interface IAuthenticationMiddleware
    {
        Task<bool> AuthenticateAsync(string token);
    }

    public class UserApiControllerV2 : IUserApiController
    {
        public Task<IEnumerable<User>> GetUsersAsync()
        {
            return Task.FromResult(Enumerable.Empty<User>());
        }
    }

    public class JwtAuthenticationMiddleware : IAuthenticationMiddleware
    {
        public Task<bool> AuthenticateAsync(string token)
        {
            return Task.FromResult(!string.IsNullOrEmpty(token));
        }
    }

    // Infrastructure Services
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T> GetAsync<T>(string key);
    }

    public interface IMessageQueueService
    {
        Task PublishAsync<T>(T message);
    }

    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }

    public class RedisCacheService : ICacheService
    {
        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) => Task.CompletedTask;
        public Task<T> GetAsync<T>(string key) => Task.FromResult(default(T));
    }

    public class MemoryCacheService : ICacheService
    {
        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) => Task.CompletedTask;
        public Task<T> GetAsync<T>(string key) => Task.FromResult(default(T));
    }

    public class RabbitMQService : IMessageQueueService
    {
        public Task PublishAsync<T>(T message) => Task.CompletedTask;
    }

    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;
        public SendGridEmailService(string apiKey) { _apiKey = apiKey; }
        public Task SendAsync(string to, string subject, string body) => Task.CompletedTask;
    }

    public class SmtpEmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        public SmtpEmailService(string host, int port) { _host = host; _port = port; }
        public Task SendAsync(string to, string subject, string body) => Task.CompletedTask;
    }

    public class MockEmailService : IEmailService
    {
        public Task SendAsync(string to, string subject, string body) => Task.CompletedTask;
    }

    // Supporting Types
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public interface ILoggingInterceptor { }
    public class PerformanceLoggingInterceptor : ILoggingInterceptor { }

    // Placeholder types
    public interface IResolutionContext
    {
        Type ServiceType { get; }
        Type? RequestingType { get; }
        object? Scope { get; }
    }
}
