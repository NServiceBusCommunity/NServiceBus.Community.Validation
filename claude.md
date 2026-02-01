# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NServiceBus.Community.Validation provides message validation for NServiceBus using two validation libraries:
- **NServiceBus.Community.FluentValidation** - Uses FluentValidation
- **NServiceBus.Community.DataAnnotations** - Uses System.ComponentModel.DataAnnotations

Both validate incoming and outgoing messages in the NServiceBus pipeline and add validation exceptions to unrecoverable exceptions to avoid unnecessary retries.

## Build Commands

```bash
# Build all projects
dotnet build src/NServiceBus.Validation.sln

# Run all tests
dotnet test src/NServiceBus.Validation.sln

# Run tests for a specific project
dotnet test src/NServiceBus.FluentValidation.Tests/NServiceBus.Community.FluentValidation.Tests.csproj
dotnet test src/NServiceBus.DataAnnotations.Tests/NServiceBus.Community.DataAnnotations.Tests.csproj

# Run a single test
dotnet test src/NServiceBus.FluentValidation.Tests --filter "FullyQualifiedName~IncomingTests.With_validator_invalid"
```

## Architecture

### Pipeline Behaviors
Both validation libraries implement NServiceBus pipeline behaviors:
- `IncomingValidationBehavior` - Validates messages being received (wraps `IIncomingLogicalMessageContext`)
- `OutgoingValidationBehavior` - Validates messages being sent (wraps `IOutgoingLogicalMessageContext`)

### FluentValidation Components
- `FluentValidationExtensions` - Entry point via `UseFluentValidation()` extension method on `EndpointConfiguration`
- `MessageValidator` - Core validation logic that invokes FluentValidation validators
- `ValidationFinder` - Scans assemblies for validator types
- `EndpointValidatorTypeCache` / `IValidatorTypeCache` - Caches validators per message type
- Validators are registered via DI using `AddValidatorsFromAssemblyContaining<T>()`

### DataAnnotations Components
- `DataAnnotationsConfigurationExtensions` - Entry point via `UseDataAnnotationsValidation()` extension method
- `MessageValidator` - Uses `Validator.TryValidateObject()` from System.ComponentModel.DataAnnotations

### Testing Support
- `NServiceBus.FluentValidation.Testing` - Provides `ValidatingContext` for unit testing message handlers with validation

## Key Patterns

- All extension methods live in the `NServiceBus` namespace for discoverability
- Both libraries use the same `MessageValidationException` type
- Validators can access message headers and `ContextBag` via extension methods on `IValidationContext`
- Central package version management via `Directory.Packages.props`

## Test Framework

Tests use NUnit with Verify.NServiceBus for snapshot testing of validation exceptions. Tests spin up real NServiceBus endpoints using `LearningTransport`.
