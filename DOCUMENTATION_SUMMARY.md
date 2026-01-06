# Speckle Objects & Converters - Comprehensive Documentation

## Files Generated

This comprehensive analysis includes two detailed documentation files:

### 1. **OBJECTS.md** (1,200 lines)
Complete reference for the Objects kit - the core interoperability layer.

**Contents:**
- Executive summary of Objects as the default Speckle kit
- Objects project structure (Objects.csproj, dependencies)
- Domain model organization (8 categories):
  - BuiltElements (37 types + app-specific variants)
  - Geometry (20+ primitive types with ICurve, ITransformable interfaces)
  - Structural Analysis (comprehensive analysis objects)
  - Organization, GIS, Other, Primitive types
- ObjectsKit class - type enumeration and converter discovery
- Converter directory overview (60+ projects)
- ISpeckleConverter interface and implementation patterns
- Bidirectional conversion (ToNative vs ToSpeckle)
- Converter discovery mechanism at runtime
- **Step-by-step guide to adding new object types** with code examples
- Common patterns checklist

**Key Insights:**
- Objects supports 200+ object types across all disciplines
- Converters are dynamically loaded based on DLL naming convention
- Version compatibility validated by Major.Minor version matching
- Factory pattern used for discovering converters at runtime

---

### 2. **CONVERTERS.md** (1,210 lines)
Complete guide to implementing and extending converters.

**Contents:**
- Quick reference table (10 converter families, 60+ projects)
- Converter project structure patterns:
  - Shared code pattern using .projitems
  - Project file template with conditional compilation
  - Version-specific wrapper pattern
- **Detailed Revit converter example:**
  - ConverterRevit.cs main class structure
  - 80+ partial classes for element type conversions
  - ConverterRevit architecture (6 versions: 2020-2025)
  - Full example: ConvertBeam.cs (ToNative and ToSpeckle)
  - Extension pattern with ParameterExtensions
- **Detailed CSI converter example:**
  - ConverterCSI.cs for SAP2000/ETABS/CSiBridge/SAFE
  - Conditional compilation for 8 CSI projects
  - Element1D conversion pattern (structural analysis)
- Rhino/Grasshopper converter overview
- Testing pattern and examples
- Common pitfalls and solutions
- Quick start guide for new converters
- Conditional compilation symbols reference

**Key Insights:**
- Most converters use shared code via .projitems to avoid duplication
- Partial classes organize conversion logic by element type
- Conditional compilation enables multi-version support from single codebase
- ISpeckleConverter interface enforces consistent API across all converters

---

## Core Findings

### Objects Kit Architecture

```
ObjectsKit (ISpeckleKit)
├── Types (200+)
│   ├── BuiltElements (Architecture: Beam, Wall, Floor, etc.)
│   ├── Geometry (Primitives: Point, Line, Mesh, Brep, etc.)
│   ├── Structural (Analysis: Model, Element1D/2D, Node, Load, etc.)
│   ├── Organization (Document, ProjectInfo, Collection)
│   ├── Other (Material, Dimension, Block, Instance, etc.)
│   ├── GIS (Spatial/Coordinate data)
│   └── Primitive (Interval, Chunk)
│
└── Converters (Discovery & Loading)
    ├── Revit (2020-2025)
    ├── CSI Suite (ETABS, SAP2000, Bridge, SAFE)
    ├── Rhino/Grasshopper (6, 7, 8)
    ├── AutoCAD/Civil3D (2021-2025)
    ├── Dynamo
    ├── DXF (Geometry only)
    ├── Tekla
    ├── Navisworks
    └── Bentley (4 variants)
```

### Converter Implementation Pattern

```csharp
public class Converter{App}{Version} : ISpeckleConverter
{
  // Identification
  public string Name { get; }
  public IEnumerable<string> GetServicedApplications()
  
  // Context
  public void SetContextDocument(object doc)
  public void SetContextObjects(List<ApplicationObject> objects)
  public void SetConverterSettings(object settings)
  
  // Bidirectional Conversion
  public Base ConvertToSpeckle(object native)          // Import
  public object ConvertToNative(Base speckle)          // Export
  
  // Capability Checks (Routing)
  public bool CanConvertToSpeckle(object obj)
  public bool CanConvertToNative(Base obj)
}
```

### Key Patterns

#### 1. **Shared Code Pattern** (Reduces Duplication)
```
Converter{App}Shared/         ← Single implementation
├── Converter{App}.cs         ← 80+ partial files
├── PartialClasses/
├── Extensions/
└── Models/

Converter{App}2024/           ← Thin wrapper
├── ConverterRevit2024.csproj
└── Imports .projitems
```

#### 2. **Version Selection** (Conditional Compilation)
```csharp
#if REVIT2025
  public static string AppName = "Revit2025";
#elif REVIT2024
  public static string AppName = "Revit2024";
#endif
```

#### 3. **Converter Discovery** (Runtime Loading)
```
File Scanning:
  Objects.Converter.*.dll
  
Version Validation:
  DLL Version Major.Minor == Objects Version Major.Minor
  
Instantiation:
  Assembly.Load() → GetTypes() → ISpeckleConverter
```

