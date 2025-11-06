# CSI Connector Suite Comprehensive Documentation

**Status**: Active Development  
**Last Updated**: November 6, 2025  
**Repository**: speckle-sharp-main  
**Branch**: feature/csibridge-connector  

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [CSI Suite Overview](#csi-suite-overview)
3. [Project Organization](#project-organization)
4. [CSI API Integration](#csi-api-integration)
5. [Converter Architecture](#converter-architecture)
6. [Build and Deployment](#build-and-deployment)
7. [Known Issues and Solutions](#known-issues-and-solutions)
8. [Recent Development](#recent-development)
9. [CSI-Specific Patterns](#csi-specific-patterns)
10. [Version Management Strategy](#version-management-strategy)
11. [Troubleshooting Guide](#troubleshooting-guide)

---

## Executive Summary

The CSI connector suite provides integration between Speckle and Computers and Structures, Inc. (CSI) applications. The suite has been significantly modernized through the ETABS22 pattern implementation, with recent additions of CSIBridge25 and CSIBridge26 connectors.

### Key Statistics

- **Products Supported**: ETABS, SAP2000, CSIBridge (v25 & v26), SAFE
- **Version-Specific Connectors**: 6 active (ETABS, ETABS22, SAP2000, SAP2000-26, CSIBridge25, CSIBridge26)
- **Legacy Connectors**: 3 (ConnectorCSI, ConnectorCSIBridge, ConnectorSAP2000)
- **Shared Code Projects**: 2 (ConnectorCSIShared, ConverterCSIShared)
- **Framework Targets**: .NET 4.8, netstandard2.0

### Major Recent Improvements

- Product-specific API DLL references (CSiBridge1.dll instead of generic CSiAPIv1)
- Type identity preservation through direct project references
- Large model support (>25MB) via Collection with DetachProperty
- Automated DLL deployment to ProgramData Plug-Ins folders
- Comprehensive diagnostic logging for troubleshooting
- Windows stability improvements with CSIBridge-specific rendering configuration

---

## CSI Suite Overview

### Products and Versions

#### ETABS
- **Current Version**: ETABS 22
- **Connector Projects**: 
  - `ConnectorETABS` (legacy, uses CSiAPIv1 NuGet)
  - `ConnectorETABS22` (ETABS22 pattern, direct ETABSv1.dll)
- **Key DLL**: `ETABSv1.dll` (product-specific)
- **Installation Path**: `C:\Program Files\Computers and Structures\ETABS 22\`
- **Deployment Path**: `C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\`

#### SAP2000
- **Current Version**: SAP2000 v25+ with v26 support
- **Connector Projects**:
  - `ConnectorSAP2000` (legacy)
  - `ConnectorSAP2000-26` (v26 support)
- **Key DLL**: CSiAPIv1.dll (generic)
- **Installation Path**: `C:\Program Files\Computers and Structures\SAP2000 <version>\`
- **Deployment Path**: AppData/LocalAppData (legacy)

#### CSIBridge
- **Current Versions**: CSIBridge 25, CSIBridge 26
- **Connector Projects**:
  - `ConnectorCSIBridge` (legacy, minimal config)
  - `ConnectorCSIBridge25` (ETABS22 pattern)
  - `ConnectorCSIBridge26` (ETABS22 pattern)
- **Key DLLs**: `CSiBridge1.dll` (product-specific), CSiAPIv1.dll (generic)
- **Installation Paths**:
  - v25: `C:\Program Files\Computers and Structures\CSiBridge 25\`
  - v26: `C:\Program Files\Computers and Structures\CSiBridge 26\`
- **Deployment Paths**:
  - v25: `C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\`
  - v26: `C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\`
- **Key Features**:
  - .NET 8 support (v25.1+, full support in v26)
  - Enhanced plugin development capabilities
  - Improved multi-instance connection control
  - API differences between v25 (1,007 KB) and v26 (1,021 KB - 14 KB larger)

#### SAFE
- **Status**: Supported via legacy pattern
- **Connector Project**: `ConnectorSAFE`
- **Key DLL**: CSiAPIv1.dll (generic)
- **Current Pattern**: Legacy (CSiAPIv1 NuGet)

---

## Project Organization

### Directory Structure

```
ConnectorCSI/
├── ConnectorCSIShared/               (Shared connector code - projitems)
│   ├── cPlugin.cs                    (Main plugin entry point)
│   ├── StreamStateManager/           (Stream state management)
│   └── UI/
│       ├── ConnectorBindingsCSI.cs   (Main connector bindings)
│       ├── ConnectorBindingsCSI.Send.cs
│       ├── ConnectorBindingsCSI.Recieve.cs
│       └── ConnectorBindingsCSI.Selection.cs
│
├── Legacy Products
│   ├── ConnectorETABS/               (Legacy ETABS connector)
│   ├── ConnectorSAP2000/             (Legacy SAP2000 connector)
│   ├── ConnectorCSIBridge/           (Legacy CSIBridge connector)
│   └── ConnectorSAFE/                (Legacy SAFE connector)
│
├── ETABS22 Pattern (Modern)
│   ├── ConnectorETABS22/             (Direct ETABSv1.dll reference)
│   └── (auto-deploys to ProgramData Plug-Ins)
│
├── CSIBridge Modern Pattern
│   ├── ConnectorCSIBridge25/         (Direct CSiBridge1.dll v25)
│   ├── ConnectorCSIBridge26/         (Direct CSiBridge1.dll v26)
│   └── (auto-deploy to ProgramData Plug-Ins)
│
├── Driver Projects
│   ├── DriverCSharp/                 (Standalone CSI app driver)
│   └── DriverPluginCSharp/           (Plugin driver wrapper)
│
└── ConnectorCSI.sln                  (Master solution)

Objects/Converters/ConverterCSI/
├── ConverterCSIShared/               (Shared converter code - projitems)
│   ├── ConverterCSI.cs               (Main converter logic)
│   ├── AnalysisResultUtils.cs        (Results conversion)
│   ├── Constants.cs                  (CSI product constants)
│   ├── PartialClasses/
│   │   ├── Geometry/                 (Frame, Area, Point, Line converters)
│   │   ├── Loading/                  (Load pattern, case, combination converters)
│   │   ├── Properties/               (Material, section, assignment converters)
│   │   ├── Analysis/                 (Analysis case, result converters)
│   │   └── Results/                  (Result extraction, processing)
│   └── Extensions/                   (Curve, Arc, Polycurve utilities)
│
├── Legacy Converters
│   ├── ConverterETABS/
│   ├── ConverterSAP2000/
│   ├── ConverterCSIBridge/
│   └── ConverterSAFE/
│
├── Modern Converters (ETABS22 Pattern)
│   ├── ConverterETABS22/
│   ├── ConverterCSIBridge25/
│   └── ConverterCSIBridge26/
│
└── PolygonMesher/                    (Mesh generation utility)
```

### Shared Code vs Version-Specific Code

#### ConnectorCSIShared (Shared Connector Code)
- **Type**: MSBuild Shared Project (.shproj)
- **GUID**: 61374cd0-e774-4dcd-bfab-6356b0931283
- **Imported By**: All connector projects
- **Key Files**:
  - `cPlugin.cs` - Plugin entry point with Avalonia initialization
  - `ConnectorBindingsCSI*.cs` - Send, Receive, Selection, Settings operations
  - `StreamStateManager.cs` - Manages connector state
  - `ResultUtils.cs` - Result processing utilities

**Conditional Compilation Directives**:
```csharp
#if ETABS
  // Legacy ETABS behavior
#elif ETABS22
  // Modern ETABS22 pattern
#elif CSIBRIDGE
  // CSIBridge behavior (v25 & v26)
#elif CSIBRIDGE25
  // CSIBridge v25 specific
#elif CSIBRIDGE26
  // CSIBridge v26 specific
#elif SAP2000
  // SAP2000 behavior
#elif SAFE
  // SAFE behavior
#endif
```

#### ConverterCSIShared (Shared Converter Code)
- **Type**: MSBuild Shared Project (.shproj)
- **GUID**: 5BBDE14E-50F8-4D6E-8E35-747667AD4A09
- **Imported By**: All converter projects
- **Key Files**:
  - `ConverterCSI.cs` - Main converter class with product detection
  - `PartialClasses/Geometry/*.cs` - Geometry element conversion
  - `PartialClasses/Properties/*.cs` - Property and material conversion
  - `PartialClasses/Loading/*.cs` - Load pattern/case conversion
  - `PartialClasses/Analysis/*.cs` - Analysis setup conversion
  - `PartialClasses/Results/*.cs` - Result extraction and processing
  - `Extensions/` - Helper utilities for curves, lines, arcs

---

## CSI API Integration

### API References Pattern

#### Legacy Pattern (Pre-ETABS22)
```xml
<PackageReference Include="CSiAPIv1" Version="1.0.0" />
```

**Issues**:
- Generic API package (version ambiguous)
- Dynamic assembly loading at runtime
- Type identity problems (assembly mismatch)
- AppData installation (unprofessional)

**Products Using This**:
- ConnectorETABS
- ConnectorSAP2000
- ConnectorCSIBridge (original)
- ConnectorSAFE

#### ETABS22 Pattern (Modern)
```xml
<Reference Include="ETABSv1">
  <HintPath>C:\Program Files\Computers and Structures\ETABS 22\ETABSv1.dll</HintPath>
  <Private>False</Private>
</Reference>
```

**Advantages**:
- Direct product-specific DLL reference
- Type identity guaranteed at compile time
- DLL from actual installation directory
- Clear version tracking

**Products Using This**:
- ConnectorETABS22 (ETABSv1.dll)
- ConnectorCSIBridge25 (CSiBridge1.dll + CSiAPIv1.dll)
- ConnectorCSIBridge26 (CSiBridge1.dll + CSiAPIv1.dll)

### API Classes and Interfaces

#### Main API Classes (cSapModel)

```csharp
// From CSiAPIv1.dll or product-specific DLL
public interface cSapModel
{
  // Model access
  void GetVersion(ref string versionString, ref double versionNumber);
  cFrameObj FrameObj { get; }
  cAreaObj AreaObj { get; }
  cPointObj PointObj { get; }
  cLinkObj LinkObj { get; }
  cTendonObj TendonObj { get; }
  cCableObj CableObj { get; }
  cLoadPatterns LoadPatterns { get; }
  cLoadCases LoadCases { get; }
  cLoadCombinations LoadCombinations { get; }
  // ... and many more properties
}
```

#### Plugin Callback Interface

```csharp
// From cPlugin.cs - implements iPluginCallback
public interface iPluginCallback
{
  // Called by CSI application to notify plugin of events
  void ButtonClick(ref string arg);  // Button click from UI
  void ModelClosed();                 // Model close notification
  void ModelOpenedReadOnly();         // Read-only model open
  void ModelOpened();                 // Model open notification
  void SelectionChanged();             // Selection change notification
}
```

**Current Implementation in cPlugin.cs**:
- Tracks plugin callback state
- Initializes Avalonia window on first button click
- Manages window lifecycle
- Handles assembly preloading for dependency resolution

#### Window Integration Challenges

**CSIBridge 25 Crash Issue** (Documented in CSIBRIDGE25_CRASH_INVESTIGATION.md):

Problem:
```
Avalonia.Layout.LayoutManager.RaiseEffectiveViewportChanged()
-> NullReferenceException or InvalidOperationException
```

Root Causes:
1. Graphics rendering conflict with CSIBridge's native rendering
2. Avalonia 0.10.18 version mismatch with Material.Avalonia
3. Window initialization on different UI thread than CSIBridge
4. Invalid window metrics reported by CSIBridge

Mitigations Implemented:
```csharp
// In cPlugin.cs - CSIBridge-specific configuration
#if CSIBRIDGE
  // Disable GPU rendering to avoid conflicts with CSIBridge
  .With(new Win32PlatformOptions { 
    AllowEglInitialization = false,   // Disable EGL
    EnableMultitouch = false,         // Disable multitouch
    UseWgl = false                    // Disable WGL
  })
  // Don't use app.Run() - just show the window
  MainWindow.Show();
#else
  // ETABS: Use standard GPU rendering
  .With(new Win32PlatformOptions { 
    AllowEglInitialization = true,
    EnableMultitouch = false 
  })
  app.Run(MainWindow);
#endif
```

**Assembly Preloading** (From cPlugin.cs, lines 206-239):
```csharp
// Preload critical assemblies before XAML initialization
private static void PreloadAssemblies()
{
  // Load from AppData first to ensure plugin path takes precedence
  var pluginDir = GetPluginDirectory();
  
  var criticalAssemblies = new[] {
    "System.Numerics.Vectors.dll",
    "Avalonia.Base.dll",
    "Avalonia.Controls.dll",
    "Avalonia.Layout.dll",
    // ... all Avalonia DLLs
  };
  
  foreach (var asmName in criticalAssemblies)
  {
    try
    {
      var path = Path.Combine(pluginDir, asmName);
      if (File.Exists(path))
      {
        var bytes = File.ReadAllBytes(path);
        Assembly.Load(bytes);  // Load from bytes, not LoadFile
        SpeckleLog.Logger.Information("Loaded {Assembly}", asmName);
      }
    }
    catch (Exception ex)
    {
      SpeckleLog.Logger.Warning(ex, "Failed to preload {Assembly}", asmName);
    }
  }
}
```

---

## Converter Architecture

### Main Converter Class (ConverterCSI)

**Location**: `Objects/Converters/ConverterCSI/ConverterCSIShared/ConverterCSI.cs`

**Key Properties**:
```csharp
public class ConverterCSI : ISpeckleConverter, IFinalizable
{
  // Product identification (compiler directive based)
  public static string CSIAppName;  // e.g., "ETABS", "SAP2000", "CSiBridge"
  public static string CSISlug;     // e.g., "etabs", "sap2000", "csibridge"
  
  // Document reference
  public cSapModel Model { get; set; }  // CSI model object
  public string ProgramVersion { get; set; }  // Version string
  
  // Speckle objects
  public Model SpeckleModel { get; set; }
  public ReceiveMode ReceiveMode { get; set; }
  
  // Conversion context
  public Dictionary<string, string> ExistingObjectGuids { get; set; }
  public List<ApplicationObject> ContextObjects { get; set; }
  public List<ApplicationObject> PreviousContextObjects { get; set; }
  public Dictionary<string, string> Settings { get; set; }
}
```

**Product Detection Pattern** (lines 33-45):
```csharp
#if ETABS
  public static string CSIAppName = HostApplications.ETABS.Name;
  public static string CSISlug = HostApplications.ETABS.Slug;
#elif SAP2000
  public static string CSIAppName = HostApplications.SAP2000.Name;
  public static string CSISlug = HostApplications.SAP2000.Slug;
#elif CSIBRIDGE
  public static string CSIAppName = HostApplications.CSiBridge.Name;
  public static string CSISlug = HostApplications.CSiBridge.Slug;
#elif SAFE
  public static string CSIAppName = HostApplications.SAFE.Name;
  public static string CSISlug = HostApplications.SAFE.Slug;
#endif
```

### Partial Class Organization

#### Geometry Converters (`PartialClasses/Geometry/`)
- **ConvertFrame.cs** - Structural frames (1D elements)
  - Handles column/beam/brace conversion
  - API: `Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame, "Default")`
  - Critical: 8-parameter call (7 params fails)
  
- **ConvertArea.cs** - Floor/wall surfaces (2D elements)
  - Large file: 44.7 KB
  - Handles shell element conversion
  - Assigns properties (material, thickness)

- **ConvertPoint.cs** - Joints and control points
  - API: `Model.PointObj.AddCartesian(...)`
  - Coordinates validation and snapping

- **ConvertLine.cs** - Line geometry (may create frames)
  - Minimum length check: 0.1 units tolerance
  - Diagnostic logging for troubleshooting

- **ConvertGridLines.cs** - Reference grid system

#### Loading Converters (`PartialClasses/Loading/`)
- Load pattern definition (static, dead, live, etc.)
- Load case definition (analysis cases)
- Load combination creation
- Coordinate system handling

#### Material & Property Converters (`PartialClasses/Properties/`)
- Material assignment
- Section property creation (concrete, steel, composite)
- Element property assignment
- Cross-section parameters (height, width, thickness)

#### Analysis Converters (`PartialClasses/Analysis/`)
- Analysis case setup
- Solver configuration
- Run control parameters
- Results database initialization

#### Results Converters (`PartialClasses/Results/`)
- **AnalysisResultUtils.cs** - Result extraction
- **Element1DAnalyticalResultConverter.cs** - Frame results (forces, moments)
- **Element2DAnalyticalResultConverter.cs** - Shell results (stresses, strains)
- **DatabaseTableWrapper.cs** - Direct database table access
- Result filtering by load case, element type, component

### Converter Instantiation

#### Legacy Pattern (Dynamic Loading)
```csharp
var kit = KitManager.GetDefaultKit();
var converter = kit.LoadConverter(appName);  // Dynamic assembly loading
```

**Issues**:
- Type mismatch if kit assembly version differs
- Assembly resolved at runtime (unpredictable)
- Can pick wrong version of Objects.dll

#### Direct Instantiation Pattern (ETABS22+)
```csharp
#if ETABS22 || CSIBRIDGE26 || CSIBRIDGE25
  // Direct instantiation - no dynamic loading, type identity guaranteed
  var converter = new Objects.Converter.CSI.ConverterCSI();
#else
  // Fallback to dynamic loading for legacy patterns
  var converter = kit.LoadConverter(appName);
#endif
```

**Advantages**:
- Type identity guaranteed at compile time
- Project reference ensures correct assembly
- No runtime ambiguity
- Faster loading (no reflection)

---

## Build and Deployment

### Build Targets

#### CopyConverterToConnectorBin (ETABS22 Pattern)

**Purpose**: Copies converter DLL to connector's bin folder so both are deployed together

**Trigger**: AfterTargets="Build"

**MSBuild Configuration**:
```xml
<Target Name="CopyConverterToConnectorBin" AfterTargets="Build">
  <PropertyGroup>
    <ConverterOutputPath>..\..\Objects\Converters\ConverterCSI\ConverterCSIBridge26\bin\$(Configuration)\netstandard2.0\Objects.Converter.CSIBridge26.dll</ConverterOutputPath>
  </PropertyGroup>
  <Message Text="Copying converter from $(ConverterOutputPath) to $(TargetDir)" Importance="high" />
  <Copy 
    Condition="Exists('$(ConverterOutputPath)')" 
    SourceFiles="$(ConverterOutputPath)" 
    DestinationFolder="$(TargetDir)" 
    SkipUnchangedFiles="false" />
  <Warning 
    Condition="!Exists('$(ConverterOutputPath)')" 
    Text="Converter DLL not found. Build the converter first!" />
</Target>
```

**Key Points**:
- Must build converter BEFORE connector
- Skips unchanged files optimization disabled (forces copy)
- Provides helpful warning if converter not found

#### PostBuild (Automatic Deployment to Plug-Ins)

**Purpose**: Deploys all DLLs to CSI application's Plug-Ins folder

**Trigger**: AfterTargets="PostBuildEvent"

**Locations**:
- ETABS 22: `C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\`
- CSIBridge 25: `C:\ProgramData\Computers and Structures\CSIBridge 25\Plug-Ins\`
- CSIBridge 26: `C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\`

**Deployment Scope** (CSIBridge25 example):
```xml
<ItemGroup>
  <DllsToCopy Include="$(TargetDir)*.dll" />
  <ConfigsToCopy Include="$(TargetDir)*.config" />
  <RuntimesToCopy Include="$(TargetDir)runtimes\**\*.*" />
  <NativeLibsToCopy Include="$(TargetDir)x86\**\*.*" />
  <NativeLibsToCopy Include="$(TargetDir)x64\**\*.*" />
  <DylibsToCopy Include="$(TargetDir)*.dylib" />
</ItemGroup>
<Copy SourceFiles="@(DllsToCopy)" DestinationFolder="C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\" />
<!-- ... additional copy tasks for configs, runtimes, native libs ... -->
```

**Advantages**:
- One build step deployments to production location
- No manual DLL copying needed
- Works cross-platform (checks IsOsPlatform('Windows'))
- Includes runtime and native library deployment

### Conditional Compilation Strategy

#### Compiler Constants

| Product | Connector Constants | Converter Constants |
|---------|-------------------|-------------------|
| ETABS | `DEBUG;TRACE;ETABS` | `TRACE;ETABS` |
| ETABS 22 | `DEBUG;TRACE;ETABS;ETABS22` | `TRACE;ETABS;ETABS22` |
| SAP2000 | `DEBUG;TRACE;SAP2000` | `TRACE;SAP2000` |
| SAP2000-26 | `DEBUG;TRACE;SAP2000` | `TRACE;SAP2000` |
| CSIBridge | `DEBUG;TRACE;CSIBRIDGE` | `TRACE;CSIBRIDGE` |
| CSIBridge 25 | `DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE25` | `TRACE;CSIBRIDGE;CSIBRIDGE25` |
| CSIBridge 26 | `DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE26` | `TRACE;CSIBRIDGE;CSIBRIDGE26` |
| SAFE | `DEBUG;TRACE;SAFE` | `TRACE;SAFE` |

#### Usage Examples

**Product-Specific API Import** (cPlugin.cs):
```csharp
#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;
#endif
```

**CSIBridge-Specific Rendering** (cPlugin.cs):
```csharp
#if CSIBRIDGE
  .With(new Win32PlatformOptions { 
    AllowEglInitialization = false,
    UseWgl = false 
  })
#else
  .With(new Win32PlatformOptions { 
    AllowEglInitialization = true 
  })
#endif
```

**Direct Converter Instantiation** (ConnectorBindingsCSI.Send.cs):
```csharp
#if ETABS22 || CSIBRIDGE26 || CSIBRIDGE25
  var converter = new Objects.Converter.CSI.ConverterCSI();
#else
  var converter = kit.LoadConverter(appName);
#endif
```

### Project Reference Strategy

#### Direct References (ETABS22 Pattern)

**ConverterETABS22.csproj**:
```xml
<ItemGroup>
  <Reference Include="ETABSv1">
    <HintPath>C:\Program Files\Computers and Structures\ETABS 22\ETABSv1.dll</HintPath>
    <Private>False</Private>
  </Reference>
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\..\..\..\Core\Core\Core.csproj" />
  <ProjectReference Include="..\..\..\Objects\Objects.csproj" />
  <ProjectReference Include="..\..\StructuralUtilities\PolygonMesher\PolygonMesher.csproj" />
</ItemGroup>

<Import Project="..\ConverterCSIShared\ConverterCSIShared.projitems" Label="Shared" />
```

**Advantages**:
- No NuGet package ambiguity
- Direct from installation directory
- Compile-time version verification
- Shared code via projitems (not DLL reference)

#### Connector Project References

**ConnectorETABS22.csproj**:
```xml
<ItemGroup>
  <ProjectReference Include="..\..\Core\Core\Core.csproj" />
  <ProjectReference Include="..\..\DesktopUI2\DesktopUI2\DesktopUI2.csproj" />
  <ProjectReference Include="..\..\Objects\Objects\Objects.csproj" />
  <ProjectReference Include="..\..\Objects\Converters\ConverterCSI\ConverterETABS22\ConverterETABS22.csproj" />
  <ProjectReference Include="..\..\Objects\Converters\StructuralUtilities\PolygonMesher\PolygonMesher.csproj" />
</ItemGroup>
```

---

## Known Issues and Solutions

### Issue 1: Avalonia Version Mismatch

**Status**: RESOLVED  
**Affected Products**: CSIBridge 25  
**Root Cause**: DesktopUI2 compiled with Avalonia 0.10.18, but Plug-Ins folder contained 0.10.21

**Error**:
```
System.IO.FileLoadException: Could not load file or assembly 
'Avalonia.Base, Version=0.10.18.0'
```

**Solution Implemented**: 
- Downgraded DesktopUI2 to consistently use Avalonia 0.10.18
- Modified: `DesktopUI2/DesktopUI2/DesktopUI2.csproj` lines 264-272
- All Avalonia packages set to 0.10.18

**Prevention**:
- Keep DesktopUI2 Avalonia version in sync with Material.Avalonia dependency
- Test assembly binding in actual deployment environment (Plug-Ins folder)
- Use `Assembly.Load(bytes)` instead of `LoadFile()` for security/version flexibility

**Reference**: `/ConnectorCSI/AVALONIA_VERSION_MISMATCH_FIX.md`

---

### Issue 2: Type Identity and Serialization

**Status**: RESOLVED (ETABS22 pattern)  
**Affected Products**: All CSI products using legacy pattern  
**Root Cause**: Assembly loading inconsistency

**Error**:
```
Model is Base? False
"Unsupported values in the commit object"
```

**Root Cause**:
```csharp
// Connector referenced AppData Objects.dll (old version)
// Converter built with local Objects.dll
// Result: Type mismatch, serialization rejected
```

**Solution Implemented**:

1. **Direct Project References** (instead of NuGet):
```xml
<ProjectReference Include="..\..\Objects\Objects\Objects.csproj" />
<ProjectReference Include="..\..\Core\Core\Core.csproj" />
```

2. **Direct Converter Instantiation** (instead of dynamic loading):
```csharp
var converter = new Objects.Converter.CSI.ConverterCSI();
```

3. **Diagnostic Logging**:
```csharp
SpeckleLog.Logger.Information("🔍 Model is Base? {IsBase}", modelObj is Speckle.Core.Models.Base);
SpeckleLog.Logger.Information("🔍 Assembly: {Assembly}", modelType.Assembly.Location);
```

**Applied To**:
- ConnectorETABS22
- ConnectorCSIBridge25
- ConnectorCSIBridge26

**Reference**: `/ConnectorCSI/CSIBRIDGE_CONNECTOR_PLAN.md` (Section 1)

---

### Issue 3: Large Models (25MB+ Limit)

**Status**: RESOLVED  
**Affected Products**: All CSI products  
**Root Cause**: Server-side 25MB object size limit for single commits

**Error**:
```
Large model objects rejected by Speckle server
Max commit object size: 25MB
```

**Solution Implemented**: Collection with DetachProperty

**Before** (fails for large models):
```csharp
var commitObj = new Base();
commitObj["elements"] = objects;  // All objects inline
```

**After** (supports unlimited sizes):
```csharp
var commitObj = new Collection("CSI Model", "CSI");
commitObj.elements = objects.Cast<Base>().ToList();
// Collection.elements has [DetachProperty] attribute
// Each element stored separately in database
// Commit object contains only IDs, not full objects
```

**Benefits**:
- Models of any size can be sent
- Server stores elements separately (faster access)
- Better database structure
- Commit object always under 25MB limit

**Implementation Location**: `ConnectorBindingsCSI.Send.cs`, lines 253-281

**Applied To**: All ETABS22 pattern connectors

**Reference**: `/ConnectorCSI/CSIBRIDGE_CONNECTOR_PLAN.md` (Section 2)

---

### Issue 4: Line Geometry Conversion Failure

**Status**: RESOLVED  
**Affected Products**: All CSI products  
**Root Cause**: Missing required parameter in ETABS API call

**Error**:
```
LineToNative failed with error code 1
Frames not created
```

**Root Cause**: Missing section property parameter
```csharp
// 7 parameters - FAILS
Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame);

// 8 parameters - WORKS
Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame, "Default");
```

**Solution Implemented**:

Location: `ConverterCSIShared/PartialClasses/Geometry/ConvertLine.cs`

```csharp
public void LineToNative(Curve curve)
{
  // ... coordinate validation ...
  
  // Check minimum length (0.1 units tolerance)
  double length = Math.Sqrt(
    Math.Pow(x2-x1, 2) + Math.Pow(y2-y1, 2) + Math.Pow(z2-z1, 2));
  if (length < 0.1)
  {
    SpeckleLog.Logger.Warning("Line length {Length} below minimum", length);
    return;
  }
  
  // Create frame with section property
  int ret = Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame, "Default");
  
  if (ret != 0)
  {
    SpeckleLog.Logger.Error("Frame creation failed with error code {Code}", ret);
  }
}
```

**Additional Improvements**:
- Comprehensive diagnostic logging
- Coordinate validation
- Minimum frame length check
- Error code reporting

**Applied To**: All converters (shared code)

**Reference**: `/ConnectorCSI/CSIBRIDGE_CONNECTOR_PLAN.md` (Section 3)

---

### Issue 5: CSIBridge 25 Window Crash

**Status**: IN PROGRESS  
**Description**: Plugin window crashes during Avalonia layout calculation  
**Error Location**: `Avalonia.Layout.LayoutManager.RaiseEffectiveViewportChanged()`  
**Affected Product**: CSIBridge 25 only (ETABS works fine with same code)

**Investigation Details**:

**What Works**:
- ETABS with same ConnectorCSIShared code
- SAP2000 with same ConnectorCSIShared code
- Assembly loading (no version mismatch errors)

**What Fails**:
- CSIBridge 25 crashes at `MainWindow.Show()`
- Only during window layout calculation
- Not during Avalonia initialization
- Not during DLL loading

**Hypothesis**:
1. CSIBridge 25's host environment reports invalid window metrics
2. Graphics rendering conflict (CSIBridge's native code vs Avalonia)
3. UI thread handling difference between products

**Mitigations Implemented**:

1. **CSIBridge-Specific Rendering** (cPlugin.cs, lines 38-44):
```csharp
#if CSIBRIDGE
  .With(new Win32PlatformOptions { 
    AllowEglInitialization = false,  // Disable EGL
    UseWgl = false                    // Disable WGL  
  })
#else
  .With(new Win32PlatformOptions { 
    AllowEglInitialization = true
  })
#endif
```

2. **Skip app.Run() for CSIBridge** (cPlugin.cs, lines 134-151):
```csharp
#if CSIBRIDGE
  MainWindow.Show();  // Don't use app.Run()
#else
  app.Run(MainWindow);
#endif
```

3. **Comprehensive Diagnostic Logging**:
   - Assembly versions
   - Window properties before crash
   - Event handler status
   - Exception details (type, inner exception)

**Next Steps**:
1. Collect logs from CSIBridge 25 crash
2. Analyze window metrics and exception details
3. Try alternative approaches:
   - Force software rendering
   - Delay window show
   - Set explicit window properties
   - Use custom window initialization

**Reference**: `/ConnectorCSI/CSIBRIDGE25_CRASH_INVESTIGATION.md`

---

## Recent Development

### CSIBridge 26 Connector Implementation (COMPLETE)

**Date**: November 5-6, 2025  
**Status**: Ready for Build & Test  
**Branch**: feature/csibridge-connector  

**What Was Implemented**:

1. **ConnectorCSIBridge26 Project**
   - Target: net48
   - Platform: x64
   - Constants: `DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE26`
   - Direct reference: `CSiBridge1.dll` from CSiBridge 26 installation
   - Direct project references (Core, Objects, DesktopUI2, Converter, PolygonMesher)
   - Auto-launch CSiBridge 26 for debugging
   - Build targets for converter copying and DLL deployment
   - Deployment path: `C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\`

2. **ConverterCSIBridge26 Project**
   - Target: netstandard2.0
   - Assembly: Objects.Converter.CSIBridge26
   - Constants: `TRACE;CSIBRIDGE;CSIBRIDGE26`
   - Direct references: CSiBridge1.dll, CSiAPIv1.dll
   - Project references: Core, Objects, PolygonMesher
   - Shared converter code: ConverterCSIShared.projitems

3. **Updated Shared Code**
   - ConnectorBindingsCSI.Send.cs: Added `CSIBRIDGE26` to direct instantiation check
   - ConnectorBindingsCSI.Recieve.cs: Added `CSIBRIDGE26` to KitManager initialization
   - Logging messages for CSIBridge26 path

4. **Solution Integration**
   - Added to ConnectorCSI.sln
   - Organized in "CSIVersionProjects" folder
   - Converter in "ConverterCSI" folder
   - Full Debug/Release configuration

**Key Features**:
- Product-specific API (CSiBridge1.dll)
- Type identity preservation
- Large model support (Collection with detach)
- 8-parameter line conversion fix
- Automatic Plug-Ins folder deployment
- Comprehensive logging

**Build Instructions**:
```bash
# Build converter first
dotnet build Objects/Converters/ConverterCSI/ConverterCSIBridge26/ConverterCSIBridge26.csproj --configuration Release

# Build connector (will copy converter and deploy)
dotnet build ConnectorCSI/ConnectorCSIBridge26/ConnectorCSIBridge26.csproj --configuration Release
```

**Reference**: `/ConnectorCSI/CSIBRIDGE26_IMPLEMENTATION_COMPLETE.md`

---

### CSIBridge 25 Connector Implementation (IN PROGRESS)

**Date**: November 5-6, 2025  
**Status**: Created, Avalonia crash investigation ongoing  
**Branch**: feature/csibridge-connector  

**What Was Implemented**:

1. **ConnectorCSIBridge25 Project**
   - Similar structure to CSIBridge26
   - Constants: `DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE25`
   - References CSiBridge 25 installation
   - Deployment to CSiBridge 25 Plug-Ins folder
   - Auto-launch CSiBridge 25 for debugging

2. **ConverterCSIBridge25 Project**
   - Similar to ConverterCSIBridge26
   - Constants: `TRACE;CSIBRIDGE;CSIBRIDGE25`
   - References CSiBridge 25 DLLs

3. **Window Crash Issue**
   - Crashes at MainWindow.Show() during layout calculation
   - CSIBridge-specific rendering disabled
   - Comprehensive logging added
   - Awaiting real-world testing and logs

**Status**:
- Code complete
- Waiting for CSIBridge 25 environment testing
- Need crash logs to diagnose layout issue
- May require alternative window initialization strategy

---

### API Analysis (COMPLETED)

**Date**: November 5, 2025  
**Key Finding**: CSiBridge has product-specific API DLL

**Discovery**:
```
CSiBridge 25: CSiBridge1.dll (1,007 KB)
CSiBridge 26: CSiBridge1.dll (1,021 KB - 14 KB larger, new features)
```

**Comparison with ETABS22**:
```
ETABS 22: ETABSv1.dll (328 KB) + CSiAPIv1.dll (1.2 MB)
CSiBridge: CSiBridge1.dll (1000+ KB) + CSiAPIv1.dll (1.2 MB)
```

**Recommendation**:
Use full ETABS22 pattern with product-specific DLLs instead of generic CSiAPIv1 NuGet package.

**Advantages**:
- Type identity guaranteed
- Version-specific features accessible
- Professional Plug-Ins installation
- Future-proof for API updates

**Reference**: `/ConnectorCSI/API_FINDINGS_CSIBRIDGE.md`

---

## CSI-Specific Patterns

### Pattern 1: Shared Code via MSBuild Projitems

**Why This Pattern**:
- Single source of truth for shared logic
- Conditional compilation by product
- Avoids assembly versioning issues
- Clear separation of concerns

**Example** (Geometry conversion):
```
ConverterCSIShared/PartialClasses/Geometry/
├── ConvertFrame.cs     (All products use this)
├── ConvertArea.cs      (All products use this)
├── ConvertLine.cs      (All products use this)
└── ConvertPoint.cs     (All products use this)
```

All converters import this via:
```xml
<Import Project="..\ConverterCSIShared\ConverterCSIShared.projitems" Label="Shared" />
```

**Conditional Behavior Inside Shared Code**:
```csharp
public void CreateFrame(...)
{
  // Common logic
  double length = CalculateLength(...);
  
  // Product-specific parameters
  #if ETABS22
    int ret = Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame, "Default");
  #elif CSIBRIDGE26
    // Same API call works for CSIBridge
    int ret = Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame, "Default");
  #else
    // Legacy products might have different signature
  #endif
}
```

### Pattern 2: Product Detection via Compiler Constants

**Usage**:
- Determines which DLL to reference
- Controls API imports
- Selects conversion strategies
- Configures GUI behavior

**Example** (API selection):
```csharp
// In converter's .cs file
#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;
#endif
```

**Example** (Product identification):
```csharp
// In ConverterCSI.cs
#if ETABS
  public static string CSIAppName = HostApplications.ETABS.Name;
#elif CSIBRIDGE
  public static string CSIAppName = HostApplications.CSiBridge.Name;
#elif SAP2000
  public static string CSIAppName = HostApplications.SAP2000.Name;
#endif
```

### Pattern 3: Direct Instantiation for Type Safety

**Why This Pattern**:
- Guarantees type identity at compile time
- Eliminates assembly loading ambiguity
- Faster than reflection-based loading
- Clearer code intent

**Example**:
```csharp
// Compile time: type is Object.Converter.CSI.ConverterCSI
// No possibility of loading wrong version
var converter = new Objects.Converter.CSI.ConverterCSI();

// vs. dynamic loading:
// Runtime: might load wrong Objects.dll version
var converter = kit.LoadConverter(appName);  // Risky
```

### Pattern 4: Collection for Large Objects

**Why This Pattern**:
- Speckle server limits single objects to 25MB
- Collection.elements uses [DetachProperty] attribute
- Each element stored separately, commit has only IDs
- Allows unlimited model sizes

**Example**:
```csharp
// Old pattern - fails for large models
var commit = new Base();
commit["elements"] = objects;  // ALL inline - hits 25MB limit

// New pattern - works for any size
var commit = new Collection("CSI Model", "CSI");
commit.elements = objects.Cast<Base>().ToList();  // Detached - unlimited
```

**How Detachment Works**:
```
Commit Object (< 25MB) = {
  "@Model": { id: "abc123" },
  "elements": [
    { "referencedId": "elem1" },
    { "referencedId": "elem2" },
    ...
  ]
}

Database = {
  elem1: { /* full element data */ },
  elem2: { /* full element data */ },
  ...
}
```

### Pattern 5: Comprehensive Diagnostic Logging

**Strategy**:
- Log at every critical step
- Include assembly/type information
- Report version mismatches
- Help users diagnose issues

**Example** (Send operation):
```csharp
SpeckleLog.Logger.Information("✅ Using direct converter reference for CSiBridge26");
SpeckleLog.Logger.Information("✅ Created ConverterCSI instance");
SpeckleLog.Logger.Information("🔍 Converter type: {Type}", converter.GetType().FullName);
SpeckleLog.Logger.Information("🔍 Converter assembly: {Path}", converter.GetType().Assembly.Location);
SpeckleLog.Logger.Information("🔍 Model is Base? {IsBase}", modelObj is Speckle.Core.Models.Base);
SpeckleLog.Logger.Information("🔍 Model assembly: {Assembly}", modelType.Assembly.Location);
SpeckleLog.Logger.Information("🔍 Model assembly version: {Version}", modelType.Assembly.GetName().Version);
```

**Logs Location**: 
```
C:\Users\<user>\AppData\Roaming\Speckle\Logs\Coreunknown\SpeckleCoreLog[DATE].txt
```

---

## Version Management Strategy

### Version-Specific Connector Strategy

**Current Approach**: Separate connectors per CSI version

**Products Following This**:
- ETABS vs ETABS22
- CSIBridge25 vs CSIBridge26

**Advantages**:
- Version-specific optimizations
- Clear dependency tracking
- Separate deployment paths
- No version detection logic needed
- Easier to test and debug

**Disadvantages**:
- More projects to maintain
- Duplicate code (mitigated by shared projects)
- More build artifacts

### API Version Evolution

**CSiBridge Example**:
```
CSiBridge 25 (July 8, 2024)
  ├─ CSiBridge1.dll: 1,007 KB
  ├─ CSiAPIv1.dll: 1,168,392 bytes
  └─ Design codes: AS 5100.5, CAN/CSA-S6-19, Eurocode

CSiBridge 26 (June 27, 2024 - newer API)
  ├─ CSiBridge1.dll: 1,021 KB (+14 KB)
  ├─ CSiAPIv1.dll: 1,187,336 bytes
  ├─ .NET 8 support
  ├─ Design codes: + IRC 112-2020, AASHTO LRFD 2024
  ├─ Features: Voided slab, unequal spirals, crowned decks
  └─ Improvements: Large displacement, multi-step moving loads
```

**Version Detection Strategy**:
- Runtime: `Model.GetVersion(ref versionString, ref version)`
- Build-time: Compiler constant defines product version
- No runtime version checking (explicit version in project name)

### Building for Multiple Versions

**Scenario**: Need to build for both CSIBridge 25 and 26

**Approach**:
```bash
# Build for CSIBridge 25
dotnet build ConnectorCSI/ConnectorCSIBridge25/ConnectorCSIBridge25.csproj --configuration Release
# Output: C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\

# Build for CSIBridge 26
dotnet build ConnectorCSI/ConnectorCSIBridge26/ConnectorCSIBridge26.csproj --configuration Release
# Output: C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\

# Both can coexist - each product uses its own Plug-Ins folder
```

**Maintenance**:
- Shared code: ConverterCSIShared, ConnectorCSIShared
- Version-specific: Project references, deployment paths, compiler constants
- Updates to shared code automatically benefit all versions

---

## Troubleshooting Guide

### Build Issues

#### Issue: "Converter DLL not found"
**Cause**: Converter project not built before connector  
**Solution**:
```bash
# Build converter first
dotnet build Objects/Converters/ConverterCSI/ConverterCSIBridge26/ConverterCSIBridge26.csproj --configuration Release

# Then build connector
dotnet build ConnectorCSI/ConnectorCSIBridge26/ConnectorCSIBridge26.csproj --configuration Release
```

#### Issue: "CSiBridge1.dll not found at C:\Program Files..."
**Cause**: CSIBridge not installed in expected location  
**Solution**:
1. Check installation path: `dir "C:\Program Files\Computers and Structures\"`
2. Adjust HintPath in .csproj if installed elsewhere
3. Or install CSIBridge in standard location

#### Issue: Compiler constants not defined
**Cause**: .csproj missing DefineConstants  
**Solution**: Add to PropertyGroup:
```xml
<PropertyGroup>
  <DefineConstants>DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE26</DefineConstants>
</PropertyGroup>
```

### Runtime Issues

#### Issue: "FileLoadException - Avalonia.Base Version=0.10.18"
**Cause**: Avalonia version mismatch  
**Solution**:
1. Check DesktopUI2.csproj Avalonia version
2. Ensure all Avalonia packages use same version (0.10.18)
3. Rebuild DesktopUI2
4. Clear Plug-Ins folder
5. Rebuild and redeploy connector

#### Issue: "Model is Base? False"
**Cause**: Assembly version mismatch  
**Solution**:
1. Ensure using direct project references (not NuGet)
2. Check that Objects.csproj is referenced
3. Use direct converter instantiation (not KitManager.LoadConverter)
4. Check logs for assembly location mismatches

#### Issue: "Zero objects selected; send stopped"
**Cause**: No objects selected in model  
**Solution**:
1. Select objects in CSI model before sending
2. Check selection filter settings
3. Review "Selection" tab in Speckle connector UI

#### Issue: Plugin window crashes on show
**Cause**: CSIBridge 25 layout calculation issue  
**Solution**:
1. Check CSiBridge25_CRASH_INVESTIGATION.md for latest updates
2. Collect crash logs from AppData/Roaming/Speckle/Logs
3. Try alternative window initialization strategy
4. May need CSIBridge 25-specific rendering workaround

### Deployment Issues

#### Issue: DLLs not deployed to Plug-Ins folder
**Cause**: PostBuild target not executing  
**Solution**:
1. Check PostBuild target in .csproj
2. Verify Plug-Ins folder exists and is writable
3. Build from Visual Studio (not just dotnet CLI) to ensure post-build runs
4. Check build output for copy operations

#### Issue: DLLs deployed but plugin doesn't load
**Cause**: Missing dependencies or assembly binding issues  
**Solution**:
1. Check all dependencies are in Plug-Ins folder
2. Verify no version conflicts
3. Check CSI application log files
4. Look for FileLoadException in event viewer
5. Try preloading assemblies (see cPlugin.cs assembly preloading)

### Testing Checklist

**Before Release**:
- [ ] Builds without warnings
- [ ] Converter DLL deployed with connector
- [ ] All DLLs in Plug-Ins folder
- [ ] CSI application loads plugin
- [ ] Plugin window shows
- [ ] Can send small model (<1MB)
- [ ] Can send medium model (1-25MB)
- [ ] Can send large model (>25MB) - uses Collection
- [ ] Can receive model
- [ ] No type identity warnings in logs
- [ ] No assembly mismatch errors

---

## References and Links

### Documentation Files
- `CSIBRIDGE_CONNECTOR_PLAN.md` - Complete migration strategy
- `API_FINDINGS_CSIBRIDGE.md` - API analysis and recommendations
- `AVALONIA_VERSION_MISMATCH_FIX.md` - Avalonia setup details
- `CSIBRIDGE25_CRASH_INVESTIGATION.md` - Window crash diagnosis
- `CSIBRIDGE26_IMPLEMENTATION_COMPLETE.md` - Implementation summary

### Key Source Files

**Connector Shared Code**:
- `/ConnectorCSI/ConnectorCSIShared/cPlugin.cs` - Plugin entry, Avalonia setup
- `/ConnectorCSI/ConnectorCSIShared/UI/ConnectorBindingsCSI.Send.cs` - Send operation
- `/ConnectorCSI/ConnectorCSIShared/UI/ConnectorBindingsCSI.Recieve.cs` - Receive operation

**Converter Shared Code**:
- `/Objects/Converters/ConverterCSI/ConverterCSIShared/ConverterCSI.cs` - Main converter
- `/Objects/Converters/ConverterCSI/ConverterCSIShared/PartialClasses/Geometry/ConvertFrame.cs` - Frame conversion
- `/Objects/Converters/ConverterCSI/ConverterCSIShared/PartialClasses/Geometry/ConvertLine.cs` - Line/frame conversion

**Version-Specific Implementations**:
- `/ConnectorCSI/ConnectorETABS22/` - ETABS22 reference implementation
- `/ConnectorCSI/ConnectorCSIBridge26/` - CSIBridge26 implementation
- `/ConnectorCSI/ConnectorCSIBridge25/` - CSIBridge25 implementation (crash investigation)

### Related Projects
- `DesktopUI2` - UI framework (Avalonia-based)
- `Core` - Speckle core functionality
- `Objects` - Speckle object definitions
- `PolygonMesher` - Mesh generation utilities

---

## Appendix: Compiler Constant Reference

### All Defined Constants

```
DEBUG (Debug configuration only)
TRACE (All configurations)
ETABS (ETABS products)
ETABS22 (ETABS 22 specifically)
SAP2000 (SAP2000 products)
CSIBRIDGE (CSIBridge products - v25 and v26)
CSIBRIDGE25 (CSIBridge 25 specifically)
CSIBRIDGE26 (CSIBridge 26 specifically)
SAFE (SAFE product)
```

### Typical Project Configurations

**ConnectorETABS22.csproj**:
```
Debug: DEBUG;TRACE;ETABS;ETABS22
Release: TRACE;ETABS;ETABS22
```

**ConnectorCSIBridge26.csproj**:
```
Debug: DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE26
Release: TRACE;CSIBRIDGE;CSIBRIDGE26
```

**ConverterCSIBridge26.csproj**:
```
Debug: TRACE;CSIBRIDGE;CSIBRIDGE26
Release: TRACE;CSIBRIDGE;CSIBRIDGE26
```

---

## Document Information

**Created**: November 6, 2025  
**Status**: ACTIVE - Reflects current development state  
**Last Updated**: November 6, 2025 08:59 UTC  
**Branch**: feature/csibridge-connector  
**Related Commits**:
- 80b626d - Add CSIBridge-specific fixes and diagnostic logging
- aea91c4 - Fix Avalonia version mismatch for CSIBridge 25
- a69aa5e - Add comprehensive logging to DriverCSharp
- f14c777 - Add CSIBridge 25/26 support to converter loading logic
- 3871bc4 - Add CSIBridge 25 and 26 connector implementations

---

**END OF DOCUMENT**
