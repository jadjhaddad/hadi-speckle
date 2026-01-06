# ETABS22 Pattern - Quick Reference Guide

## Key File Locations

### Connector Projects
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/ConnectorCSI/ConnectorETABS22/ConnectorETABS22.csproj` - ETABS22 connector (net48)
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/ConnectorCSI/ConnectorCSIBridge25/ConnectorCSIBridge25.csproj` - CSIBridge 25 connector
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/ConnectorCSI/ConnectorCSIBridge26/ConnectorCSIBridge26.csproj` - CSIBridge 26 connector
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/ConnectorCSI/ConnectorCSIShared/ConnectorCSIShared.projitems` - SHARED connector code

### Converter Projects
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/Objects/Converters/ConverterCSI/ConverterETABS22/ConverterETABS22.csproj` - ETABS22 converter (netstandard2.0)
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/Objects/Converters/ConverterCSI/ConverterCSIBridge25/ConverterCSIBridge25.csproj` - CSIBridge 25 converter
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/Objects/Converters/ConverterCSI/ConverterCSIBridge26/ConverterCSIBridge26.csproj` - CSIBridge 26 converter
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/Objects/Converters/ConverterCSI/ConverterCSIShared/ConverterCSIShared.projitems` - SHARED converter code

### Main Implementation Files
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/Objects/Converters/ConverterCSI/ConverterCSIShared/ConverterCSI.cs` - Core converter class (uses conditional compilation)
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/Objects/Converters/ConverterCSI/ConverterETABS22/Class1.cs` - ETABS22 stub class (inherits from ConverterCSI)
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/ConnectorCSI/ConnectorCSIShared/UI/ConnectorBindingsCSI.Send.cs` - Send logic (uses direct instantiation)
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/ConnectorCSI/ConnectorCSIShared/UI/ConnectorBindingsCSI.Recieve.cs` - Receive logic (initializes KitManager)
- `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/ConnectorCSI/ConnectorCSIShared/cPlugin.cs` - Plugin entry point

## The Pattern in One Picture

```
┌─────────────────────────────────────────────────────────────┐
│                    BUILD PROCESS                             │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  1. Build ConverterETABS22.csproj (netstandard2.0)           │
│     - Imports ConverterCSIShared.projitems                   │
│     - DefineConstants: TRACE;ETABS;ETABS22                  │
│     - References ETABSv1.dll directly                        │
│     - Output: Objects.Converter.ETABS22.dll                  │
│     - Post-build: Copy to ETABS 22 Plug-Ins folder          │
│                                                               │
│  2. Build ConnectorETABS22.csproj (net48)                    │
│     - Imports ConnectorCSIShared.projitems                   │
│     - DefineConstants: DEBUG;TRACE;ETABS;ETABS22            │
│     - ProjectReference to ConverterETABS22                   │
│     - Post-build: Copy converter DLL to output               │
│     - Post-build: Copy all DLLs to ETABS 22 Plug-Ins        │
│                                                               │
│  3. Result in Plug-Ins folder:                               │
│     - SpeckleConnectorCSI.dll (connector)                    │
│     - Objects.Converter.ETABS22.dll (converter)              │
│     - Objects.dll                                            │
│     - Core.dll                                               │
│     - ... all dependencies                                   │
│                                                               │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   RUNTIME INSTANTIATION                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  In ConnectorBindingsCSI.Send.cs:                            │
│                                                               │
│  #if ETABS22 || CSIBRIDGE26 || CSIBRIDGE25                   │
│      var converter = new Objects.Converter.CSI.ConverterCSI()│
│  #else                                                        │
│      var converter = kit.LoadConverter(appName)             │
│  #endif                                                      │
│                                                               │
│  KEY: Direct instantiation = type-safe, no dynamic loading   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

## Critical Differences from Legacy ETABS

| Feature | ETABS22 | Legacy ETABS |
|---------|---------|--------------|
| Defines | `ETABS;ETABS22` | `ETABS` |
| API Ref | Direct DLL (ETABSv1.dll) | NuGet Package |
| Converter | Shared via .projitems | Separate assembly |
| Instantiation | Direct: `new ConverterCSI()` | Dynamic: `LoadConverter()` |
| Type Safety | Compile-time | Runtime |
| Deployment | MSBuild targets | xcopy post-build |

## What Makes CSIBridge25/26 Work

They already follow the ETABS22 pattern:

1. **ConnectorCSIBridge25.csproj**
   - DefineConstants: `CSIBRIDGE;CSIBRIDGE25`
   - Imports ConnectorCSIShared.projitems
   - ProjectReference to ConverterCSIBridge25
   - Direct instantiation in code

2. **ConverterCSIBridge25.csproj**
   - DefineConstants: `CSIBRIDGE;CSIBRIDGE25`
   - Imports ConverterCSIShared.projitems
   - References CSiBridge1.dll directly

3. **Shared code**
   - Single ConverterCSI.cs with conditionals
   - One cPlugin.cs with conditionals
   - One ConnectorBindingsCSI.cs with conditionals

## Verification Checklist

For CSIBridge25/26 to work correctly:

- [ ] ConnectorCSIBridge25.csproj has `CSIBRIDGE;CSIBRIDGE25` defines
- [ ] ConverterCSIBridge25.csproj has `CSIBRIDGE;CSIBRIDGE25` defines
- [ ] Both projects use ProjectReference (not dynamic loading)
- [ ] CSiBridge API DLL paths are correct (HintPath)
- [ ] MSBuild targets copy DLLs to Plug-Ins folder
- [ ] ConverterCSIBridge25 inherits from ConverterCSI
- [ ] Code uses `#if CSIBRIDGE` conditionals where needed
- [ ] KitManager initialized before deserialization (in Receive)

