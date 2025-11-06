# CSIBridge 25/26 vs ETABS22: Detailed Implementation Comparison

**Analysis Date**: November 6, 2025  
**Status**: CSIBridge 25/26 NOT WORKING (crash during window layout)  
**Comparison Baseline**: ETABS22 (WORKING)

---

## Executive Summary

CSIBridge 25 and 26 connectors have been created following the ETABS22 pattern, but **CSIBridge 25 is crashing during window initialization** while CSIBridge 26 hasn't been tested yet. The project structures are nearly identical, but there are critical differences in:

1. **API References** - Both products use hybrid approach (CSiBridge1.dll + CSiAPIv1.dll)
2. **Build Deployment** - CSIBridge25 deploys more dependencies than ETABS22
3. **Shared Code Behavior** - CSIBridge has conditional compilation for rendering/window handling
4. **Known Issue** - Avalonia layout crash specific to CSIBridge 25 environment

---

## 1. Connector Project Structure Comparison

### ConnectorETABS22.csproj (WORKING REFERENCE)

**File Size**: 4,357 bytes  
**Target Framework**: net48  
**Platform Target**: x64  
**Compiler Constants**: `DEBUG;TRACE;ETABS;ETABS22`

**Key Properties**:
```xml
<TargetFramework>net48</TargetFramework>
<RootNamespace>SpeckleConnectorCSI</RootNamespace>
<AssemblyName>SpeckleConnectorCSI</AssemblyName>
<PlatformTarget>x64</PlatformTarget>
<DefineConstants>DEBUG;TRACE;ETABS;ETABS22</DefineConstants>
```

**API References**:
- `ETABSv1.dll` from `C:\Program Files\Computers and Structures\ETABS 22\`
- Set to `Private=False` (not copied to output)

**PostBuild Deployment**:
```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
  <ItemGroup>
    <DllsToCopy Include="$(TargetDir)*.dll" />
  </ItemGroup>
  <Copy SourceFiles="@(DllsToCopy)" 
        DestinationFolder="C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\"/>
</Target>
```
- Simple: Only copies *.dll files
- No config, runtimes, or native libs

---

### ConnectorCSIBridge25.csproj (NOT WORKING - CRASHES)

**File Size**: 5,650 bytes (+1,293 bytes, +30%)  
**Target Framework**: net48  
**Platform Target**: x64  
**Compiler Constants**: `DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE25`

**Key Properties** - Same as ETABS22:
```xml
<TargetFramework>net48</TargetFramework>
<RootNamespace>SpeckleConnectorCSI</RootNamespace>
<AssemblyName>SpeckleConnectorCSI</AssemblyName>
<PlatformTarget>x64</PlatformTarget>
<DefineConstants>DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE25</DefineConstants>
<NoWarn>NU1605</NoWarn>  <!-- ADDED: Suppress NuGet warnings -->
```

**API References** - DIFFERS:
```xml
<Reference Include="CSiAPIv1">
  <HintPath>C:\Program Files\Computers and Structures\CSiBridge 25\CSiAPIv1.dll</HintPath>
</Reference>
<Reference Include="CSiBridge1">
  <HintPath>C:\Program Files\Computers and Structures\CSiBridge 25\CSiBridge1.dll</HintPath>
  <Private>False</Private>
</Reference>
```
- **BOTH** CSiAPIv1.dll AND CSiBridge1.dll are referenced
- ETABS22 only references ETABSv1.dll
- CSiBridge1.dll is the product-specific API (like ETABSv1.dll)
- CSiAPIv1.dll is the generic API (NOT used by ETABS22)

**PostBuild Deployment** - DIFFERS (MORE COMPLEX):
```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
  <ItemGroup>
    <DllsToCopy Include="$(TargetDir)*.dll" />
    <ConfigsToCopy Include="$(TargetDir)*.config" />
    <RuntimesToCopy Include="$(TargetDir)runtimes\**\*.*" />
    <NativeLibsToCopy Include="$(TargetDir)x86\**\*.*" />
    <NativeLibsToCopy Include="$(TargetDir)x64\**\*.*" />
    <NativeLibsToCopy Include="$(TargetDir)arm\**\*.*" />
    <NativeLibsToCopy Include="$(TargetDir)arm64\**\*.*" />
    <NativeLibsToCopy Include="$(TargetDir)musl-x64\**\*.*" />
    <NativeLibsToCopy Include="$(TargetDir)Native\**\*.*" />
    <DylibsToCopy Include="$(TargetDir)*.dylib" />
  </ItemGroup>
  <Copy SourceFiles="@(DllsToCopy)" 
        DestinationFolder="C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\" />
  <Copy SourceFiles="@(ConfigsToCopy)" 
        DestinationFolder="C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\" />
  <Copy SourceFiles="@(RuntimesToCopy)" 
        DestinationFolder="C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\runtimes\%(RecursiveDir)" />
  <Copy SourceFiles="@(NativeLibsToCopy)" 
        DestinationFolder="C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\%(RecursiveDir)" />
  <Copy SourceFiles="@(DylibsToCopy)" 
        DestinationFolder="C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\" />
