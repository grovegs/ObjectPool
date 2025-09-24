# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GroveGames.ObjectPool is a high-performance object pooling library for .NET, Unity, and Godot with Native AOT support. The library provides thread-safe and non-thread-safe object pools for various collection types and custom objects.

## Project Structure

The repository contains three main project types:

- **Core Library** (`src/GroveGames.ObjectPool/`): The main .NET library targeting both `net9.0` and `netstandard2.1`
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
dotnet test tests/GroveGames.ObjectPool.Tests/        # Core library tests only
dotnet test tests/GroveGames.ObjectPool.Godot.Tests/  # Godot-specific tests only
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
- **Multi-targeting**: `net9.0` (with AOT support) and `netstandard2.1` (for broader compatibility)
- **Nullable Reference Types**: Enabled across the project
- **AOT Compatibility**: The `net9.0` target includes AOT analyzers and trimming support
- **Code Formatting**:
  - Automatic formatting on save configured in VS Code settings
  - `dotnet format` command respects all `.editorconfig` settings
  - GitHub Actions enforce formatting via `--verify-no-changes` flag
  - Supports whitespace, style, and analyzer-based formatting

## Testing Framework

- **Test Framework**: xUnit v3
- **Test Projects**:
  - `GroveGames.ObjectPool.Tests` (core functionality)
  - `GroveGames.ObjectPool.Godot.Tests` (Godot-specific tests)
- **Test Configuration**: Uses `xunit.runner.json` for xUnit configuration

## Build Configuration

Key build configurations:

- **Multi-targeting**: Projects support both modern .NET and legacy .NET Standard
- **AOT Support**: Native AOT compilation enabled for `net9.0` target
- **Documentation**: XML documentation generation enabled for all projects
- **Package Properties**: Centralized in `Directory.Build.props`

## SDK Version

The project targets .NET 9.0 SDK (see `global.json`). When working with this codebase, ensure you have .NET 9.0 SDK installed.

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

These sandbox projects are useful for:
- Testing changes across different platforms
- Demonstrating usage examples
- Performance testing and benchmarking
- Integration validation

## Key Dependencies

- **PolySharp**: Used for .NET Standard 2.1 compatibility (backporting modern language features)
- **System.Collections.Immutable**: For older target frameworks that don't include it
- **Microsoft.SourceLink.GitHub**: For source linking in packages