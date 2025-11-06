# CSiBridge API Investigation Results

## Executive Summary

🎯 **CRITICAL DISCOVERY**: CSiBridge has a product-specific API DLL called **CSiBridge1.dll**, similar to ETABS22's ETABSv1.dll!

**Recommendation**: Follow the **Full ETABS22 Pattern** for CSiBridge connector development, using CSiBridge1.dll instead of the generic CSiAPIv1 NuGet package.

---

## Investigation Results

### Product-Specific API Discovery

✅ **CSiBridge1.dll EXISTS** in both versions:

| Version | File Size | Date | Type |
|---------|-----------|------|------|
| CSiBridge 25 | 1,007 KB (1007K) | July 8, 2024 | .NET Assembly (PE32, Intel i386) |
| CSiBridge 26 | 1,021 KB (1021K) | June 27, 2024 | .NET Assembly (PE32, Intel i386) |

**Size Difference**: 14 KB larger in v26 (likely due to new features/API enhancements)

### Comparison with ETABS22

| Product | Product-Specific DLL | Size | Generic API |
|---------|---------------------|------|-------------|
| **ETABS 22** | ETABSv1.dll | 328 KB | CSiAPIv1.dll (1.2 MB) |
| **CSiBridge 25** | CSiBridge1.dll | 1,007 KB | CSiAPIv1.dll (1.2 MB) |
| **CSiBridge 26** | CSiBridge1.dll | 1,021 KB | CSiAPIv1.dll (1.2 MB) |

**Note**: All three products have both the product-specific DLL AND CSiAPIv1.dll, but ETABS22 connector uses only ETABSv1.dll.

---

## CSiBridge Version Differences (v25 vs v26)

### Major Enhancements in v26

#### Design Codes
- ✅ Concrete voided slab deck sections modeling
- ✅ Pile and shaft pier foundations below wall bents
- ✅ Superstructure design per AS 5100.5:2017 (Australian)
- ✅ Updated to CAN/CSA-S6-19 (Canadian)
- ✅ Eurocode substructure column strength design
- ✅ T-beam bridge design per Eurocode
- ✅ IRC 112-2020 superstructure design (Indian)
- ✅ AASHTO LRFD 2024 Caltrans amendments
- ✅ AASHTO MBE Section 6, Part B (2002 resistance for bridge rating)

#### Modeling Capabilities
- ✅ Highway curves with unequal spirals
- ✅ Two-way sloping (crowned) bridge decks
- ✅ Geometry control for balanced-cantilever segmental bridges
- ✅ Automated bridge wind loading (AASHTO 2020 9th Edition)

#### Analysis
- ✅ Large displacement analysis improvements (link elements)
- ✅ Multi-step moving load with horizontal loads

#### API & Development
- ✅ **.NET 8 support** (introduced in v25.1.0, continued in v26)
- ✅ Enhanced plugin development capabilities
- ✅ Improved multi-instance connection control
- ✅ Simplified plugin interface
- ✅ Speed enhancements for external .NET clients

### API Compatibility

**Both CSiBridge 25 and 26 support .NET development**, with v26 offering full .NET 8 support alongside .NET Framework 4.8.

**Backward Compatibility**: CSiBridge 26 API is generally backward compatible with v25, but uses an updated CSiBridge1.dll (14KB larger).

---

## Current CSiBridge Connector Configuration

### Connector Project (`ConnectorCSIBridge.csproj`)

**Current State:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>SpeckleConnectorCSI</AssemblyName>
    <TargetFramework>net48</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="NetTopologySuite" Version="2.5.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\DriverPluginCSharp\DriverPluginCSharp.csproj" />
  </ItemGroup>
</Project>
```

**Issues:**
- ❌ No API reference (relies on shared code)
- ❌ Very minimal configuration
- ❌ No direct CSiBridge DLL reference

### Converter Project (`ConverterCSIBridge.csproj`)

**Current State:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <DefineConstants>TRACE;CSIBRIDGE</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CSiAPIv1" Version="1.0.0" />
    <PackageReference Include="NetTopologySuite" Version="2.5.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\..\Core\Core\Core.csproj" />
    <ProjectReference Include="..\..\..\Objects\Objects.csproj" />
  </ItemGroup>
</Project>
```

**Issues:**
- ❌ Uses generic `CSiAPIv1` NuGet package (v1.0.0)
- ❌ Not using product-specific `CSiBridge1.dll`
- ❌ Old pattern (same as pre-ETABS22 connectors)
- ⚠️ Potential type identity issues
- ⚠️ No large model support (25MB limit)

---

## ETABS22 Pattern (Reference Implementation)