</Target>
```
- **5 copy operations** vs ETABS22's 1 copy operation
- Includes: .config files, runtimes/, native libs, dylibs
- Multi-platform native library support (x86, x64, arm, arm64, musl-x64, Native/)

**System References** - SIMILAR BUT DIFFERENT:
```xml
<!-- ETABS22 -->
<Reference Include="NetTopologySuite">
  <HintPath>..\..\packages\NetTopologySuite.2.5.0\lib\netstandard2.0\NetTopologySuite.dll</HintPath>
</Reference>
<Reference Include="System.Numerics.Vectors">
  <HintPath>..\..\packages\System.Numerics.Vectors.4.5.0\lib\net46\System.Numerics.Vectors.dll</HintPath>
</Reference>

<!-- CSIBridge25 -->
<PackageReference Include="System.Numerics.Vectors" Version="4.4.0" />
```
- ETABS22 references NuGet packages via HintPath (local)
- CSIBridge25 uses PackageReference for System.Numerics.Vectors (NuGet)

---

### ConnectorCSIBridge26.csproj (UNTESTED)

**File Size**: Same class as CSIBridge25 (simpler deployment)

**Key Differences from CSIBridge25**:
```xml
<!-- CSIBridge26 does NOT have NoWarn -->
<!-- CSIBridge26 does NOT reference System.Numerics.Vectors as PackageReference -->
<!-- CSIBridge26 has SIMPLIFIED PostBuild (like ETABS22) -->
<Copy SourceFiles="@(DllsToCopy)" 
      DestinationFolder="C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\" />
<!-- Only 1 copy operation, not 5 -->
```

**Why the Difference?**
- CSIBridge25 may need broader runtime/native library support due to dependencies
- CSIBridge26 was created more recently, may have cleaner dependencies
- CSIBridge25's additional copies might be cargo-culting or defensive

---

## 2. Converter Project Structure Comparison

### ConverterETABS22.csproj (WORKING)

**Target Framework**: netstandard2.0  
**Assembly Name**: Objects.Converter.ETABS22  
**Compiler Constants**: `TRACE;ETABS;ETABS22`

**API References**:
```xml
<Reference Include="ETABSv1">
  <HintPath>...[ETABS 22]\ETABSv1.dll</HintPath>
  <Private>False</Private>
</Reference>
```
- Only ETABSv1.dll (no CSiAPIv1.dll)
- This is the key difference: product-specific only

**PostBuild Targets**:
```xml
<!-- Copies to kit folder (AppData\Speckle\Kits\Objects) -->
<Target Name="CopyDependenciesToKitfolder" ...>

<!-- Also copies to Plug-Ins folder -->
<Target Name="CopyToETABSPlugins" AfterTargets="PostBuildEvent">
  <Copy ... DestinationFolder="C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\" />
</Target>
```

---

### ConverterCSIBridge25.csproj (NOT WORKING)

**Target Framework**: netstandard2.0  
**Assembly Name**: Objects.Converter.CSIBridge25  
**Compiler Constants**: `TRACE;CSIBRIDGE;CSIBRIDGE25`

**API References** - DIFFERS:
```xml
<Reference Include="CSiAPIv1">
  <HintPath>...[CSiBridge 25]\CSiAPIv1.dll</HintPath>
</Reference>
<Reference Include="CSiBridge1">
  <HintPath>C:\Program Files\Computers and Structures\CSiBridge 25\CSiBridge1.dll</HintPath>
  <Private>False</Private>
