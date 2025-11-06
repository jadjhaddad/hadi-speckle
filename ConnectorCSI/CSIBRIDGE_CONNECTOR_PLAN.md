# CSiBridge Connector Migration Plan

## Executive Summary

This document outlines the work completed for ETABS22 connector and provides a roadmap for updating the CSiBridge connector. The ETABS22 work addressed three critical issues:
1. **Type identity problems** causing serialization failures
2. **25MB object size limit** preventing large model transfers
3. **Line geometry conversion failures** in the ETABS API

## Table of Contents

1. [ETABS22 Changes Overview](#etabs22-changes-overview)
2. [CSiBridge Current State](#csibridge-current-state)
3. [API/DLL Analysis](#apidll-analysis)
4. [Migration Recommendations](#migration-recommendations)
5. [Implementation Checklist](#implementation-checklist)

---

## ETABS22 Changes Overview

### 1. Assembly Loading and Type Identity Fix

#### Problem
The most critical issue was a type identity mismatch that caused serialization to fail:
- ConnectorETABS22 referenced `Objects.dll` from AppData (old version)
- ConverterETABS22 used locally built Objects.dll
- Result: Serialization rejected objects with `Model is Base? False`
- Error: "Unsupported values in the commit object"

#### Solution
Changed from DLL references to direct project references in `ConnectorETABS22.csproj`:

**Before:**
```xml
<Reference Include="Objects">
  <HintPath>$(LocalAppData)\Programs\SpeckleManager\Connectors\ETABS22\Objects.dll</HintPath>
  <Private>False</Private>
</Reference>
```

**After:**
```xml
<ProjectReference Include="..\..\Objects\Objects\Objects.csproj" />
<ProjectReference Include="..\..\Core\Core\Core.csproj" />
<ProjectReference Include="..\..\Objects\Converters\ConverterCSI\ConverterETABS22\ConverterETABS22.csproj" />
```

#### Direct Converter Instantiation
Modified `ConnectorBindingsCSI.Send.cs` to bypass dynamic loading:

```csharp
#if ETABS22
  // Direct instantiation preserves type identity
  var converter = new Objects.Converter.CSI.ConverterCSI();
#else
  // Dynamic loading (old pattern)
  var converter = kit.LoadConverter(appName);
#endif
```

### 2. Large Model Support (25MB+ Objects)

#### Problem
Models exceeding 25MB object size were rejected by the Speckle server.

#### Solution
Changed from sending a single Base object to using Collection with detached elements in `ConnectorBindingsCSI.Send.cs`:

**Before:**
```csharp
var commitObj = new Base();
commitObj["elements"] = objects;
```

**After:**
```csharp
var commitObj = new Collection("CSI Model", "CSI");
commitObj.elements = objects.Cast<Base>().ToList();
```

**Why this works:**
- `Collection.elements` has `[DetachProperty]` attribute
- Each element stored separately in database
- Commit object contains only references (IDs), not full objects
- Keeps commit object under 25MB limit regardless of model complexity

### 3. Line Geometry Conversion Fix

#### Problem
`LineToNative` failed with error code 1 - frames were not created in ETABS.

#### Root Cause
Missing required section property parameter in ETABS API call.

#### Solution
Updated `ConvertLine.cs` to include section property parameter:

**Before (7 parameters - failed):**
```csharp
Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame);
```

**After (8 parameters - works):**
```csharp
Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame, "Default");
```

**Additional improvements:**
- Added minimum frame length check (0.1 units tolerance)
- Comprehensive diagnostic logging
- Coordinate validation

### 4. Build Process Enhancements

#### New Build Targets
Added to `ConnectorETABS22.csproj`:

1. **CopyConverterToConnectorBin**
   ```xml
   <Target Name="CopyConverterToConnectorBin" AfterTargets="Build">
     <!-- Copies converter DLL to connector's bin directory -->
   </Target>
   ```

2. **PostBuild**
   ```xml
   <Target Name="PostBuild" AfterTargets="CopyConverterToConnectorBin">
     <!-- Copies all dependencies to ETABS Plug-Ins folder -->
   </Target>
   ```

#### Install Location Change
- **Old**: `$(LocalAppData)\Programs\SpeckleManager\Connectors\ETABS22\`
- **New**: `C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\`

#### Documentation Created
- `ETABS22_SERIALIZATION_FIX.md` - Type identity solution details
- `SEND_LARGE_MODELS.md` - Collection usage for large models
- `RECEIVE_DIAGNOSTIC.md` - Line conversion troubleshooting guide
- `BUILD_COMMANDS.txt` - Step-by-step build instructions
- `build-etabs22.bat` / `build-etabs22.ps1` - Automated build scripts

### 5. API Configuration

**ETABS22 Specific:**
- Uses `ETABSv1.dll` from `C:\Program Files\Computers and Structures\ETABS 22\`
- **Does NOT use** CSiAPIv1 NuGet package (unlike older versions)
- Direct COM reference in project file
- Compiler constants: `DEBUG;TRACE;ETABS;ETABS22`

---

## CSiBridge Current State

### Project Structure

**Connector Project:** `ConnectorCSI/ConnectorCSIBridge/ConnectorCSIBridge.csproj`
- Minimal configuration (13 lines)
- Uses shared code via `ConnectorCSIShared.projitems`
- References: NetTopologySuite, DriverPluginCSharp

**Converter Project:** `Objects/Converters/ConverterCSI/ConverterCSIBridge/ConverterCSIBridge.csproj`
- Target: netstandard2.0
- Compiler constant: `CSIBRIDGE`
- Uses shared converter code via `ConverterCSIShared.projitems`

### API Configuration

**Current:**
- Uses `CSiAPIv1` NuGet package (Version 1.0.0)
- Same pattern as older ETABS, SAP2000, SAFE connectors
- Dynamic converter loading via KitManager
- Install location: AppData/LocalAppData

### Key Differences from ETABS22

| Aspect | CSiBridge (Current) | ETABS22 (New) |
|--------|-------------------|---------------|
| **API Reference** | CSiAPIv1 NuGet package | Direct ETABSv1.dll COM reference |
| **Objects Reference** | Via NuGet/shared locations | Direct project reference |
| **Converter Loading** | Dynamic via KitManager | Direct instantiation (#if ETABS22) |
| **Build Complexity** | Minimal configuration | Complex with copy targets |
| **Install Location** | AppData/LocalAppData | ProgramData Plug-Ins folder |
| **Large Model Support** | No (25MB limit) | Yes (Collection with detach) |

---

## API/DLL Analysis

### Installed Versions

#### CSiBridge 25
- **Path:** `C:\Program Files\Computers and Structures\CSiBridge 25\`
- **CSiAPIv1.dll:** 1,168,392 bytes (July 8, 2024)
- **Status:** Older API version

#### CSiBridge 26
- **Path:** `C:\Program Files\Computers and Structures\CSiBridge 26\`
- **CSiAPIv1.dll:** 1,187,336 bytes (June 27, 2024)
- **Status:** Newer API, 18,944 bytes larger than v25

#### ETABS 22
- **Path:** `C:\Program Files\Computers and Structures\ETABS 22\`
- **CSiAPIv1.dll:** 1,187,392 bytes (July 11, 2024)
- **ETABSv1.dll:** 335,424 bytes (July 11, 2024)
- **Note:** ETABS22 connector uses ETABSv1.dll, not CSiAPIv1.dll

### Key Findings

1. **API Size Comparison:**
   - CSiBridge 26 and ETABS 22 have nearly identical CSiAPIv1.dll (differ by only 56 bytes)
   - Suggests they're from the same API generation
   - CSiBridge 25 uses significantly older API

2. **Product-Specific API:**
   - ETABS has product-specific `ETABSv1.dll`
   - Need to check if CSiBridge has `CSiBridgev1.dll`
   - This determines whether to follow ETABS22 pattern

3. **Version Compatibility:**
   - Different DLL sizes suggest API changes between CSiBridge 25 and 26
   - May need separate connectors or version detection

---

## Migration Recommendations

### Decision Tree

```
┌─ Does CSiBridge have product-specific API (CSiBridgev1.dll)?
│
├─ YES → Apply Full ETABS22 Pattern
│   ├─ Create ConnectorCSIBridge26 (or version-specific)
│   ├─ Direct DLL reference
│   ├─ Direct project references
│   ├─ Direct converter instantiation
│   └─ Plug-Ins folder deployment
│
└─ NO → Apply Essential Fixes Only
    ├─ Keep CSiAPIv1 NuGet package
    ├─ Add Collection for large models
    ├─ Verify Line conversion fix inheritance
    └─ Minimal architecture changes
```

### Approach 1: Essential Fixes Only (Recommended if using CSiAPIv1)

**When to use:**
- CSiBridge only has CSiAPIv1.dll (no product-specific API)
- Want to support both CSiBridge 25 and 26 with one connector
- Current connector working, just needs improvements

**Changes required:**
1. ✅ Apply Collection fix in `ConnectorBindingsCSI.Send.cs`
2. ✅ Verify Line conversion inherits 8-parameter fix from shared code
3. ✅ Add diagnostic logging
4. ✅ Test with large models (>25MB)

**Advantages:**
- Minimal changes
- Single connector for multiple versions
- Lower maintenance burden
- Faster implementation

**Disadvantages:**
- Still uses dynamic loading (potential type identity issues)
- Doesn't leverage newest API features
- AppData installation (less professional)

### Approach 2: Full ETABS22 Pattern (Recommended if CSiBridgev1.dll exists)

**When to use:**
- CSiBridge has product-specific API (CSiBridgev1.dll)
- Want to target specific version (CSiBridge 26)
- Want to match ETABS22 architecture

**Changes required:**
1. ✅ Create `ConnectorCSIBridge26` project (or version-specific)
2. ✅ Direct API DLL reference (like ETABSv1.dll)
3. ✅ Change to direct project references for Objects/Core/Converter
4. ✅ Implement direct converter instantiation with `#if CSIBRIDGE26`
5. ✅ Add build targets for DLL copying
6. ✅ Set up Plug-Ins folder deployment
7. ✅ Apply Collection fix
8. ✅ Apply Line conversion fix
9. ✅ Create version-specific build scripts

**Advantages:**
- Matches ETABS22 architecture (consistency)
- Eliminates type identity issues
- Professional Plug-Ins folder installation
- Leverages newest API features

**Disadvantages:**
- More complex build process
- Separate connectors for different versions
- Higher maintenance burden

### Approach 3: Hybrid (If API compatible between versions)

**When to use:**
- CSiBridge 25 and 26 APIs are compatible
- Want to apply fixes but keep simpler architecture

**Changes required:**
1. ✅ Keep CSiAPIv1 package but update to newer version
2. ✅ Apply Collection fix
3. ✅ Add version detection logic
4. ✅ Conditional behavior for version-specific features

---

## Implementation Checklist

### Phase 1: Investigation (Do First!)

- [ ] Check for `CSiBridgev1.dll` in CSiBridge 25 installation
- [ ] Check for `CSiBridgev1.dll` in CSiBridge 26 installation
- [ ] Compare CSiAPIv1.dll versions in detail
- [ ] Test API compatibility between versions
- [ ] Decide on approach based on findings

### Phase 2: Essential Fixes (All Approaches)

#### Large Model Support
- [ ] Update `ConnectorBindingsCSI.Send.cs`:
  ```csharp
  #if CSIBRIDGE || CSIBRIDGE25 || CSIBRIDGE26
    var commitObj = new Collection("CSI Model", "CSI");
    commitObj.elements = objects.Cast<Base>().ToList();
  #endif
  ```

#### Line Conversion Verification
- [ ] Verify `ConvertLine.cs` uses 8-parameter version:
  ```csharp
  Model.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref newFrame, "Default");
  ```
- [ ] Ensure minimum length check included
- [ ] Add diagnostic logging for CSiBridge

#### Testing
- [ ] Test with small model (<1MB)
- [ ] Test with medium model (1-25MB)
- [ ] Test with large model (>25MB)
- [ ] Test Line geometry conversion
- [ ] Test send and receive operations

### Phase 3: Full Pattern (If Applying Approach 2)

#### Project Setup
- [ ] Create `ConnectorCSIBridge26` project folder
- [ ] Copy `ConnectorCSIBridge.csproj` as starting point
- [ ] Add compiler constant: `CSIBRIDGE26`

#### API Reference
- [ ] Add direct reference to CSiBridgev1.dll or CSiAPIv1.dll:
  ```xml
  <Reference Include="CSiBridgev1">
    <HintPath>C:\Program Files\Computers and Structures\CSiBridge 26\CSiBridgev1.dll</HintPath>
    <Private>False</Private>
  </Reference>
  ```

#### Project References
- [ ] Replace Objects reference with project reference:
  ```xml
  <ProjectReference Include="..\..\Objects\Objects\Objects.csproj" />
  <ProjectReference Include="..\..\Core\Core\Core.csproj" />
  <ProjectReference Include="..\..\Objects\Converters\ConverterCSI\ConverterCSIBridge26\ConverterCSIBridge26.csproj" />
  ```

#### Converter Instantiation
- [ ] Update `ConnectorBindingsCSI.Send.cs`:
  ```csharp
  #if ETABS22 || CSIBRIDGE26
    var converter = new Objects.Converter.CSI.ConverterCSI();
  #else
    var converter = kit.LoadConverter(appName);
  #endif
  ```

#### Build Targets
- [ ] Add CopyConverterToConnectorBin target
- [ ] Add PostBuild target for Plug-Ins folder
- [ ] Update install path: `C:\ProgramData\Computers and Structures\CSiBridge 26\Plug-Ins\`

#### Create Converter Project
- [ ] Create `ConverterCSIBridge26` project
- [ ] Add compiler constant: `CSIBRIDGE26`
- [ ] Reference ConverterCSIShared
- [ ] Build and test

#### Build Scripts
- [ ] Create `build-csibridge26.bat`
- [ ] Create `build-csibridge26.ps1`
- [ ] Test automated build process

#### Documentation
- [ ] Create `CSIBRIDGE26_BUILD_INSTRUCTIONS.md`
- [ ] Document API differences from CSiBridge 25
- [ ] Update main README

### Phase 4: Testing & Validation

#### Unit Testing
- [ ] Test type identity (Model is Base? True)
- [ ] Test serialization with direct references
- [ ] Test deserialization

#### Integration Testing
- [ ] Send simple bridge model
- [ ] Send complex bridge model
- [ ] Send large model (>25MB)
- [ ] Receive model from Speckle
- [ ] Test all geometry types (especially Lines)

#### Version Testing (if supporting multiple)
- [ ] Test on CSiBridge 25
- [ ] Test on CSiBridge 26
- [ ] Verify no cross-contamination

---

## Next Steps

### Immediate Actions

1. **Check for product-specific API:**
   ```bash
   ls "C:\Program Files\Computers and Structures\CSiBridge 25\" | grep -i bridge
   ls "C:\Program Files\Computers and Structures\CSiBridge 26\" | grep -i bridge
   ```

2. **Compare API versions:**
   - Check file sizes, dates, version numbers
   - Look for version-specific documentation

3. **Choose approach** based on findings

### Implementation Timeline

**Week 1:** Investigation & Planning
- API discovery
- Approach decision
- Setup development environment

**Week 2:** Essential Fixes
- Apply Collection fix
- Verify Line conversion
- Initial testing

**Week 3:** Full Pattern (if needed)
- Create new projects
- Update references
- Build configuration

**Week 4:** Testing & Documentation
- Comprehensive testing
- Documentation updates
- Release preparation

---

## Lessons Learned from ETABS22

### Critical Insights

1. **Type Identity Matters**
   - Assembly loading must be consistent
   - Direct project references eliminate ambiguity
   - Dynamic loading can cause subtle failures

2. **Large Models Need Special Handling**
   - Collections with DetachProperty solve 25MB limit
   - Server-side limitations require client-side solutions
   - Consider model size from the start

3. **API Parameters Matter**
   - CSI APIs may have different parameter requirements
   - Always check API documentation for each version
   - Parameter counts can differ between products

4. **Build Process Complexity**
   - Copying DLLs to correct locations is critical
   - Automated scripts prevent manual errors
   - Installation location affects debugging

5. **Documentation is Essential**
   - Complex fixes need detailed documentation
   - Troubleshooting guides save time later
   - Build instructions should be step-by-step

### Best Practices

✅ **Always use direct project references** for core assemblies
✅ **Always use Collections** for large model support
✅ **Always add diagnostic logging** for troubleshooting
✅ **Always create build scripts** for reproducibility
✅ **Always document** API-specific quirks

❌ **Never use AppData DLL references** in connector projects
❌ **Never assume** API parameters are the same across versions
❌ **Never skip** large model testing
❌ **Never deploy** without automated build process

---

## Comparison Table: All Approaches

| Feature | Current CSiBridge | Approach 1: Essential | Approach 2: Full Pattern | Approach 3: Hybrid |
|---------|------------------|---------------------|------------------------|-------------------|
| **API** | CSiAPIv1 NuGet | CSiAPIv1 NuGet | CSiBridgev1.dll Direct | CSiAPIv1 Package (updated) |
| **Objects Ref** | Shared/NuGet | Shared/NuGet | Project Reference | Project Reference |
| **Converter Load** | Dynamic | Dynamic | Direct | Dynamic with fallback |
| **Large Models** | ❌ No | ✅ Yes | ✅ Yes | ✅ Yes |
| **Type Identity** | ⚠️ Risk | ⚠️ Risk | ✅ Guaranteed | ✅ Guaranteed |
| **Install Path** | AppData | AppData | ProgramData Plug-Ins | AppData or Plug-Ins |
| **Build Complexity** | Low | Low | High | Medium |
| **Maintenance** | Low | Low | High | Medium |
| **Version Support** | Multiple | Multiple | Single | Multiple |
| **Implementation Time** | - | 1-2 weeks | 3-4 weeks | 2-3 weeks |

---

## References

### Files to Review
- `ConnectorCSI/ConnectorETABS22/ConnectorETABS22.csproj` - Full ETABS22 pattern
- `ConnectorCSI/ConnectorBindingsCSI/UI/ConnectorBindingsCSI.Send.cs` - Collection implementation
- `Objects/Converters/ConverterCSI/ConverterCSIShared/PartialClasses/Geometry/ConvertLine.cs` - Line conversion fix
- `ETABS22_SERIALIZATION_FIX.md` - Type identity solution
- `SEND_LARGE_MODELS.md` - Large model handling

### Git Commits to Study
- `042a9e0` - Main ETABS22 merge
- `654ff23` - Collection for large models
- `d0f83a4` - Line conversion fix
- `14284ed` - Type registration
- `a9972fc` - KitManager initialization

---

## Summary

The ETABS22 connector work successfully addressed three critical issues that will inform CSiBridge connector updates. The key decision point is whether CSiBridge has a product-specific API like ETABS22's ETABSv1.dll. If yes, follow the full ETABS22 pattern; if no, apply essential fixes (especially Collection for large models) while maintaining the simpler architecture. Both approaches will significantly improve the CSiBridge connector's reliability and capability.

**Recommended first step:** Check for product-specific API DLLs in CSiBridge installations to inform the implementation approach.