### What ETABS22 Does Right

**ConnectorETABS22.csproj:**
```xml
<PropertyGroup>
  <DefineConstants>DEBUG;TRACE;ETABS;ETABS22</DefineConstants>
  <PlatformTarget>x64</PlatformTarget>
</PropertyGroup>

<!-- Direct product-specific API reference -->
<Reference Include="ETABSv1">
  <HintPath>C:\Program Files\Computers and Structures\ETABS 22\ETABSv1.dll</HintPath>
  <Private>False</Private>
</Reference>

<!-- Direct Speckle project references -->
<ProjectReference Include="..\..\Core\Core\Core.csproj" />
<ProjectReference Include="..\..\Objects\Objects\Objects.csproj" />
<ProjectReference Include="..\..\Objects\Converters\ConverterCSI\ConverterETABS22\ConverterETABS22.csproj" />

<!-- Build targets for DLL copying -->
<Target Name="CopyConverterToConnectorBin" AfterTargets="Build">
  <!-- Copies converter DLL to connector bin -->
</Target>

<Target Name="PostBuild" AfterTargets="PostBuildEvent">
  <!-- Copies all DLLs to ETABS Plug-Ins folder -->
  <Copy SourceFiles="@(DllsToCopy)" DestinationFolder="C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\"/>
</Target>
```

**Benefits:**
- ✅ Direct product-specific API reference (type identity guaranteed)
- ✅ Direct project references (no assembly loading issues)
- ✅ Automated DLL deployment to Plug-Ins folder
- ✅ Professional installation location
- ✅ Supports large models (Collection with DetachProperty)
- ✅ Direct converter instantiation (no dynamic loading)

---

## Recommended Implementation for CSiBridge

### Decision: Use Full ETABS22 Pattern

**Rationale:**
1. ✅ CSiBridge HAS product-specific API (CSiBridge1.dll)
2. ✅ Eliminates type identity issues
3. ✅ Enables large model support
4. ✅ Professional Plug-Ins folder deployment
5. ✅ Consistency with ETABS22 architecture
6. ✅ Future-proof for CSiBridge updates

### Proposed Project Structure

#### Option A: Support Both Versions (Recommended)

Create two separate connectors:

1. **ConnectorCSIBridge25**
   - References: `C:\Program Files\Computers and Structures\CSiBridge 25\CSiBridge1.dll`
   - Deploys to: `C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\`
   - Compiler constant: `CSIBRIDGE;CSIBRIDGE25`

2. **ConnectorCSIBridge26**
   - References: `C:\Program Files\Computers and Structures\CSiBridge 26\CSiBridge1.dll`
   - Deploys to: `C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\`
   - Compiler constant: `CSIBRIDGE;CSIBRIDGE26`

**Advantages:**
- Version-specific optimizations
- No version detection needed
- Clean separation
- Follows ETABS pattern (ETABS vs ETABS22)

**Disadvantages:**
- Duplicate code (mitigated by shared code)
- Two separate builds/releases

#### Option B: Single Connector (Not Recommended)

Single connector with version detection.

**Advantages:**
- Single codebase
- Easier maintenance

**Disadvantages:**
- Complex version detection logic
- Must reference one version's DLL or both
- Potential compatibility issues
- Harder to test

### Recommendation: **Option A - Separate Connectors**

This matches the ETABS/ETABS22 pattern and provides the cleanest architecture.

---

## Implementation Roadmap

### Phase 1: Create ConnectorCSIBridge26

Since CSiBridge 26 is the newer version with .NET 8 support and more features, start here.

#### Step 1.1: Project Setup
```bash
# Create directory structure
mkdir ConnectorCSI/ConnectorCSIBridge26
mkdir Objects/Converters/ConverterCSI/ConverterCSIBridge26
```

#### Step 1.2: Create ConnectorCSIBridge26.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <RootNamespace>SpeckleConnectorCSI</RootNamespace>
    <AssemblyName>SpeckleConnectorCSI</AssemblyName>
    <PlatformTarget>x64</PlatformTarget>
    <DefineConstants>DEBUG;TRACE;CSIBRIDGE;CSIBRIDGE26</DefineConstants>
    <OutputType>Library</OutputType>
  </PropertyGroup>

  <!-- CSiBridge 26 API Reference -->
  <Reference Include="CSiBridge1">
    <HintPath>C:\Program Files\Computers and Structures\CSiBridge 26\CSiBridge1.dll</HintPath>
    <Private>False</Private>
  </Reference>

  <!-- Speckle Project References -->
  <ProjectReference Include="..\..\Core\Core\Core.csproj" />
  <ProjectReference Include="..\..\Objects\Objects\Objects.csproj" />
  <ProjectReference Include="..\..\Objects\Converters\ConverterCSI\ConverterCSIBridge26\ConverterCSIBridge26.csproj" />

  <!-- Build Targets -->
  <Target Name="CopyConverterToConnectorBin" AfterTargets="Build">
    <!-- Copy converter DLL -->
  </Target>

  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <ItemGroup>
      <DllsToCopy Include="$(TargetDir)*.dll" />
    </ItemGroup>
    <Copy SourceFiles="@(DllsToCopy)" DestinationFolder="C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\"/>
  </Target>

  <Import Project="..\ConnectorCSIShared\ConnectorCSIShared.projitems" Label="Shared" />
</Project>
```

