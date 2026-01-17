# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GroveGames.ObjectPool is a high-performance object pooling library for .NET, Unity, and Godot with Native AOT support. The library provides thread-safe and non-thread-safe object pools for various collection types and custom objects. It targets .NET 10.0 for modern features and netstandard2.1 for Unity compatibility.

## Project Structure

The repository contains three main project types:

- **Core Library** (`src/GroveGames.ObjectPool/`): The main .NET library targeting both `net10.0` and `netstandard2.1`
- **Godot Integration** (`src/GroveGames.ObjectPool.Godot/`): Godot-specific extensions and plugin
- **Unity Integration** (`src/GroveGames.ObjectPool.Unity/`): Unity-specific package structure

The architecture is built around several key abstractions:

1. **Core Interfaces**: `IObjectPool<T>`, `IMultiTypeObjectPool<T>` define the pooling contracts
2. **Pool Types**: Specialized pools for collections (List, Dictionary, Queue, Stack, etc.)
3. **Concurrent Pools**: Thread-safe versions in the `Concurrent/` namespace
4. **Builder Pattern**: `MultiTypeObjectPoolBuilder<T>` for configuring multi-type pools

## Development Commands

### Building
```bash
dotnet build                           # Build all projects
dotnet build -c Release               # Release build
```

### Testing
```bash
dotnet test                           # Run all tests
dotnet test tests/GroveGames.ObjectPool.Tests/        # Core library tests
```

### Formatting
```bash
dotnet format                         # Format all code according to .editorconfig
dotnet format --verify-no-changes    # Check if code is properly formatted (CI/CD)
dotnet format whitespace             # Format whitespace only
dotnet format style                  # Apply code style fixes
```

### Packaging
```bash
dotnet pack -c Release                # Create NuGet packages
```

## Code Style & Formatting

The project uses automated formatting via GitHub Actions and comprehensive configuration files:

### Configuration Structure

The project uses a layered configuration approach with platform-specific settings:

#### Root Configuration (Core .NET Library)
- **EditorConfig** (`.editorconfig`): Core C# coding standards, 4-space indentation, naming conventions (private fields: `_camelCase`, static: `s_camelCase`, interfaces: `IPascalCase`)
- **Git Configuration** (`.gitignore`/`.gitattributes`): Focused on .NET build outputs, NuGet packages, IDE files, and basic OS artifacts
- **VS Code Settings** (`.vscode/settings.json`): C# formatting and save actions

#### Unity-Specific Configuration (`sandbox/UnityApplication/`)
- **EditorConfig** (`.editorconfig`): Inherits core C# settings, same coding standards as main library
- **Git Configuration** (`.gitignore`/`.gitattributes`): Unity build artifacts, temp files, auto-generated content, Git LFS for large assets, Unity YAML merge drivers
- **Project Files**: Unity-specific project configuration and package management

#### Godot-Specific Configuration (`sandbox/GodotApplication/`)
- **EditorConfig** (`.editorconfig`): Inherits core C# settings plus Godot scene files (`.tscn`/`.tres`) with 2-space indentation
- **Git Configuration** (`.gitignore`/`.gitattributes`): Godot-specific files (`.godot/`, `.import/`), C# project artifacts, binary asset handling
- **Plugin Configuration** (`src/GroveGames.ObjectPool.Godot/addons/GroveGames.ObjectPool/plugin.cfg`): Godot plugin metadata
- **Project Configuration** (`project.godot`): Godot 4.3+ project with C# support and mobile rendering