</Reference>
```
- References BOTH CSiAPIv1.dll AND CSiBridge1.dll
- CSIAPIv1.dll is actually referenced (unlike ConverterETABS22)
- Path inconsistency: CSiAPIv1 uses HintPath with relative path, CSiBridge1 uses absolute

**Package References** - ADDED:
```xml
<PackageReference Include="System.Numerics.Vectors" Version="4.4.0" />
```
- ETABS22 converter does not have this
- CSIBridge25 adds this dependency

**PostBuild Targets** - MISSING:
```xml
<!-- CSIBridge25 converter does NOT have CopyToETABSPlugins target! -->
<!-- Only has CopyDependenciesToKitfolder -->
```
- CSIBridge25 converter only copies to kit folder, not Plug-Ins folder
- Connector handles Plug-Ins folder copying instead
- ✅ This is correct pattern (connector copies converter DLL to Plug-Ins)

---

### ConverterCSIBridge26.csproj (UNTESTED)

**Key Differences from CSIBridge25**:
```xml
<!-- Does NOT have NoWarn -->
<!-- Does NOT have System.Numerics.Vectors PackageReference -->
<!-- Otherwise identical structure -->
```

---

## 3. Shared Code Differences

### API Import Differences (cPlugin.cs, lines 10-14)

```csharp
#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;
#endif
```

**Issue**: CSIBridge 25/26 imports CSiAPIv1 (generic) but references BOTH CSiAPIv1.dll AND CSiBridge1.dll

**Why This Matters**:
- Both DLLs are loaded into memory
- CSiAPIv1 may have older/different type definitions than CSiBridge1
- Type conflicts possible if both provide same classes
- ETABS22 avoids this by only importing ETABSv1

---

### Window Initialization Differences (cPlugin.cs, lines 38-46)

```csharp
#if CSIBRIDGE
  // Disable GPU rendering to avoid conflicts with CSIBridge's own rendering
  .With(new Win32PlatformOptions { 
    AllowEglInitialization = false,    // ← Disabled for CSIBridge
    EnableMultitouch = false, 
    UseWgl = false                      // ← Disabled for CSIBridge
  })
#else
  // ETABS and others: Use standard GPU rendering
  .With(new Win32PlatformOptions { 
    AllowEglInitialization = true,      // ← Enabled for ETABS
    EnableMultitouch = false 
  })
#endif
```

**ETABS22**: Uses EGL and standard GPU rendering  
**CSIBridge**: Explicitly disables EGL, WGL, and GPU rendering  

**Hypothesis**: CSIBridge 25's host environment conflicts with Avalonia's GPU rendering

---

### Message Loop Differences (cPlugin.cs, lines 134-156)

```csharp
#if CSIBRIDGE
  SpeckleLog.Logger.Information("🔧 CSIBridge mode: Using MainWindow.Show() instead of app.Run()");
  MainWindow.Show();
  // No app.Run() call
#else
  SpeckleLog.Logger.Information("🔧 ETABS mode: Using app.Run(MainWindow)");
  app.Run(MainWindow);  // Standard Avalonia pattern
#endif
```

**ETABS22**: Uses `app.Run(MainWindow)` (standard Avalonia)  
**CSIBridge**: Uses only `MainWindow.Show()` (skips event loop)  

**Implication**: CSIBridge doesn't enter Avalonia's main event loop, may cause window management issues

---

### Converter Instantiation (ConnectorBindingsCSI.Send.cs, lines 34-63)

```csharp
#if ETABS22 || CSIBRIDGE26 || CSIBRIDGE25
  #if ETABS22
    SpeckleLog.Logger.Information("✅ Using direct converter reference for ETABS22");
  #elif CSIBRIDGE26
    SpeckleLog.Logger.Information("✅ Using direct converter reference for CSiBridge26");
  #elif CSIBRIDGE25
    SpeckleLog.Logger.Information("✅ Using direct converter reference for CSiBridge25");
  #endif
  
  // Direct instantiation - no assembly loading, preserves type identity
  var converter = new Objects.Converter.CSI.ConverterCSI();
#else
  // Legacy products use dynamic loading
  var converter = kit.LoadConverter(appName);
#endif
```

**Both CSIBridge25 and CSIBridge26 use direct instantiation** (✅ Correct)  
**Same pattern as ETABS22** (✅ Correct)

---

### Collection Usage for Large Models (ConnectorBindingsCSI.Send.cs, lines 251-281)

```csharp
// Use Collection instead of Base to ensure elements are detached properly
var commitObj = new Collection("CSI Model", "CSI");