#### Step 1.3: Create ConverterCSIBridge26.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <AssemblyName>Objects.Converter.CSIBridge26</AssemblyName>
    <RootNamespace>Objects.Converter.CSIBridge</RootNamespace>
    <DefineConstants>TRACE;CSIBRIDGE;CSIBRIDGE26</DefineConstants>
  </PropertyGroup>

  <!-- NO CSiAPIv1 NuGet package! -->
  <ItemGroup>
    <Reference Include="CSiBridge1">
      <HintPath>C:\Program Files\Computers and Structures\CSiBridge 26\CSiBridge1.dll</HintPath>
      <Private>False</Private>
    </Reference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\..\Core\Core\Core.csproj" />
    <ProjectReference Include="..\..\..\Objects\Objects.csproj" />
  </ItemGroup>

  <Import Project="..\ConverterCSIShared\ConverterCSIShared.projitems" Label="Shared" />
</Project>
```

### Phase 2: Update Shared Code for Direct Instantiation

#### Update ConnectorBindingsCSI.Send.cs

Add CSiBridge26 to direct instantiation pattern:

```csharp
#if ETABS22 || CSIBRIDGE26
  // Direct instantiation preserves type identity
  var converter = new Objects.Converter.CSI.ConverterCSI();
#else
  // Dynamic loading (old pattern)
  var converter = kit.LoadConverter(appName);
#endif
```

### Phase 3: Apply Essential Fixes

#### Large Model Support
Already applied in ETABS22, ensure CSiBridge26 inherits:
```csharp
#if ETABS22 || CSIBRIDGE26 || CSIBRIDGE25
  var commitObj = new Collection("CSI Model", "CSI");
  commitObj.elements = objects.Cast<Base>().ToList();
#endif
```

#### Line Conversion Fix
Verify shared code uses 8-parameter version:
```csharp
Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame, "Default");
```

### Phase 4: Create CSiBridge25 Connector (Optional)

If users need CSiBridge 25 support, repeat Phase 1-3 with:
- Compiler constant: `CSIBRIDGE;CSIBRIDGE25`
- API path: `C:\Program Files\Computers and Structures\CSiBridge 25\CSiBridge1.dll`
- Install path: `C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\`

---

## Testing Strategy

### Test Matrix

| Test Case | CSiBridge 25 | CSiBridge 26 |
|-----------|--------------|--------------|
| Type Identity (Model is Base?) | ✅ Should be True | ✅ Should be True |
| Small Model (<1MB) Send | ✅ | ✅ |
| Medium Model (1-25MB) Send | ✅ | ✅ |
| Large Model (>25MB) Send | ✅ Must use Collection | ✅ Must use Collection |
| Receive Simple Model | ✅ | ✅ |
| Receive with Line Geometry | ✅ 8-param API call | ✅ 8-param API call |
| Plugin Load in Software | ✅ | ✅ |
| DLL Deployment | ✅ Plug-Ins folder | ✅ Plug-Ins folder |

### Validation Checklist

**Type Identity:**
- [ ] Verify `Model is Base? True` in logs
- [ ] No "unsupported values" errors
- [ ] Serialization succeeds

**Large Models:**
- [ ] Models >25MB send successfully
- [ ] Collection with detached elements used
- [ ] Server accepts commit object

**Line Conversion:**
- [ ] Lines create frames in CSiBridge
- [ ] No error code 1 from API
- [ ] Section property parameter included

**Build Process:**
- [ ] DLLs copy to Plug-Ins folder
- [ ] Converter DLL in connector bin
- [ ] All dependencies present

**Multi-Version:**
- [ ] CSiBridge 25 and 26 can coexist
- [ ] No DLL conflicts
- [ ] Each loads correct version

---

## Build Scripts

### build-csibridge26.bat
```batch
@echo off
echo ========================================
echo Building CSiBridge 26 Connector
echo ========================================