## Absolute Requirements

1. **MUST have version-specific defines in BOTH projects**
   ```xml
   <DefineConstants>CSIBRIDGE;CSIBRIDGE25</DefineConstants>
   ```

2. **MUST use direct instantiation, not LoadConverter()**
   ```csharp
   var converter = new Objects.Converter.CSI.ConverterCSI();
   ```

3. **MUST reference CSI API DLLs directly**
   ```xml
   <Reference Include="CSiBridge1">
     <HintPath>C:\Program Files\Computers and Structures\CSiBridge 25\CSiBridge1.dll</HintPath>
     <Private>False</Private>
   </Reference>
   ```

4. **MUST import shared .projitems files**
   ```xml
   <Import Project="..\ConnectorCSIShared\ConnectorCSIShared.projitems" Label="Shared" />
   <Import Project="..\ConverterCSIShared\ConverterCSIShared.projitems" Label="Shared" />
   ```

5. **MUST initialize KitManager in Receive path**
   ```csharp
   var objectsKit = new Objects.ObjectsKit();
   var kits = KitManager.Kits;  // Triggers initialization
   ```

## Conditional Compilation Summary

The code uses these defines to differentiate behavior:

- `ETABS22` - ETABS 22 specific (uses ETABSv1.dll)
- `CSIBRIDGE` - Generic CSIBridge code
- `CSIBRIDGE25` - CSIBridge 25 specific
- `CSIBRIDGE26` - CSIBridge 26 specific
- `ETABS` - Legacy ETABS code

In shared files:
```csharp
#if ETABS22
  using ETABSv1;  // DIFFERENT NAMESPACE!
#else
  using CSiAPIv1;
#endif
```

This is why the defines are so critical - the API namespaces are actually different!

## For CSIBridge: Special Considerations

From cPlugin.cs:
```csharp
#if CSIBRIDGE
  // CSIBridge: Disable GPU rendering to avoid conflicts
  .With(new Win32PlatformOptions { AllowEglInitialization = false, ... })
#else
  // ETABS and others: Use standard GPU rendering
  .With(new Win32PlatformOptions { AllowEglInitialization = true, ... })
#endif
```

CSIBridge has different rendering requirements and the code handles this.

---

**For detailed analysis, see: `/mnt/c/Users/jjhaddad/source/repos/speckle-sharp-main/ETABS22_PATTERN_ANALYSIS.md`**
