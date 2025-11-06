# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

# Speckle Sharp Architecture Guide

This document provides a high-level overview of the Speckle Sharp monorepo architecture to help AI assistants understand how to navigate and modify this complex .NET codebase.

## Documentation Structure

This repository includes comprehensive documentation split across multiple specialized files:

- **[CORE.md](CORE.md)** - Deep dive into Core SDK: Base objects, serialization, transports, Kit system, API client
- **[OBJECTS.md](OBJECTS.md)** - Objects domain models: BuiltElements, Geometry, Structural, and the Objects Kit
- **[CONVERTERS.md](CONVERTERS.md)** - Complete converter implementation guide for all 60+ converters
- **[CONNECTORS.md](CONNECTORS.md)** - Connector architecture patterns for Revit, Rhino, AutoCAD, CSI, and more
- **[DESKTOPUI.md](DESKTOPUI.md)** - DesktopUI2 integration: Avalonia, ReactiveUI, MVVM patterns
- **[ConnectorCSI/CSI_CONNECTORS.md](ConnectorCSI/CSI_CONNECTORS.md)** - Specialized CSI connector suite documentation

**Quick Start:** Read this file first for architecture overview, then dive into specific documentation as needed.

## Project Structure Overview

Speckle Sharp is a .NET monorepo organized into five main pillars:

```
speckle-sharp/
├── Core/                          # Core SDK - Speckle protocol, API clients, transports
├── Objects/                       # Domain models and converters for all applications
├── DesktopUI2/                    # Cross-platform Avalonia-based UI (netstandard2.0)
├── ConnectorCore/                 # Shared utilities for all connectors
├── Connector*/                    # Application-specific connectors (11 different apps)
│   ├── ConnectorRevit/           # Revit 2019-2025 (6 versions)
│   ├── ConnectorCSI/             # ETABS, SAP2000, CSIBridge, SAFE
│   ├── ConnectorArchicad/
│   ├── ConnectorAutocadCivil/    # AutoCAD & Civil 3D
│   ├── ConnectorDynamo/          # Dynamo plugin for Revit
│   ├── ConnectorGrasshopper/     # Grasshopper plugin for Rhino
│   ├── ConnectorRhino/           # Rhino native connector
│   ├── ConnectorBentley/         # Bentley (MicroStation, OpenRoads, OpenBuildings, OpenRail)
│   ├── ConnectorNavisworks/      # Autodesk Navisworks
│   └── ConnectorTeklaStructures/ # Tekla Structures
├── Automate/                      # Cloud automation runner
└── All.sln                        # Master solution file
```

## Layer Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      CONNECTORS                                 │
│  (ConnectorRevit, ConnectorCSI, ConnectorDynamo, etc)          │
│  - net48 library                                                │
│  - UI bindings & business logic                                 │
│  - Plugin entry points (cPlugin.cs, etc)                       │
└─────────────────────────────────────────────────────────────────┘
                            ↓↑
