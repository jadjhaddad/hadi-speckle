# ETABS22 Implementation Pattern Analysis

## Executive Summary

The ETABS22 connector is a **working, proven pattern** that uses a combination of:
1. **Shared source code** (via .projitems files) for implementation
2. **Conditional compilation defines** to enable version-specific features
3. **Direct converter instantiation** (not dynamic loading) for type safety
4. **Proper MSBuild targets** for deployment and assembly management
5. **Separate converter projects** with focused scope

This is a **fundamentally different and more robust approach** than dynamic assembly loading patterns.

---

## 1. PROJECT STRUCTURE COMPARISON

### ETABS22 (Working Pattern)

```
ConnectorCSI/
├── ConnectorETABS22/                 ← net48 connector project
│   └── ConnectorETABS22.csproj
├── ConnectorCSIShared/               ← SHARED source code (*.projitems)
│   ├── cPlugin.cs
│   ├── UI/
│   │   ├── ConnectorBindingsCSI.cs
│   │   ├── ConnectorBindingsCSI.Send.cs
│   │   ├── ConnectorBindingsCSI.Recieve.cs
│   │   └── ...
│   └── ConnectorCSIShared.projitems  ← INCLUDES all shared files

Objects/Converters/ConverterCSI/
├── ConverterETABS22/                 ← netstandard2.0 converter project
│   ├── ConverterETABS22.csproj
│   └── Class1.cs (just a stub!)
├── ConverterCSIShared/               ← SHARED converter code (*.projitems)
│   ├── ConverterCSI.cs               ← Main implementation
│   ├── PartialClasses/               ← Organized by feature
│   ├── Models/
│   ├── Extensions/
│   └── ConverterCSIShared.projitems  ← INCLUDES all shared files
```

### Legacy ETABS (Not Working)

```
ConnectorCSI/
├── ConnectorETABS/
│   └── ConnectorETABS.csproj
├── Objects references to pre-built DLLs
└── Post-build events using xcopy

Objects/Converters/ConverterCSI/
├── ConverterETABS/
│   └── ConverterETABS.csproj
└── PackageReference to CSiAPIv1 NuGet
```

---

## 2. PROJECT FILE STRUCTURE

### ConnectorETABS22.csproj (CRITICAL PATTERN)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <AssemblyName>SpeckleConnectorCSI</AssemblyName>
    <!-- CRITICAL: Version-specific defines -->
    <DefineConstants>DEBUG;TRACE;ETABS;ETABS22</DefineConstants>
  </PropertyGroup>

  <!-- Post-build: Copy converter DLL from converter project -->
  <Target Name="CopyConverterToConnectorBin" AfterTargets="Build">
    <PropertyGroup>
      <ConverterOutputPath>..\..\Objects\Converters\ConverterCSI\ConverterETABS22\bin\$(Configuration)\netstandard2.0\Objects.Converter.ETABS22.dll</ConverterOutputPath>
    </PropertyGroup>
    <Copy SourceFiles="$(ConverterOutputPath)" DestinationFolder="$(TargetDir)" />
  </Target>

  <!-- Post-build: Deploy to CSI Plug-Ins folder -->
  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Copy SourceFiles="@(DllsToCopy)" DestinationFolder="C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\"/>
  </Target>

  <!-- CRITICAL: Import shared code -->
  <Import Project="..\ConnectorCSIShared\ConnectorCSIShared.projitems" Label="Shared" />

  <!-- Project References (not NuGet!) -->
  <ItemGroup>
    <ProjectReference Include="..\..\Core\Core\Core.csproj" />
    <ProjectReference Include="..\..\DesktopUI2\DesktopUI2\DesktopUI2.csproj" />
    <ProjectReference Include="..\..\Objects\Objects\Objects.csproj" />
    <ProjectReference Include="..\..\Objects\Converters\ConverterCSI\ConverterETABS22\ConverterETABS22.csproj" />
  </ItemGroup>

  <!-- Direct DLL References for CSI API -->
  <ItemGroup>
    <Reference Include="ETABSv1">
      <HintPath>C:\Program Files\Computers and Structures\ETABS 22\ETABSv1.dll</HintPath>
      <Private>False</Private>
    </Reference>
  </ItemGroup>
