# Speckle Sharp Documentation Index

This index helps you navigate the comprehensive documentation for the Speckle Sharp monorepo.

## 📚 Documentation Files

### Core Architecture & Getting Started
- **[CLAUDE.md](CLAUDE.md)** - Start here! High-level architecture overview and quick reference
  - Project structure and layer architecture
  - Build commands and common tasks
  - Quick troubleshooting guide

### Component Deep Dives

#### Foundation Layer
- **[CORE.md](CORE.md)** (38 KB) - Speckle Core SDK
  - Base object system and serialization
  - Transport layer (Memory, SQLite, Server)
  - Kit system and converter loading
  - API client and authentication
  - 40+ code examples

- **[OBJECTS.md](OBJECTS.md)** (34 KB) - Domain Models and Objects Kit
  - 200+ object types across 8 categories
  - BuiltElements, Geometry, Structural models
  - ObjectsKit architecture
  - How to add new object types

#### Conversion Layer
- **[CONVERTERS.md](CONVERTERS.md)** (36 KB) - Converter Implementation Guide
  - 60+ converter projects documented
  - Shared code patterns (.shproj)
  - ToSpeckle vs ToNative patterns
  - Version-specific implementations
  - Testing and best practices

#### Application Layer
- **[CONNECTORS.md](CONNECTORS.md)** (30 KB) - All Connectors Architecture
  - Revit, Rhino, AutoCAD, CSI, Dynamo, Grasshopper, and more
  - Plugin registration patterns
  - Send/Receive operation flows
  - ConnectorBindings interface
  - Version management strategies

- **[DESKTOPUI.md](DESKTOPUI.md)** (32 KB) - DesktopUI2 Integration
  - Avalonia UI framework
  - ReactiveUI and MVVM patterns
  - Connector integration guide
  - Customization points
  - 40+ integration examples

#### Specialized Documentation
- **[ConnectorCSI/CSI_CONNECTORS.md](ConnectorCSI/CSI_CONNECTORS.md)** (44 KB) - CSI Suite Deep Dive
  - ETABS, SAP2000, CSIBridge, SAFE
  - Version-specific implementations (22, 25, 26)
  - Build and deployment automation
  - Known issues and solutions
  - Recent development history

## 🎯 Quick Navigation by Task

### I want to...

#### Understand the Architecture
1. Read [CLAUDE.md](CLAUDE.md) - Overview
2. Read [CORE.md](CORE.md) - Foundation concepts
3. Read [OBJECTS.md](OBJECTS.md) - Data models