┌─────────────────────────────────────────────────────────────────┐
│                     CONVERTERS                                  │
│  (Objects/Converters/*)                                         │
│  - netstandard2.0 library                                       │
│  - Application-specific domain models                           │
│  - Logic: Native ↔ Speckle conversions                         │
│  - Implements ISpeckleConverter interface                       │
└─────────────────────────────────────────────────────────────────┘
                            ↓↑
┌─────────────────────────────────────────────────────────────────┐
│  DESKTOP UI2 (UI Shell) + CORE (SDK)                            │
│  - DesktopUI2: Avalonia-based cross-platform UI                │
│  - Core: API clients, serialization, transports, kits          │
│  - Objects: Base domain models (Walls, Beams, etc)             │
└─────────────────────────────────────────────────────────────────┘
                            ↓↑
┌─────────────────────────────────────────────────────────────────┐
│              SPECKLE API & STORAGE                              │
│  (GraphQL API, Disk/MongoDB transports, authentication)         │
└─────────────────────────────────────────────────────────────────┘
```

## Core Components

### 1. Core (Speckle SDK)
**Location:** `/Core/Core/` (netstandard2.0)

**Purpose:** The foundation .NET SDK for Speckle
- **API Client:** GraphQL communication with Speckle server
- **Transports:** Data serialization/deserialization (Disk, MongoDB)
- **Kits System:** Plugin architecture for converters
  - `ISpeckleConverter` interface
  - `KitManager.cs` - dynamically loads converter assemblies
- **Models:** Base `Base` class and `ApplicationObject` for tracking conversions
- **Credentials:** Authentication handling

**Key Dependencies:**
- GraphQL.Client (API communication)
- Serilog (logging)
- NetTopologySuite (geometry)
- Polly (resilience/retry policies)

### 2. Objects
**Location:** `/Objects/Objects/` + `/Objects/Converters/` (netstandard2.0)

**Purpose:** Domain models and conversion logic

**Structure:**
```
Objects/
├── Objects/                                    # Base domain models
│   ├── BuiltElements/                         # Walls, Floors, Columns, Beams, etc
│   ├── Structural/                            # Structural-specific (Analysis, Loading, etc)
│   ├── Geometry/                              # Geometric primitives
│   └── Other domain-specific models
│
└── Converters/
    ├── ConverterRevit/
    │   ├── ConverterRevit2020-2025/          # Version-specific converters (netstandard2.0)
    │   └── ConverterRevitShared/             # Shared logic (.shproj)
    │
    ├── ConverterCSI/
    │   ├── ConverterETABS/                   # ETABS-specific (netstandard2.0)
    │   ├── ConverterETABS22/                 # ETABS v22 (netstandard2.0)
    │   ├── ConverterSAP2000/                 # SAP2000 (netstandard2.0)
    │   ├── ConverterCSIBridge/               # CSIBridge (netstandard2.0)
    │   ├── ConverterCSIBridge25/             # CSIBridge v25 (netstandard2.0)
    │   ├── ConverterCSIBridge26/             # CSIBridge v26 (netstandard2.0)
    │   ├── ConverterSAFE/                    # SAFE (netstandard2.0)
    │   └── ConverterCSIShared/               # Shared logic (.shproj)
    │
    ├── ConverterRhinoGh/                     # Rhino & Grasshopper converters
    ├── ConverterDynamo/                      # Dynamo converters
    └── ... (other converters)
```

**Converter Pattern:**
- Converters implement `ISpeckleConverter` interface
- Methods:
  - `ConvertToSpeckle(native)` → Speckle object
  - `ConvertToNative(speckle)` → Native application object
  - `CanConvertToSpeckle/ToNative()` → Capability checks
  - `SetContextDocument()` → Receives active document
  - `SetContextObjects()` → Receives related objects for dependency tracking

### 3. DesktopUI2
**Location:** `/DesktopUI2/DesktopUI2/` (netstandard2.0, Avalonia 0.10.18)

**Purpose:** Cross-platform desktop UI shell
- Runs on Windows, macOS, Linux via Avalonia framework
- Responsible for:
  - User authentication
  - Stream management (send/receive workflows)
  - Settings and configuration
  - Activity and notifications
  - Connector-specific mappings (e.g., Revit family mapping)

**Architecture:**
- MVVM pattern using ReactiveUI
- Compiled to netstandard2.0 for reusability
- Hosted in native applications:
  - **Windows:** WPF host (DesktopUI2.WPF)
  - **Revit:** HWND host (AvaloniaHwndHost) - embedded in Revit's window
  - **CSI Apps:** Native window embedding

### 4. Connectors
**Location:** `/Connector*/` folders (net48, Windows-only)

**Purpose:** Application-specific integration layer

**Anatomy of a Connector:**
```
ConnectorRevit/
├── ConnectorRevit/                          # Shared code (.projitems - shared project)
├── ConnectorRevit2021/                      # Version-specific (net48)
│   ├── ConnectorRevit2021.csproj           # References RevitSharedResources2021
│   └── Contains: MainWindow, command bindings, plugin entry
├── ConnectorRevit2022/, 2023/, etc.        # Additional versions
├── RevitSharedResources2021/                # Version-specific resources (net48)
└── RevitSharedResources2022/, 2023/, etc.  # (Revit API versions differ)
```

**Key Responsibilities:**
- **Plugin Initialization:** Hooks into application (e.g., Revit Ribbon, CSI plugin system)
- **Native API Binding:** Interfaces with host application's object model
- **Event Handling:** Selection changes, document saves, etc
- **Converter Integration:** Loads and invokes appropriate converter
- **UI Embedding:** Integrates DesktopUI2 into host application's window

**Example (CSI):**
```csharp
public class cPlugin : IExternalApplication
{
    public static Window MainWindow { get; private set; }
    public static cSapModel model { get; set; }  // ETABS/SAP2000 active model
    public static ConnectorBindingsCSI Bindings { get; set; }
    
    public static void CreateOrFocusSpeckle()
    {
        // Shows DesktopUI2 window within CSI application
        // Handles Avalonia/WPF interop
    }
}
```

## Version-Specific Implementation Pattern

### Why Multiple Versions?

Different versions of host applications have breaking API changes:
- **Revit:** 2019-2025 (7 versions, each with different API)
- **CSIBridge:** v25, v26 (different struct definitions)
- **Navisworks:** 2020-2025 (5 versions)
- **SAP2000:** Multiple versions with API differences

### Version Management Strategy

**Option 1: Completely Separate Projects** (Revit)
```
ConnectorRevit2021/
├── .csproj references RevitSharedResources2021
└── Contains only version-specific bindings

RevitSharedResources2021/
└── net48 assembly with Revit 2021 API references
```

**Option 2: Shared Code + Conditional Compilation** (CSI)
```
ConverterCSI/
├── ConverterCSIShared/           # .shproj (shared project)
│   └── All conversion logic
├── ConverterETABS/               # .csproj that includes ConverterCSIShared
│   └── #if ETABS
├── ConverterETABS22/             # .csproj that includes ConverterCSIShared
│   └── #if ETABS22
└── ConverterCSIBridge25/         # .csproj that includes ConverterCSIShared
    └── #if CSIBRIDGE25

ConnectorCSI/
├── ConnectorCSIShared/           # .shproj (shared project)
│   └── UI bindings, event handling
├── ConnectorETABS/               # Uses ConnectorCSIShared
├── ConnectorCSIBridge25/         # Uses ConnectorCSIShared
│   └── DefineConstants: CSIBRIDGE;CSIBRIDGE25
```

**How Shared Projects Work:**
- `.shproj` files don't compile independently
- Referenced projects include the code at compile time
- Code is compiled separately in each consumer project
- Allows conditional compilation via `#if DEFINES`
- Files appear twice in Visual Studio but are one source

### Converter Discovery & Loading

**Process:**
1. DesktopUI2 calls `ISpeckleConverter.GetServicedApplications()`
2. Returns slugs like "Revit", "ETABS", "SAP2000"
3. Core's `KitManager` loads converter DLL from:
   - `%APPDATA%\Speckle\Kits\Objects\` (Windows)
   - `~/.config/Speckle/Kits/Objects/` (macOS)
4. Connector receives converter instance via dependency injection

**Example (ConverterCSI):**
```csharp
public class ConverterCSI : ISpeckleConverter, IFinalizable
{
#if ETABS
  public static string CSIAppName = HostApplications.ETABS.Name;
#elif CSIBRIDGE
  public static string CSIAppName = HostApplications.CSiBridge.Name;
#endif

    public IEnumerable<string> GetServicedApplications()
    {
        yield return CSISlug;  // "etabs", "csibridge", etc
    }
}
```

## Build System

### Directory.Build.props (root)
**Location:** `/Directory.Build.props`

**Enforces across entire monorepo:**
- Language version: C# 11
- Code analysis: CA (Code Analyzer) rules, as errors
- Package versioning: 2.0.999-local for dev builds
- Documentation generation
- Security checks (CA5xxx rules)

**Key Properties:**
```xml
<LangVersion>11</LangVersion>
<EnableNetAnalyzers>true</EnableNetAnalyzers>
<AnalysisLevel>latest-AllEnabledByDefault</AnalysisLevel>
```

### Directory.Build.targets (root)
**Location:** `/Directory.Build.targets`

**Provides common targets:**
- **CopyToKitFolder:** Copies converter DLLs to `%APPDATA%\Speckle\Kits\Objects\`
  - Applied to all converters (net48 ones are excluded)
  - Runs as post-build event on Windows only
- **ReportBuildVersion:** Logs version info during build
- **DeepClean:** Removes all bin/obj folders
  - Invoked: `dotnet clean /p:DeepClean=true`
- **TestProject Support:** Suppresses analyzers for test projects

**Example Usage:**
```xml
<!-- In a converter's .csproj -->
<PropertyGroup>
    <CopyToKitFolder>true</CopyToKitFolder>
    <KitFolder>Objects</KitFolder>
</PropertyGroup>
```

### Objects/Directory.Build.props
**Location:** `/Objects/Directory.Build.props`

Additional settings for Objects solution:
- Strict warnings-as-errors in CI
- Exception handling for legacy code

## Deployment Patterns

### Converter Deployment (netstandard2.0 → Kit Folder)
```
Build Converter (netstandard2.0)
    ↓
Copy to %APPDATA%\Speckle\Kits\Objects\
    ↓
KitManager loads at runtime
    ↓
Connector uses via ISpeckleConverter interface
```

**Post-Build Copy Targets:**
- Windows: `xcopy /Y "$(TargetDir)*.dll" "$(AppData)\Speckle\Kits\Objects\"`
- macOS: `cp -r` to `~/.config/Speckle/Kits/Objects/`

### Connector Deployment (net48 → Application Folder)
```
Build Connector (net48)
    ↓
Copy DLLs to application plug-ins folder:
    - Revit: %APPDATA%\Autodesk\Revit\Addins\{Version}\SpeckleRevit2\
    - CSI: C:\ProgramData\Computers and Structures\{App}\Plug-Ins\
    ↓
Application loads .addin file or similar registration
    ↓
Connector initializes on app startup
```

## CSI Connector Architecture (Deep Dive)

The CSI connector is a good example of the version-specific pattern:

### Projects:
```
ConnectorCSI/
├── DriverCSharp/                # Low-level CSI API wrapper (optional)
├── DriverPluginCSharp/          # Plugin registration/initialization
├── ConnectorCSIShared/          # Shared code (.shproj)
│   ├── cPlugin.cs              # IExternalApplication entry point
│   ├── UI/ConnectorBindingsCSI.cs  # Business logic, event handlers
│   ├── StreamStateManager/      # State persistence
│   └── Util/                    # Helpers
├── ConnectorETABS/              # net48, #if ETABS
├── ConnectorETABS22/            # net48, #if ETABS22
├── ConnectorSAP2000/            # net48, #if SAP2000
├── ConnectorCSIBridge/          # net48, #if CSIBRIDGE
├── ConnectorCSIBridge25/        # net48, #if CSIBRIDGE25
└── ConnectorCSIBridge26/        # net48, #if CSIBRIDGE26

Objects/Converters/ConverterCSI/
├── ConverterCSIShared/          # Shared conversion logic (.shproj)
├── ConverterETABS/              # netstandard2.0, #if ETABS
├── ConverterETABS22/            # netstandard2.0, #if ETABS22
├── ConverterSAP2000/            # netstandard2.0
├── ConverterCSIBridge/          # netstandard2.0, #if CSIBRIDGE
├── ConverterCSIBridge25/        # netstandard2.0, #if CSIBRIDGE25
└── ConverterCSIBridge26/        # netstandard2.0, #if CSIBRIDGE26
```

### Conditional Compilation Defines:
```
ConnectorCSIBridge25.csproj:
  <DefineConstants>DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE25</DefineConstants>

ConverterCSIBridge25.csproj:
  <DefineConstants>TRACE;CSIBRIDGE25</DefineConstants>
```

### Code Organization:
```csharp
// In ConverterCSIShared/ConverterCSI.cs
public partial class ConverterCSI : ISpeckleConverter
{
#if ETABS
    public static string CSIAppName = HostApplications.ETABS.Name;
#elif ETABS22
    public static string CSIAppName = HostApplications.ETABS22.Name;
#elif CSIBRIDGE
    public static string CSIAppName = HostApplications.CSiBridge.Name;
#elif CSIBRIDGE25
    public static string CSIAppName = HostApplications.CSiBridge25.Name;
#endif
}

// In ConverterCSIShared/PartialClasses/*.cs
public partial class ConverterCSI
{
    public void ConvertBeams() { ... }
    public void ConvertColumns() { ... }
}
```

## Key Design Patterns

### 1. Converter Interface Pattern
All converters implement `ISpeckleConverter`:
```csharp
public interface ISpeckleConverter
{
    // Conversion methods
    Base ConvertToSpeckle(object value);
    object ConvertToNative(Base value);
    
    // Capability checks
    bool CanConvertToSpeckle(object value);
    bool CanConvertToNative(Base value);
    
    // Context setup
    void SetContextDocument(object doc);
    void SetContextObjects(List<ApplicationObject> objects);
    void SetPreviousContextObjects(List<ApplicationObject> objects);
    
    // Metadata
    string Name { get; }
    string Description { get; }
    IEnumerable<string> GetServicedApplications();
}
```

### 2. Partial Class Pattern for Converters
Converters are split across many files using `partial class`:
```
ConverterCSIShared/
├── ConverterCSI.cs                          # Main class definition
├── PartialClasses/Geometry/ConvertBeam.cs   # Beam conversion logic
├── PartialClasses/Geometry/ConvertColumn.cs # Column conversion logic
├── PartialClasses/Loading/ConvertLoadPattern.cs
└── ... (100+ files)
```

**Benefit:** Logical organization by domain (Geometry, Loading, Properties, etc)

### 3. Shared Project Pattern
Avoid code duplication across versions:
```
ConverterCSIShared.shproj  (shared project - no .csproj)
  ↑
  │ (included at compile time)
  ├─ ConverterETABS.csproj (with #if ETABS)
  ├─ ConverterETABS22.csproj (with #if ETABS22)
  └─ ConverterCSIBridge25.csproj (with #if CSIBRIDGE25)
```

Each project compiles the shared code separately with different `#define` values.

### 4. Post-Build Automation
Connectors and converters use MSBuild targets to:
- Copy DLLs to application plug-in folders
- Copy converters to Kit folder
- Generate installation packages
- Clean up previous builds

## Important Build Considerations

### 1. Target Frameworks
- **Core & Objects:** `netstandard2.0` (maximum compatibility)
- **DesktopUI2:** `netstandard2.0` (Avalonia requirement)
- **Connectors:** `net48` (Windows/native APIs required)
- **Converters:** `netstandard2.0` (for Kit loading)

### 2. Assembly Binding Redirects
Connectors (net48) use assembly binding redirects to handle version mismatches:
```xml
<runtime>
    <assemblyBinding>
        <bindingRedirect oldVersion="0.0.0.0-7.0.0.0" 
                        newVersion="7.0.0.0" />
    </assemblyBinding>
</runtime>
```

### 3. Private vs. Shared Dependencies
- **DesktopUI2.csproj:** References Objects with `<Private>False</Private>`
  - Prevents Objects.dll from being copied to bin
  - Allows sharing between connector and UI
- **Converters:** Auto-reference Objects (part of kit loading)

## Adding a New Connector

1. **Create connector structure:**
   ```bash
   mkdir ConnectorNewApp/ConnectorNewApp
   mkdir Objects/Converters/ConverterNewApp
   ```

2. **Create shared projects (.shproj):**
   - `ConnectorNewApp/ConnectorNewAppShared/`
   - `Objects/Converters/ConverterNewApp/ConverterNewAppShared/`

3. **Create version-specific projects:**
   ```
   ConnectorNewApp/ConnectorNewApp/ConnectorNewApp.csproj (net48)
   Objects/Converters/ConverterNewApp/ConverterNewApp.csproj (netstandard2.0)
   ```

4. **Implement ISpeckleConverter:**
   - Create `ConverterNewApp.cs` in Objects/Converters
   - Implement interface methods
   - Use partial classes for organization

5. **Create plugin entry point:**
   - Implement IExternalApplication or equivalent
   - Initialize DesktopUI2
   - Wire up event handlers

6. **Update main solution:**
   - Add projects to `All.sln`
   - Add projects to `ConnectorNewApp/ConnectorNewApp.sln`

7. **Configure build:**
   - Set post-build targets to copy DLLs
   - Add to CI/CD pipeline

## Adding a New Version of Existing Connector

Example: Adding Revit 2026 support

1. **Create new connector project:**
   - Copy `ConnectorRevit2025` → `ConnectorRevit2026`
   - Update .csproj: `RevitVersion>2026</RevitVersion>`
   - Update: `DefineConstants>REVIT2026</DefineConstants>`

2. **Create new resources project:**
   - Copy `RevitSharedResources2025` → `RevitSharedResources2026`
   - Update NuGet reference: `ModPlus.Revit.API.2026`

3. **Create new converter project:**
   - Copy `ConverterRevit2025` → `ConverterRevit2026`
   - Update references to `RevitSharedResources2026`

4. **Update SharedProject references:**
   - Both connector and converter reference `ConnectorRevitShared`/`ConverterRevitShared`
   - These shared projects use conditional compilation

5. **Register in solutions:**
   - Add to `ConnectorRevit.sln`
   - Add to `Objects.sln`
   - Add to `All.sln`

## Troubleshooting Common Issues

### Issue: "Could not load file or assembly X, Version=Y"
**Cause:** Assembly version mismatch between connector and converter
**Solution:** 
- Check binding redirects in App.config
- Ensure converters match connector's expected versions
- Use `KitManager` to load converters (handles redirects)

### Issue: "Converter DLL not found in Kit folder"
**Cause:** Post-build copy didn't execute
**Solution:**
- Verify `<CopyToKitFolder>true</CopyToKitFolder>` in converter .csproj
- Check `IsDesktopBuild` property (set false in CI)
- Run: `dotnet clean /p:DeepClean=true && dotnet build`

### Issue: Plugin doesn't appear in application
**Cause:** Registration not found or assembly not loadable
**Solution:**
- Check plugin registration (.addin files, manifest, etc)
- Verify DLLs copied to correct plug-in folder
- Ensure net48 build succeeded
- Check Application logs for loading errors

### Issue: DesktopUI2 crashes on load
**Cause:** Avalonia/WPF/HWND initialization issues
**Solution:**
- Check `BuildAvaloniaApp()` configuration in cPlugin.cs
- Verify `AllowEglInitialization` flag set correctly for app
- Check GPU rendering settings (`MaxGpuResourceSizeBytes`)
- Enable debug logging in Serilog

## Dependency Map

```
Connector (net48, native APIs)
    ↓
DesktopUI2 (netstandard2.0, Avalonia UI)
    ↓
Objects (netstandard2.0, domain models)
    ↓
Core (netstandard2.0, Speckle SDK)
    ↓
Transports, GraphQL Client, APIs

Converter (netstandard2.0, loaded at runtime)
    ↓
Objects (netstandard2.0)
    ↓
Core (netstandard2.0)
```

**Important:** 
- Converters should NOT reference Connector projects
- Connectors can reference Converter projects
- DesktopUI2 references Core but NOT specific converters
- All dependencies are resolved at runtime via KitManager

## Testing Strategy

- **Unit Tests:** `/Tests/` directories in Core, Objects
- **Integration Tests:** Speckle.Core.Tests.Integration
- **Converter Tests:** Objects/Converters/*/Tests/
- **End-to-End:** Manual testing with each application version

## Key Files to Know

| File | Purpose |
|------|---------|
| `/Core/Core/Kits/ISpeckleConverter.cs` | Converter interface definition |
| `/Core/Core/Kits/KitManager.cs` | Dynamic converter loading |
| `/Core/Core/Models/Base.cs` | Root Speckle domain object |
| `/Objects/Objects/Objects.csproj` | Main domain model assembly |
| `/DesktopUI2/DesktopUI2/App.xaml` | Avalonia app entry point |
| `/Directory.Build.props` | Build configuration (all projects) |
| `/Directory.Build.targets` | Build targets (all projects) |
| `/All.sln` | Master solution (all projects) |

## Next Steps

When working on this codebase:
1. Understand which layer you're modifying (Core → Objects → UI → Connector)
2. Check if changes affect multiple versions (use shared projects)
3. Run `dotnet clean /p:DeepClean=true` before major builds
4. Test in actual application, not just unit tests
5. Update documentation when adding new connectors/versions
6. Follow existing patterns for consistency