REM Clean previous builds
echo Cleaning previous builds...
if exist "ConnectorCSI\ConnectorCSIBridge26\bin" rmdir /s /q "ConnectorCSI\ConnectorCSIBridge26\bin"
if exist "ConnectorCSI\ConnectorCSIBridge26\obj" rmdir /s /q "ConnectorCSI\ConnectorCSIBridge26\obj"
if exist "Objects\Converters\ConverterCSI\ConverterCSIBridge26\bin" rmdir /s /q "Objects\Converters\ConverterCSI\ConverterCSIBridge26\bin"
if exist "Objects\Converters\ConverterCSI\ConverterCSIBridge26\obj" rmdir /s /q "Objects\Converters\ConverterCSI\ConverterCSIBridge26\obj"

REM Build converter first
echo Building ConverterCSIBridge26...
dotnet build "Objects\Converters\ConverterCSI\ConverterCSIBridge26\ConverterCSIBridge26.csproj" --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Converter build failed!
    exit /b 1
)

REM Build connector (will copy converter DLL and deploy to Plug-Ins)
echo Building ConnectorCSIBridge26...
dotnet build "ConnectorCSI\ConnectorCSIBridge26\ConnectorCSIBridge26.csproj" --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Connector build failed!
    exit /b 1
)

echo ========================================
echo Build Complete!
echo DLLs deployed to: C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\
echo ========================================
```

### build-csibridge26.ps1
```powershell
Write-Host "========================================"
Write-Host "Building CSiBridge 26 Connector"
Write-Host "========================================"

# Clean previous builds
Write-Host "Cleaning previous builds..."
Remove-Item -Path "ConnectorCSI\ConnectorCSIBridge26\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "ConnectorCSI\ConnectorCSIBridge26\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Objects\Converters\ConverterCSI\ConverterCSIBridge26\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Objects\Converters\ConverterCSI\ConverterCSIBridge26\obj" -Recurse -Force -ErrorAction SilentlyContinue

# Build converter
Write-Host "Building ConverterCSIBridge26..."
dotnet build "Objects\Converters\ConverterCSI\ConverterCSIBridge26\ConverterCSIBridge26.csproj" --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Converter build failed!"
    exit 1
}

# Build connector
Write-Host "Building ConnectorCSIBridge26..."
dotnet build "ConnectorCSI\ConnectorCSIBridge26\ConnectorCSIBridge26.csproj" --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Connector build failed!"
    exit 1
}

Write-Host "========================================"
Write-Host "Build Complete!"
Write-Host "DLLs deployed to: C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\"
Write-Host "========================================"
```

---

## Summary & Next Steps

### Key Findings

✅ **CSiBridge1.dll exists** - Product-specific API confirmed
✅ **Full ETABS22 pattern applicable** - Use CSiBridge1.dll instead of CSiAPIv1
✅ **Version differences documented** - v26 has 14KB larger DLL with new features
✅ **.NET 8 support confirmed** - v26 ready for modern .NET development
✅ **API compatibility preserved** - Can support both v25 and v26

### Immediate Actions

1. **Create ConnectorCSIBridge26** following ETABS22 pattern
2. **Reference CSiBridge1.dll** directly, not CSiAPIv1 NuGet
3. **Apply Collection fix** for large model support
4. **Set up Plug-Ins deployment** to `C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\`
5. **Update shared code** for direct converter instantiation
6. **Test thoroughly** with small, medium, and large models

### Timeline Estimate

- **Week 1**: Project setup, ConnectorCSIBridge26 creation
- **Week 2**: Apply fixes, build configuration, testing
- **Week 3** (Optional): Create ConnectorCSIBridge25 if needed
- **Week 4**: Documentation, final testing, release prep

### Success Criteria

✅ Type identity verified (`Model is Base? True`)
✅ Large models (>25MB) send successfully
✅ Line geometry converts without errors
✅ DLLs deploy to Plug-Ins folder automatically
✅ No assembly loading issues
✅ Compatible with CSiBridge 26 (and optionally 25)

---

## References

- ETABS22 connector implementation: `ConnectorCSI/ConnectorETABS22/`
- ETABS22 serialization fix: `ETABS22_SERIALIZATION_FIX.md`
- Large model fix: `SEND_LARGE_MODELS.md`
- Line conversion fix: `RECEIVE_DIAGNOSTIC.md`
- Shared converter code: `Objects/Converters/ConverterCSI/ConverterCSIShared/`
- Shared connector code: `ConnectorCSI/ConnectorCSIShared/`

---

**Document Created**: November 5, 2025
**CSiBridge Versions Analyzed**: 25, 26
**Comparison Baseline**: ETABS 22 connector implementation