### Target Frameworks & Features
- **Multi-targeting**: `net10.0` (with AOT support) and `netstandard2.1` (for Unity compatibility)
- **Nullable Reference Types**: Enabled across the project
- **AOT Compatibility**: The `net10.0` target includes AOT analyzers and trimming support
- **Polyfills via Extension Members**: Custom polyfills using C# 14 extension members (`extension(Type)` syntax) in `Polyfills/` folder provide backward compatibility for netstandard2.1. These use `#if !NET6_0_OR_GREATER` preprocessor directives and are placed in the `System` namespace for seamless usage:
  - `CallerArgumentExpressionAttribute` for parameter name capture
  - `ArgumentNullException.ThrowIfNull` static method
  - `ArgumentOutOfRangeException.ThrowIfNegative`, `ThrowIfNegativeOrZero`, `ThrowIfGreaterThan` static methods
  - `ObjectDisposedException.ThrowIf` static method
  - `FrozenDictionary` for immutable dictionary support
- **Code Formatting**:
  - Automatic formatting on save configured in VS Code settings
  - `dotnet format` command respects all `.editorconfig` settings
  - GitHub Actions enforce formatting via `--verify-no-changes` flag
  - Supports whitespace, style, and analyzer-based formatting

## Platform Integration Structure

### Unity Package Structure (`src/GroveGames.ObjectPool.Unity/Packages/com.grovegames.objectpool/`)
Standard Unity Package Manager (UPM) layout:
- `Runtime/` - Runtime scripts with `.asmdef`
- `Editor/` - Editor scripts with `.asmdef`
- `Tests/Runtime/` - Unity Test Framework tests with `.asmdef`
- `package.json` - UPM package manifest
- `LICENSE` → symlink to root LICENSE
- `README.md` → symlink to root README.md

Unity-specific components:
- `IGameObjectPool` / `GameObjectPool` - Pool interface and implementation for Unity GameObjects
- `IComponentPool<T>` / `ComponentPool<T>` - Pool interface and implementation for Unity Components
- `GameObjectRental` / `ComponentRental<T>` - RAII-style rental structs for automatic return on dispose
- `GameObjectPoolExtensions` / `ComponentPoolExtensions` - Extension methods for fluent API

The Unity package requires the core NuGet package (`GroveGames.ObjectPool`) installed via NuGetForUnity, then the Unity package via git URL.

### Godot Addon Structure (`src/GroveGames.ObjectPool.Godot/addons/GroveGames.ObjectPool/`)
Standard Godot addon layout:
- `plugin.cfg` - Godot plugin configuration
- `Plugin.cs` - Plugin entry point (EditorPlugin)
- `LICENSE` → symlink to root LICENSE
- `README.md` → symlink to root README.md

The Godot addon requires the NuGet package (`GroveGames.ObjectPool.Godot`) plus the addon files extracted to project's `addons/` folder.

## Testing Framework

- **Test Framework**: xUnit v3
- **Test Projects**:
  - `GroveGames.ObjectPool.Tests` (core functionality)
- **Test Configuration**: Uses `xunit.runner.json` for xUnit configuration

## Build Configuration

Key build configurations:

- **Multi-targeting**: Projects support both modern .NET and legacy .NET Standard
- **AOT Support**: Native AOT compilation enabled for `net10.0` target
- **Documentation**: XML documentation generation enabled for all projects
- **Package Properties**: Centralized in `Directory.Build.props`

## SDK Version

The project targets .NET 10.0 SDK (see `global.json`). When working with this codebase, ensure you have .NET 10.0 SDK installed.

## GitHub Workflows

The project uses reusable workflows from `grovegs/workflows`:

- **Tests** (`tests.yml`): Runs on pushes/PRs to main/develop branches
- **Format** (`format.yml`): Validates code formatting
- **Release** (`release.yml`): Manual workflow for creating releases and publishing NuGet packages

## Development Sandbox

The `sandbox/` directory contains sample applications for testing and development:

- **ConsoleApplication**: Basic .NET console app for testing core functionality
- **UnityApplication**: Full Unity project for testing Unity integration
- **GodotApplication**: Godot project with the object pool addon for testing Godot integration
- **DotnetBenchmark**: Performance benchmarking application

### Unity Sandbox Setup

The Unity sandbox project uses a symlink to reference the Unity package locally:

- `Packages/com.grovegames.objectpool` → symlink to `src/GroveGames.ObjectPool.Unity/Packages/com.grovegames.objectpool`
- `Assets/Editor/PluginBuilder/` → separate assembly that auto-builds `GroveGames.ObjectPool.dll` to `Assets/Plugins`
- `Packages/manifest.json` → includes NuGetForUnity via OpenUPM scoped registry
- `Assets/packages.config` → NuGet package dependencies

The PluginBuilder is in a separate assembly (`PluginBuilder.Editor.asmdef`) with no dependencies, allowing it to compile even when the Unity package has errors due to a missing DLL. On first Unity open, it builds the DLL automatically, then refreshes so the package can compile.

### Godot Sandbox Setup

The Godot sandbox project uses a symlink to reference the Godot addon locally:

- `addons/GroveGames.ObjectPool` → symlink to `src/GroveGames.ObjectPool.Godot/addons/GroveGames.ObjectPool`

These sandbox projects are useful for:
- Testing changes across different platforms
- Demonstrating usage examples
- Performance testing and benchmarking
- Integration validation

## Key Dependencies

- **System.Collections.Immutable**: For older target frameworks that don't include it (used by FrozenDictionary polyfill)
- **Microsoft.SourceLink.GitHub**: For source linking in packages

Note: Previously used PolySharp has been replaced with custom polyfill extensions in the `System` namespace for better control and lighter weight.

## Template Patterns for Other Packages

This project structure serves as a template for other GroveGames packages. Key patterns to follow:

### Interface Abstraction for External Dependencies

When using types from NuGet packages that are only available in newer .NET versions, wrap them in custom interfaces to avoid exposing them in the public API. This prevents Unity/Godot users from needing to reference those packages directly.

Example pattern (from Logger project):

```csharp
public interface ITimeProvider
{
    DateTimeOffset GetUtcNow();
}

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow();
}

public SomeFactory(..., ITimeProvider? timeProvider = null)
{
    _timeProvider = timeProvider ?? new SystemTimeProvider();
}
```

This pattern ensures:

- Public API only exposes types defined in the library
- External dependencies are implementation details
- Unity/Godot projects compile without needing polyfill packages in their project
- Easy unit testing via mock implementations

### Platform-Specific Project Settings

Both Unity and Godot integrations can provide project settings for configuration (see Logger project for full example):

**Unity** (via `SettingsProvider`):
- Settings stored in `ProjectSettings/GroveGames{PackageName}Settings.json`
- UI at Edit > Project Settings > Grove Games > {Package}
- Settings class with `Load()` and `Save()` methods
- `SettingsProvider` subclass for Editor UI

**Godot** (via `ProjectSettings`):
- Settings stored in Godot's project settings under `grove_games/{package}/`
- Static settings class with `GodotSetting<T>` typed accessors
- `CreateIfNotExist()` to initialize defaults

Both provide factory classes and builder extensions for easy setup.

### Backward Compatibility via Static Extensions

Use C# 14 extension members with preprocessor directives for .NET API polyfills:

```csharp
#if !NET6_0_OR_GREATER
namespace System;

internal static class ArgumentNullExceptionExtensions
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
#endif
```

### README Structure

Consistent platform sections with installation + usage + components:

1. **Features** - Brief feature list
2. **.NET** - NuGet install, usage examples, core/concurrent components
3. **Unity** - NuGetForUnity + git URL install, Unity-specific usage, Unity components
4. **Godot** - NuGet + addon install, Godot-specific usage
5. **Architecture** - Performance optimizations
6. **Testing/Contributing/License**

### Sandbox Applications

Always include sandbox projects for testing:

- `sandbox/ConsoleApplication/` - Basic .NET testing
- `sandbox/UnityApplication/` - Unity integration testing (symlink to Unity package, isolated PluginBuilder assembly for auto DLL build)
- `sandbox/GodotApplication/` - Godot integration testing (symlink to Godot addon)
- `sandbox/DotnetBenchmark/` - Performance benchmarking
