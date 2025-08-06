# DI Attribute System - Developer Experience Guide

## Overview

This comprehensive DI attribute system provides a sophisticated, developer-friendly approach to service registration with excellent IntelliSense support, validation, and flexibility.

## Key Features

### 🎯 **Developer Experience (DX) Focused**
- **Rich IntelliSense**: All attributes have comprehensive XML documentation
- **Compile-time Validation**: Catches configuration errors during build
- **Clear Error Messages**: Descriptive exceptions with actionable guidance
- **Consistent API**: All attributes follow the same patterns and conventions

### 🚀 **Versatile Registration Patterns**
- **Basic Registration**: Simple `[Service]` attribute for common scenarios
- **Contract Specification**: `[ProvidesContract]` for interface-based registration
- **Conditional Registration**: `[ServiceCondition]` for environment/config-based registration
- **Decorator Pattern**: `[ServiceDecorator]` with automatic ordering
- **Factory Pattern**: `[ServiceFactory]` for dynamic service creation
- **Keyed Services**: Support for named service registrations

### 🔧 **Sophisticated Features**
- **Priority System**: Control resolution order with priority values
- **Metadata Support**: Rich metadata for filtering and discovery
- **Tagging System**: Semantic tags for service categorization
- **Validation**: Comprehensive attribute validation with clear error messages
- **Exclusion Support**: Exclude services from automatic registration

## Attribute Reference

### Core Attributes

#### `[Service]` - Basic Service Registration
```csharp
[Service(ServiceLifetime.Scoped, Priority = 10)]
[ProvidesContract(typeof(IMyService), Primary = true)]
public class MyService : IMyService { }
```

**Properties:**
- `Lifetime`: Service lifetime (Transient, Scoped, Singleton)
- `Priority`: Registration priority (higher = preferred)
- `Replace`: Whether to replace existing registrations
- `FactoryType`: Custom factory for service creation
- `RegisterSelf`: Register the concrete type in addition to contracts

#### `[ProvidesContract]` - Contract Specification
```csharp
[ProvidesContract(typeof(IEmailService), Primary = true, ServiceKey = "production")]
public class ProductionEmailService : IEmailService { }
```

**Properties:**
- `ContractType`: The interface/abstract class this service implements
- `Primary`: Whether this is the primary implementation
- `ServiceKey`: Named service key for keyed registrations

#### `[ServiceCondition]` - Conditional Registration
```csharp
// Environment-based
[ServiceCondition("Development", "Testing")]

// Configuration-based
[ServiceCondition("Features:EmailEnabled", "true")]
[ServiceCondition("Email:Provider", "SendGrid", "Mailgun")]
```

**Properties:**
- `ConfigurationKey`: Configuration key to evaluate
- `ExpectedValues`: Values that enable registration (OR condition)
- `EnvironmentNames`: Environment names to match
- `Negate`: Invert the condition logic

#### `[ServiceDecorator]` - Decorator Pattern
```csharp
[ServiceDecorator(typeof(IEmailService), Order = 1)]
public class LoggingEmailDecorator : IEmailService
{
    public LoggingEmailDecorator(IEmailService inner) { }
}
```

**Properties:**
- `ServiceType`: The service type being decorated
- `Order`: Decoration order (lower values closer to original)

#### `[ServiceFactory]` - Factory Pattern
```csharp
[ServiceFactory(typeof(IEmailService))]
public class EmailServiceFactory : IServiceFactory<IEmailService>
{
    public IEmailService Create(IServiceProvider provider) => new EmailService();
}
```

### Metadata Attributes

All attributes support rich metadata through base class properties:

```csharp
[Service(ServiceLifetime.Singleton,
    Priority = 10,
    Tags = new[] { "communication", "external" },
    Category = "Infrastructure",
    Description = "Primary email service using SendGrid",
    Metadata = new[] { "Provider=SendGrid", "Version=2.0", "Region=US-East" })]
```