commitObj["@Model"] = modelObj;
commitObj.elements = objects.Cast<Base>().ToList();  // Elements are detached
```

**All products use Collection with [DetachProperty]**:
- ETABS22 ✅
- CSIBridge25 ✅
- CSIBridge26 ✅

**Same pattern everywhere** (✅ Correct)

---

### Receive Operation Type Registration (ConnectorBindingsCSI.Recieve.cs, lines 38-124)

```csharp
#if ETABS22 || CSIBRIDGE26 || CSIBRIDGE25
  // Force load the Objects assembly BEFORE initializing KitManager
  var objectsAssembly = typeof(Objects.Structural.Geometry.Element1D).Assembly;
  
  // Manually instantiate ObjectsKit to ensure its types are available
  var objectsKit = new Objects.ObjectsKit();
  
  // Initialize KitManager
  var kits = KitManager.Kits;
  
  // Verify types are registered
  var typesCount = KitManager.Types.Count();
  
  // Use reflection to inject types if needed
  // ...
  
  // Direct instantiation
  var converter = new Objects.Converter.CSI.ConverterCSI();
#else
  var converter = kit.LoadConverter(appName);
#endif
```

**All three products follow identical pattern** (✅ Correct)

---

## 4. Key Differences Summary Table

| Aspect | ETABS22 | CSIBridge25 | CSIBridge26 | Status |
|--------|---------|------------|------------|--------|
| **API References** | ETABSv1 only | CSiAPIv1 + CSiBridge1 | CSiAPIv1 + CSiBridge1 | ⚠️ Hybrid approach |
| **GPU Rendering** | Enabled (EGL) | Disabled | Disabled | ⚠️ Different config |
| **App.Run()** | Yes | No (Show only) | No (Show only) | ⚠️ Different loop |
| **Converter Reference** | Direct instantiation | Direct instantiation | Direct instantiation | ✅ Same |
| **Collection Pattern** | Yes, with detach | Yes, with detach | Yes, with detach | ✅ Same |
| **Type Registration** | Manual init | Manual init | Manual init | ✅ Same |
| **PostBuild Deploy** | 1 copy operation | 5 copy operations | 1 copy operation | ⚠️ CSIBridge25 more complex |
| **System.Numerics.Vectors** | Via HintPath | Via PackageReference | No reference | ⚠️ Inconsistent |
| **Status** | ✅ WORKING | ❌ CRASHES | ❓ UNTESTED | |

---

## 5. Root Cause Analysis: CSIBridge 25 Crash

### What Happens (From Logs)

```
1. OpenOrFocusSpeckle() is called
2. CreateOrFocusSpeckle() creates Avalonia app
3. AppMain() is called
4. MainWindow created successfully
5. MainWindow.Show() is called
6. CRASH in Avalonia.Layout.LayoutManager.RaiseEffectiveViewportChanged()
```

### Likely Causes (Ranked by Probability)

#### 1. **GPU Rendering Conflict** (HIGH PROBABILITY)
- CSIBridge 25 disables EGL/WGL GPU rendering
- But still uses SkiaOptions GPU resource configuration
- LayoutManager might still try to use GPU despite disabled EGL
- **Evidence**: Window shows properly before Show() - rendering initialization is the trigger

#### 2. **Invalid Window Metrics** (MEDIUM PROBABILITY)
- CSIBridge 25 environment reports invalid window dimensions
- Causes layout calculation to produce NaN or invalid values
- **Evidence**: Need logs showing window Width/Height/Bounds values

#### 3. **Application Message Loop Conflict** (MEDIUM PROBABILITY)
- CSIBridge uses MainWindow.Show() without app.Run()
- Avalonia expects to be in main event loop when calculating layout
- **Evidence**: Same code works in ETABS (which uses app.Run())

#### 4. **Plugin Callback Threading** (MEDIUM PROBABILITY)
- CSIBridge callback happens on different thread than ETABS
- Window creation/Show() happens on wrong thread
- **Evidence**: Would see threading exceptions in logs

#### 5. **CSiAPIv1.dll Type Conflict** (LOWER PROBABILITY)
- CSIBridge25 references both CSiAPIv1.dll and CSiBridge1.dll
- Type definitions might conflict
- But this would affect model loading, not window layout
- **Evidence**: No model operations before crash

---

## 6. Why CSIBridge26 Might Work

### Differences from CSIBridge25

1. **Simplified PostBuild** - Only 1 copy operation (like ETABS22)
   - CSIBridge25's 5 copy operations might be pulling in problematic dependencies

2. **Cleaner Dependencies** - No System.Numerics.Vectors PackageReference
   - CSIBridge25's extra package might introduce compatibility issues

3. **Newer API DLL** - 14 KB larger CSiBridge1.dll (v26 > v25)
   - v26 might have GPU rendering fixes

4. **Better Threading Support** - v26 has "Improved multi-instance connection control"
   - Might handle plugin callbacks better

---

## 7. Recommended Actions

### For CSIBridge 25 (Currently Crashing)

1. **Option A: Force Software Rendering**
   ```csharp
   .With(new SkiaOptions { MaxGpuResourceSizeBytes = 0 })  // Force software rendering
   ```

2. **Option B: Delay Window Show**
   ```csharp
   await Task.Delay(200);  // Let CSIBridge settle
   MainWindow.Show();
   ```

3. **Option C: Custom Window Initialization**
   ```csharp
   MainWindow.Width = 400;
   MainWindow.Height = 650;
   MainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
   MainWindow.Position = new PixelPoint(100, 100);
   MainWindow.Show();
   ```

4. **Option D: Try app.Run() Anyway**
   - Override #if CSIBRIDGE condition temporarily
   - Test if app.Run() works despite expectations

### For CSIBridge 26 (Not Yet Tested)

1. **Build and test immediately**
   - Simpler deployment might already fix the issue
   - Different API DLL might have fixes

2. **If it works**: Confirm why (GPU rendering? Dependencies? Newer API?)

3. **If it fails**: Apply same fixes as CSIBridge 25

### For Both

1. **Verify HintPath Consistency**
   - ConverterCSIBridge25 has inconsistent HintPaths (one relative, one absolute)
   - Make both absolute or both relative

2. **Remove Dual API References**
   - Consider referencing ONLY CSiBridge1.dll (like ETABS22 does)
   - Remove CSiAPIv1.dll from converter project
   - Update #if directive to use CSiBridge1 namespace only

3. **Simplify PostBuild**
   - CSIBridge25's 5 copy operations might be defensive/incorrect
   - Align with CSIBridge26's single copy operation
   - Only deploy what's actually needed

4. **Add Detailed Logs**
   - Already added to cPlugin.cs ✅
   - Capture window metrics at crash time
   - Determine if Show() or layout calculation crashes

---

## 8. File Locations Summary

### Working Reference
- `/ConnectorCSI/ConnectorETABS22/ConnectorETABS22.csproj`
- `/Objects/Converters/ConverterCSI/ConverterETABS22/ConverterETABS22.csproj`

### Not Working
- `/ConnectorCSI/ConnectorCSIBridge25/ConnectorCSIBridge25.csproj`
- `/Objects/Converters/ConverterCSI/ConverterCSIBridge25/ConverterCSIBridge25.csproj`

### Untested
- `/ConnectorCSI/ConnectorCSIBridge26/ConnectorCSIBridge26.csproj`
- `/Objects/Converters/ConverterCSI/ConverterCSIBridge26/ConverterCSIBridge26.csproj`

### Shared Code (All Use)
- `/ConnectorCSI/ConnectorCSIShared/cPlugin.cs` (lines 10-156)
- `/ConnectorCSI/ConnectorCSIShared/UI/ConnectorBindingsCSI.Send.cs` (lines 34-281)
- `/ConnectorCSI/ConnectorCSIShared/UI/ConnectorBindingsCSI.Recieve.cs` (lines 38-124)

### Investigation Documents
- `/ConnectorCSI/CSIBRIDGE25_CRASH_INVESTIGATION.md` (Latest investigation)
- `/ConnectorCSI/API_FINDINGS_CSIBRIDGE.md` (API analysis)
- `/ConnectorCSI/CSI_CONNECTORS.md` (Comprehensive documentation)

---

## 9. Next Steps

1. **Collect crash logs** from CSIBridge 25 with detailed logging
   - Window dimensions at Show() time
   - Full exception details
   
2. **Test CSIBridge 26** with current implementation
   - May reveal if issue is v25-specific or pattern-specific
   
3. **Try mitigation options** in order:
   - Option A: Force software rendering (simplest)
   - Option B: Delay window show (quick test)
   - Option C: Custom window initialization (more involved)
   - Option D: Use app.Run() (contradicts current design)

4. **Align CSIBridge25 with CSIBridge26**
   - Use simplified PostBuild
   - Remove extra PackageReferences
   - Fix HintPath consistency

---

**Document Prepared**: November 6, 2025  
**Analysis Basis**: Comparison of 6 project files + 3 investigation documents  
**Conclusion**: CSIBridge connectors follow ETABS22 pattern correctly, but Avalonia/CSIBridge25 compatibility issue remains to be resolved