#### Build and Run
- [CLAUDE.md - Build Commands](CLAUDE.md#build-commands)
- [ConnectorCSI/CSI_CONNECTORS.md - CSI Build](ConnectorCSI/CSI_CONNECTORS.md#build-and-deployment)

#### Create a New Connector
1. [CONNECTORS.md - Implementation Guide](CONNECTORS.md#implementation-guide-creating-a-new-connector)
2. [CONVERTERS.md - Adding Converters](CONVERTERS.md#adding-a-new-converter-project)
3. [DESKTOPUI.md - UI Integration](DESKTOPUI.md#connector-integration)

#### Add Object Types
1. [OBJECTS.md - Adding Object Types](OBJECTS.md#adding-new-object-types)
2. [CONVERTERS.md - Converter Logic](CONVERTERS.md#converter-architecture)

#### Work on CSI Connectors
- [ConnectorCSI/CSI_CONNECTORS.md](ConnectorCSI/CSI_CONNECTORS.md) - Complete CSI guide
- [CLAUDE.md - CSI Section](CLAUDE.md#csi-connectors-etabs-sap2000-csibridge-safe)

#### Troubleshoot Issues
- [CLAUDE.md - Troubleshooting](CLAUDE.md#troubleshooting)
- [ConnectorCSI/CSI_CONNECTORS.md - Known Issues](ConnectorCSI/CSI_CONNECTORS.md#known-issues-and-solutions)
- [CONVERTERS.md - Common Pitfalls](CONVERTERS.md#common-pitfalls-and-solutions)

#### Understand Design Patterns
- [CORE.md - Design Patterns](CORE.md#design-patterns)
- [CONVERTERS.md - Patterns](CONVERTERS.md#key-patterns)
- [DESKTOPUI.md - MVVM Patterns](DESKTOPUI.md#architecture-patterns)

## 📊 Documentation Statistics

| File | Size | Lines | Focus |
|------|------|-------|-------|
| CLAUDE.md | 25 KB | 637 | Architecture overview, quick reference |
| CORE.md | 38 KB | 1,454 | Foundation SDK, serialization, transports |
| OBJECTS.md | 34 KB | 1,200 | Domain models, object types |
| CONVERTERS.md | 36 KB | 1,210 | Converter implementations |
| CONNECTORS.md | 30 KB | 1,091 | Application connectors |
| DESKTOPUI.md | 32 KB | 927 | UI framework integration |
| CSI_CONNECTORS.md | 44 KB | 1,434 | CSI suite specialized docs |
| **Total** | **239 KB** | **7,953** | **Complete documentation** |

## 🔍 Key Concepts Reference

### Architecture Layers
```
Connectors (Application-specific)
    ↓↑
Converters (Format translation)
    ↓↑
DesktopUI2 + Core (UI + SDK)
    ↓↑
Speckle Server (API + Storage)
```

### Core Components
- **Core** - SDK, API client, transports, kit system
- **Objects** - Domain models (Walls, Beams, etc.)
- **Converters** - ToSpeckle/ToNative conversion logic
- **Connectors** - Application plugins (Revit, Rhino, etc.)
- **DesktopUI2** - Cross-platform Avalonia UI

### Key Interfaces
- `ISpeckleConverter` - Converter contract
- `ConnectorBindings` - Connector integration interface
- `Base` - All Speckle objects inherit from this
- `ITransport` - Data transport abstraction

### Important Patterns
- **Shared Projects (.shproj)** - Code sharing across versions
- **Conditional Compilation** - Version-specific behavior
- **Post-Build Automation** - MSBuild deployment targets
- **Kit System** - Runtime converter discovery
- **MVVM + ReactiveUI** - UI architecture

## 🚀 Recommended Reading Order

### For New Developers
1. [CLAUDE.md](CLAUDE.md) - Get oriented
2. [CORE.md](CORE.md) - Understand foundation
3. [OBJECTS.md](OBJECTS.md) - Learn data models
4. Pick connector-specific docs based on your work

### For Connector Developers
1. [CLAUDE.md](CLAUDE.md) - Architecture overview
2. [CONNECTORS.md](CONNECTORS.md) - Connector patterns
3. [CONVERTERS.md](CONVERTERS.md) - Conversion logic
4. [DESKTOPUI.md](DESKTOPUI.md) - UI integration

### For CSI Work
1. [CLAUDE.md](CLAUDE.md) - General architecture
2. [ConnectorCSI/CSI_CONNECTORS.md](ConnectorCSI/CSI_CONNECTORS.md) - CSI-specific everything
3. [CONVERTERS.md](CONVERTERS.md) - For converter details

### For Architecture Review
1. [CLAUDE.md](CLAUDE.md) - High-level overview
2. [CORE.md - Design Patterns](CORE.md#design-patterns)
3. [CONVERTERS.md - Patterns](CONVERTERS.md#key-patterns)
4. [DESKTOPUI.md - Architecture](DESKTOPUI.md#architecture-patterns)

## 📝 Documentation Maintenance

### When to Update
- Adding new connectors or converters
- Significant architectural changes
- New design patterns introduced
- Build process modifications
- Major bug fixes or workarounds

### Which File to Update
- **CLAUDE.md** - High-level architecture changes
- **CORE.md** - SDK API changes
- **OBJECTS.md** - New object types or categories
- **CONVERTERS.md** - New converter patterns
- **CONNECTORS.md** - New connector implementations
- **DESKTOPUI.md** - UI framework updates
- **CSI_CONNECTORS.md** - CSI-specific changes

## 🔗 External Resources

- [Speckle Documentation](https://speckle.guide/)
- [Contribution Guidelines](.github/CONTRIBUTING.md)
- [Speckle Community Forum](https://speckle.community)
- [GitHub Repository](https://github.com/specklesystems/speckle-sharp)

---

**Last Updated:** 2025-11-06
**Documentation Version:** 1.0
**Covers Speckle Sharp:** Legacy monorepo (for next-gen see speckle-sharp-connectors and speckle-sharp-sdk)