**Metadata Properties:**
- `Priority`: Registration priority (default: 0)
- `Tags`: String array for semantic filtering
- `Category`: Service category for grouping
- `Description`: Human-readable description
- `Metadata`: Key-value pairs (format: "Key=Value")

### Utility Attributes

#### `[ExcludeFromRegistration]` - Exclusion
```csharp
[ExcludeFromRegistration("Abstract base class")]
public abstract class BaseService { }
```

Prevents automatic registration scanning from registering specific types.

## Usage Patterns

### 1. Simple Service Registration
```csharp
[Service(ServiceLifetime.Scoped)]
[ProvidesContract(typeof(IEmailService))]
public class EmailService : IEmailService { }
```

### 2. Multi-Contract Service
```csharp
[Service(ServiceLifetime.Singleton)]
[ProvidesContract(typeof(IEmailService), Primary = true)]
[ProvidesContract(typeof(INotificationService))]
public class CommunicationService : IEmailService, INotificationService { }
```

### 3. Environment-Specific Service
```csharp
[Service(ServiceLifetime.Scoped)]
[ProvidesContract(typeof(IEmailService))]
[ServiceCondition("Development")]
public class MockEmailService : IEmailService { }
```

### 4. Feature-Flagged Service
```csharp
[Service(ServiceLifetime.Scoped)]
[ProvidesContract(typeof(IAdvancedEmailService))]
[ServiceCondition("Features:AdvancedEmail", "true")]
[ServiceCondition("Email:Provider", "SendGrid")]
public class AdvancedEmailService : IAdvancedEmailService { }
```

### 5. Decorator Chain
```csharp
// Base service
[Service(ServiceLifetime.Scoped)]
[ProvidesContract(typeof(IEmailService), Primary = true)]
public class EmailService : IEmailService { }

// First decorator (closest to original)
[ServiceDecorator(typeof(IEmailService), Order = 0)]
public class RetryEmailDecorator : IEmailService { }

// Second decorator (outermost)
[ServiceDecorator(typeof(IEmailService), Order = 1)]
public class LoggingEmailDecorator : IEmailService { }
```

### 6. Keyed Services
```csharp
[Service(ServiceLifetime.Scoped)]
[ProvidesContract(typeof(IEmailService), ServiceKey = "production")]
public class ProductionEmailService : IEmailService { }

[Service(ServiceLifetime.Scoped)]
[ProvidesContract(typeof(IEmailService), ServiceKey = "development")]
public class DevelopmentEmailService : IEmailService { }
```

## Best Practices

### 1. **Use Primary Contracts Sparingly**
Only mark one implementation as `Primary = true` per contract to avoid ambiguity.

### 2. **Leverage Conditional Registration**
Use conditions to avoid runtime service resolution errors in different environments.

### 3. **Organize with Metadata**
Use categories and tags consistently across your application for better service discovery.

### 4. **Validate Early**
The system validates configurations at registration time, catching errors early in the development cycle.

### 5. **Document Intent**
Use the `Description` property to document the purpose and behavior of services.

### 6. **Order Decorators Thoughtfully**
Lower order values are applied first (closer to the original service).

## Error Handling

The system provides comprehensive validation with clear error messages:

```csharp
// This will throw a clear validation error
[ProvidesContract(typeof(string))] // ❌ Not an interface or abstract class
public class BadService { }

// This will throw at registration time
[ServiceDecorator(typeof(IEmailService))] // ❌ Missing constructor parameter
public class BadDecorator : IEmailService { }
```

## Integration

The attribute system is designed to integrate seamlessly with:
- ASP.NET Core dependency injection
- Microsoft.Extensions.DependencyInjection
- Popular third-party containers (Autofac, Castle Windsor, etc.)
- Configuration systems (IConfiguration, IOptions)
- Environment detection systems

This comprehensive system provides maximum flexibility while maintaining excellent developer experience through clear APIs, rich metadata support, and comprehensive validation.