</Project>
```

**KEY DIFFERENCES from Legacy:**
- DefineConstants includes BOTH `ETABS` AND `ETABS22` (for shared code to differentiate)
- MSBuild Target copies converter DLL after build
- Uses ProjectReference to ConverterETABS22 (not dynamic loading)
- Direct DLL references with `<Private>False</Private>` (DLL stays in CSI folder)

### ConverterETABS22.csproj (CRITICAL PATTERN)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <AssemblyName>Objects.Converter.ETABS22</AssemblyName>
    <!-- CRITICAL: Version-specific defines -->
    <DefineConstants>TRACE;ETABS;ETABS22</DefineConstants>
  </PropertyGroup>

  <!-- Post-build: Copy to ETABS Plug-Ins folder -->
  <Target Name="CopyToETABSPlugins" AfterTargets="PostBuildEvent">
    <Copy SourceFiles="$(TargetPath)" DestinationFolder="C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\" />
  </Target>

  <!-- CRITICAL: Import shared converter code -->
  <Import Project="..\ConverterCSIShared\ConverterCSIShared.projitems" Label="Shared" />

  <!-- Direct DLL reference for ETABS API -->
  <ItemGroup>
    <Reference Include="ETABSv1">
      <HintPath>C:\Program Files\Computers and Structures\ETABS 22\ETABSv1.dll</HintPath>
      <Private>False</Private>
    </Reference>
  </ItemGroup>
</Project>
```

**KEY POINTS:**
- netstandard2.0 target (can be referenced by both net48 and other frameworks)
- AssemblyName is `Objects.Converter.ETABS22` (version-specific name)
- Imports shared ConverterCSIShared.projitems (all actual code)
- References ETABSv1.dll directly (not via NuGet)

---

## 3. CONVERTER INSTANTIATION PATTERN

### The Working ETABS22 Pattern

In **ConnectorBindingsCSI.Send.cs** (lines 34-63):

```csharp
#if ETABS22 || CSIBRIDGE26 || CSIBRIDGE25
    // Direct instantiation - no assembly loading, preserves type identity
    var converter = new Objects.Converter.CSI.ConverterCSI();
    
    SpeckleLog.Logger.Information("✅ Created ConverterCSI instance");
    SpeckleLog.Logger.Information("🔍 Converter type: {Type}", converter.GetType().FullName);
    SpeckleLog.Logger.Information("🔍 Converter assembly: {Path}", 
        converter.GetType().Assembly.Location);
#else
    SpeckleLog.Logger.Information("✅ Using default kit manager");
    var kit = KitManager.GetDefaultKit();
    var converter = kit.LoadConverter(appName);  // DYNAMIC LOADING
#endif
```

**CRITICAL INSIGHT:** 
- ETABS22, CSIBridge25, and CSIBridge26 use **DIRECT INSTANTIATION**
- Legacy ETABS uses **KitManager.LoadConverter()** (dynamic loading)
- Direct instantiation is **type-safe** and avoids assembly loading issues

### In **ConnectorBindingsCSI.Recieve.cs** (lines 38-124):

```csharp
#if ETABS22 || CSIBRIDGE26 || CSIBRIDGE25
    // 1. Force load Objects assembly FIRST
    var objectsAssembly = typeof(Objects.Structural.Geometry.Element1D).Assembly;
    
    // 2. Manually instantiate ObjectsKit
    var objectsKit = new Objects.ObjectsKit();
    var objectsKitTypes = objectsKit.Types.ToList();
    
    // 3. Initialize KitManager (for deserialization type resolution)
    var kits = KitManager.Kits;  // Triggers KitManager.Initialize()
    
    // 4. Verify types are registered
    if (typesCount < 100) { /* manually inject types */ }
    
    // 5. Create converter directly
    var converter = new Objects.Converter.CSI.ConverterCSI();
#else
    // Legacy: Use KitManager.GetDefaultKit().LoadConverter()
#endif
```