#### 4. **Object Extension** (Adding New Types)
```
Step 1: Define class in Objects/{Category}/NewType.cs
Step 2: Create app-specific subclass in Objects/{Category}/{App}/
Step 3: Add ToSpeckle conversion in Converter partial class
Step 4: Add ToNative conversion in Converter partial class
Step 5: Update CanConvert methods
Step 6: Register in main switch statements
```

---

## CSIBridge Connector Context

Based on your current work on the CSIBridge connector (feature/csibridge-connector):

### CSI Converter Structure (Relevant to CSIBridge25/26)

**Location:** `/Objects/Converters/ConverterCSI/`

**Projects:**
- `ConverterCSIShared/` - Shared implementation (57 files)
- `ConverterCSIBridge/` - Base implementation
- `ConverterCSIBridge25/` - v25 specific (Your current work)
- `ConverterCSIBridge26/` - v26 specific

**Key Objects for Bridge Analysis:**
- `CSIElement1D.cs` - Frame/beam elements
- `CSIElement2D.cs` - Area/plate elements
- `CSINode.cs` - Structural nodes
- `CSIGridLines.cs` - ETABS/Bridge grids
- `CSIPier.cs`, `CSISpandrel.cs` - Bridge-specific elements
- `CSITendon.cs` - Prestressing/tendons
- `CSIStories.cs` - Story/deck definitions

**Conversion Pattern:**
```
CSI API (cSapModel)
  ↓
ConverterCSI.ConvertToSpeckle()
  ↓
Speckle Structural Objects (Element1D, Element2D, Node, etc.)
  ↓
Speckle Viewer/API
```

**Common CSIBridge Operations:**
1. **Modeling** - Create bridge structure (piers, spans, tendons)
2. **Analysis** - Run finite element analysis
3. **Results** - Extract displacement, forces, stresses
4. **Exchange** - Send/receive via Speckle

---

## Quick Reference - File Locations

### Core Objects
- Objects definition: `/Objects/Objects/Objects.csproj`
- ObjectsKit: `/Objects/Objects/ObjectsKit.cs`
- Interfaces: `/Objects/Objects/Interfaces.cs`

### Domain Models
- BuiltElements: `/Objects/Objects/BuiltElements/`
- Geometry: `/Objects/Objects/Geometry/`
- Structural: `/Objects/Objects/Structural/`
- CSI-specific: `/Objects/Objects/Structural/CSI/`

### Converters
- Revit: `/Objects/Converters/ConverterRevit/`
- CSI: `/Objects/Converters/ConverterCSI/`
- Rhino: `/Objects/Converters/ConverterRhinoGh/`
- AutoCAD: `/Objects/Converters/ConverterAutocadCivil/`

### Tests
- Unit tests: `/Objects/Tests/Objects.Tests.Unit/`
- Revit tests: `/Objects/Converters/ConverterRevit/ConverterRevitTests/`

---

## Documentation Structure

### OBJECTS.md
For understanding:
- What objects exist and how they're organized
- How the ObjectsKit works
- How to add new object types
- The ISpeckleConverter interface
- How objects are serialized and transmitted

**Best for:** Domain modeling, schema design, adding new object categories

### CONVERTERS.md
For understanding:
- How converters implement ISpeckleConverter
- Converter project organization patterns
- Real examples: Revit, CSI, Rhino
- How conversion logic is implemented
- Testing patterns
- Adding new converters or converters versions

**Best for:** Converter development, new app integration, extending functionality

---

## Next Steps for CSIBridge Connector

With this documentation, you can:

1. **Understand the conversion pipeline:**
   - CONVERTERS.md Section 3: CSI Converter pattern
   - OBJECTS.md Section 6: ToNative vs ToSpeckle patterns

2. **Add new CSI object types:**
   - OBJECTS.md Section 8: Step-by-step guide
   - Follow CSI-specific objects in Structural/CSI subdirectory

3. **Debug converter loading:**
   - OBJECTS.md Section 7: Converter discovery mechanism
   - Check Objects.csproj and ObjectsKit.cs

4. **Implement bridge-specific conversions:**
   - CONVERTERS.md Section 3: CSI Element1D example
   - Look at CSIPier.cs, CSISpandrel.cs patterns

5. **Add CSIBridge25/26 specific features:**
   - CONVERTERS.md Section 1.2: Conditional compilation pattern
   - Update ConverterCSI with #if CSIBRIDGE25 blocks

---

## Statistics

- **Total Objects:** 200+ across all disciplines
- **Total Converter Projects:** 60 (across 10 applications)
- **Revit Converter Files:** 80+ partial classes
- **CSI Shared Converter Files:** 57 files
- **Documentation Lines:** 2,410 (combined)
- **Code Examples:** 15+ complete examples

---

Generated: 2025-11-06
Source: Comprehensive analysis of `/Objects/` directory
Scope: Objects project structure, domain models, converter patterns, and extension guide