**CRITICAL INSIGHT:**
- Must initialize KitManager BEFORE deserialization
- Must manually ensure ObjectsKit types are available
- But converter instantiation is direct (not via LoadConverter)

---

## 4. THE ACTUAL CONVERTER CLASSES

### ConverterETABS22.cs (Just a Stub!)

File: `/Objects/Converters/ConverterCSI/ConverterETABS22/Class1.cs`

```csharp
using Objects.Converter.CSI;
using Speckle.Core.Kits;

namespace Objects.Converter.ETABS22;

public class ConverterETABS22 : ConverterCSI, ISpeckleConverter
{
  // Empty class: this is just to expose the shared converter to the loader.
}
```

**CRITICAL INSIGHT:**
- The actual converter class is EMPTY
- It just inherits from the shared `ConverterCSI` base class
- The `.projitems` import brings in all the shared implementation code
- This pattern allows multiple versions to compile from the same source

### The Real Implementation: ConverterCSI.cs

File: `/Objects/Converters/ConverterCSI/ConverterCSIShared/ConverterCSI.cs`

```csharp
#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;  // For CSIBRIDGE, ETABS, SAP2000, SAFE
#endif

namespace Objects.Converter.CSI;

public partial class ConverterCSI : ISpeckleConverter, IFinalizable
{
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

  public cSapModel Model { get; private set; }
  public string ProgramVersion { get; private set; }
  
  // ... rest of implementation
}
```

**KEY PATTERN:**
- Conditional compilation to use correct API namespace (ETABSv1 vs CSiAPIv1)
- Single `cSapModel` type works for all versions due to API compatibility
- Shared implementation code handles version differences gracefully

---

## 5. SHARED PROJECT FILES (.projitems)

### ConnectorCSIShared.projitems

Includes all connector UI code:
```xml
<ItemGroup>
  <Compile Include="$(MSBuildThisFileDirectory)cPlugin.cs" />
  <Compile Include="$(MSBuildThisFileDirectory)UI\ConnectorBindingsCSI.cs" />
  <Compile Include="$(MSBuildThisFileDirectory)UI\ConnectorBindingsCSI.Send.cs" />
  <Compile Include="$(MSBuildThisFileDirectory)UI\ConnectorBindingsCSI.Recieve.cs" />
  <Compile Include="$(MSBuildThisFileDirectory)UI\ConnectorBindingsCSI.Selection.cs" />
  <!-- ... many more files ... -->
</ItemGroup>
```

### ConverterCSIShared.projitems

Includes all converter implementation:
```xml
<ItemGroup>
  <Compile Include="$(MSBuildThisFileDirectory)ConverterCSI.cs" />
  <Compile Include="$(MSBuildThisFileDirectory)ConverterCSIUtils.cs" />
  <Compile Include="$(MSBuildThisFileDirectory)Models\*.cs" />
  <Compile Include="$(MSBuildThisFileDirectory)PartialClasses\**\*.cs" />
  <!-- ... organized by feature ... -->
</ItemGroup>
```

---

## 6. KEY IMPLEMENTATION DIFFERENCES

### ETABS22 Pattern vs Legacy ETABS

| Aspect | ETABS22 (Working) | Legacy ETABS (Issues) |
|--------|-------------------|----------------------|
| **Converter Location** | Shared source (*.projitems) | Separate assembly |
| **Instantiation** | Direct: `new ConverterCSI()` | Dynamic: `KitManager.LoadConverter()` |
| **Define Constants** | ETABS22 + ETABS | Just ETABS |
| **API Reference** | Direct DLL (ETABSv1.dll) | NuGet Package (CSiAPIv1) |
| **Connector Scope** | Thin wrapper importing shared | Thicker with own code |
| **Version Handling** | Conditional compilation | Runtime detection |
| **Type Safety** | Compile-time guaranteed | Runtime via reflection |
| **Deployment** | Copy both DLLs to Plug-Ins | Post-build xcopy |

### Why ETABS22 Works Better

1. **Type Safety**: Converter type is known at compile time, not runtime
2. **No Assembly Loading**: Uses direct instantiation, avoiding AppDomain/AssemblyResolver issues
3. **Shared Code**: Single source of truth with version-specific conditionals
4. **Clear Scoping**: Each version project is thin and version-aware
5. **Better Deployment**: MSBuild targets ensure proper DLL placement

---

## 7. CONDITIONAL COMPILATION USAGE

Used in both connector and converter to handle differences:

### In ConverterCSI.cs (Lines 5-9):
```csharp
#if ETABS22
using ETABSv1;  // Different namespace!
#else
using CSiAPIv1;  // For ETABS, CSIBRIDGE, SAP2000, SAFE
#endif
```

### In cPlugin.cs (Lines 10-14):
```csharp
#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;
#endif
```

### In ConnectorBindingsCSI.Send.cs (Lines 34-63):
```csharp
#if ETABS22 || CSIBRIDGE26 || CSIBRIDGE25
    var converter = new Objects.Converter.CSI.ConverterCSI();  // DIRECT
#else
    var converter = kit.LoadConverter(appName);  // DYNAMIC
#endif
```

### API Usage Differences (Lines in various files):
```csharp
#if ETABS22
  // ETABS22-specific code path
#else
  // Legacy ETABS path
#endif
```

---

## 8. BUILD AND DEPLOYMENT STRATEGY

### Step 1: Build Converter Project
```
Build: ConverterETABS22.csproj
  Input: Objects/Converters/ConverterCSI/ConverterETABS22/
  Output: Objects/Converters/ConverterCSI/ConverterETABS22/bin/Release/netstandard2.0/
    - Objects.Converter.ETABS22.dll
    - (dependencies)
  
  Post-build Target CopyToETABSPlugins:
    Copy Objects.Converter.ETABS22.dll
    To: C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\
```

### Step 2: Build Connector Project
```
Build: ConnectorETABS22.csproj
  Input: ConnectorCSI/ConnectorETABS22/
  Output: ConnectorCSI/ConnectorETABS22/bin/Release/net48/
  
  Post-build Target CopyConverterToConnectorBin:
    Copy Objects.Converter.ETABS22.dll from converter output
    To: ConnectorCSI/ConnectorETABS22/bin/Release/net48/
    (Ensures connector has both assemblies)
  
  Post-build Target PostBuild:
    Copy ALL *.dll from output folder
    To: C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\
```

### Result in Plug-Ins Folder
```
C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\
├── SpeckleConnectorCSI.dll          (connector)
├── Objects.Converter.ETABS22.dll    (converter)
├── Objects.dll
├── Core.dll
├── SpeckleCore2.dll
└── ... all dependencies
```

---

## 9. CSIBRIDGE25/26 COMPARISON

### CSIBridge25 Pattern
- Identical structure to ETABS22
- Define: `CSIBRIDGE;CSIBRIDGE25`
- API: CSiBridge1.dll (from CSiBridge 25 installation)
- Converter: `Objects.Converter.CSIBridge25`
- Uses direct instantiation (same pattern as ETABS22)

### CSIBridge26 Pattern
- Identical structure to CSIBridge25
- Define: `CSIBRIDGE;CSIBRIDGE26`
- API: CSiBridge1.dll (from CSiBridge 26 installation)
- Converter: `Objects.Converter.CSIBridge26`
- Uses direct instantiation (same pattern as ETABS22)

**KEY INSIGHT**: CSIBridge25 and 26 follow the exact same pattern as ETABS22, suggesting this is the **proven working architecture**.

---

## 10. COMMON ETABS22/CSIBRIDGE PITFALLS

### Pitfall 1: Using NuGet CSiAPIv1
```xml
<!-- BAD -->
<PackageReference Include="CSiAPIv1" Version="1.0.0" />
```

**Why**: NuGet package may be outdated or incompatible with specific CSI version.

**Solution (ETABS22 way)**:
```xml
<!-- GOOD -->
<Reference Include="ETABSv1">
  <HintPath>C:\Program Files\Computers and Structures\ETABS 22\ETABSv1.dll</HintPath>
  <Private>False</Private>
</Reference>
```

### Pitfall 2: Dynamic Assembly Loading
```csharp
// BAD
var converter = kit.LoadConverter("ETABS22");  // Runtime resolution
```

**Why**: Assembly resolver may fail, type identity issues, AppDomain problems.

**Solution (ETABS22 way)**:
```csharp
// GOOD
var converter = new Objects.Converter.CSI.ConverterCSI();  // Compile-time resolution
```

### Pitfall 3: Missing KitManager Initialization
```csharp
// BAD - Deserialization fails
var converter = new ConverterCSI();
var obj = converter.ConvertToNative(receivedObject);  // Types not registered!
```

**Solution (ETABS22 way)**:
```csharp
// GOOD - Initialize KitManager first
var objectsKit = new Objects.ObjectsKit();
var kits = KitManager.Kits;  // Triggers initialization
var converter = new ConverterCSI();
```

### Pitfall 4: Wrong Conditional Defines
```xml
<!-- BAD: Missing version specificity -->
<DefineConstants>TRACE;ETABS</DefineConstants>
```

**Solution (ETABS22 way)**:
```xml
<!-- GOOD: Include both -->
<DefineConstants>DEBUG;TRACE;ETABS;ETABS22</DefineConstants>
```

---

## 11. WHAT MAKES ETABS22 SUCCESSFUL

1. **Single Source of Truth**: Shared .projitems files mean one implementation
2. **Version-Aware**: Conditional compilation handles API differences at build time
3. **Direct Instantiation**: No dynamic assembly loading = no resolver issues
4. **Proper Scoping**: Thin version-specific projects just glue the pieces together
5. **Clear Deployment**: MSBuild targets ensure DLLs end up in right place
6. **Type Safety**: Compiler catches many issues that runtime loading would miss
7. **Maintainability**: Adding a new CSI version (e.g., ETABS23) would be straightforward

---

## 12. MIGRATION PATH FOR CSIBRIDGE25/26

The CSIBridge25 and CSIBridge26 already follow this pattern! They are essentially:

```
ConnectorCSIBridge25/
  - Uses ConnectorCSIShared.projitems (same shared UI code)
  - DefineConstants: CSIBRIDGE;CSIBRIDGE25
  - ProjectReference to ConverterCSIBridge25

ConverterCSIBridge25/
  - Uses ConverterCSIShared.projitems (same shared converter code)
  - DefineConstants: CSIBRIDGE;CSIBRIDGE25
  - References CSiBridge1.dll from v25 installation
```

The pattern is **already correct**. Any issues are likely due to:
1. Missing defines in .csproj files
2. Wrong DLL reference paths
3. KitManager initialization problems
4. Avalonia rendering issues (CSIBridge-specific UI problem)

---

## RECOMMENDATIONS

1. **Ensure Correct .csproj Defines**: Both connector and converter need version-specific defines
2. **Use Direct Instantiation**: Don't rely on KitManager.LoadConverter() for these versions
3. **Initialize KitManager First**: In Receive path, ensure ObjectsKit types are loaded
4. **Copy Converter DLL**: Use MSBuild targets to copy converter DLL to connector output
5. **Direct API References**: Use HintPath to CSI installation DLLs, not NuGet packages
6. **Test Type Safety**: Verify at compile time that all conditional code paths are valid

