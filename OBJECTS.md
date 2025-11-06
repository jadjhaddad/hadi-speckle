# OBJECTS - THE SPECKLE INTEROPERABILITY KIT

## Executive Summary

**Objects** is the default and core interoperability kit for Speckle, providing a unified object model that enables seamless data exchange across 30+ BIM, structural, civil, and CAD applications. It contains:

- **Core domain models** (200+ object types) organized by discipline
- **Version-specific converters** (60+ projects) for applications like Revit, SAP2000, ETABS, Rhino, AutoCAD, etc.
- **Converter discovery system** that dynamically loads version-specific converters at runtime
- **Bidirectional conversion** (ToNative/ToSpeckle) patterns for each application

Objects is netstandard2.0 compliant and serves as the foundation of all Speckle connectors.

---

## 1. OBJECTS PROJECT STRUCTURE

### Project File Location
```
/Objects/Objects/Objects.csproj
```

### Key Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <RootNamespace>Objects</RootNamespace>
    <AssemblyName>Objects</AssemblyName>
    <PackageId>Speckle.Objects</PackageId>
    <Description>Objects is the default object model for Speckle</Description>
    <Nullable>enable</Nullable>
  </PropertyGroup>
```

### Core Files
- **ObjectsKit.cs** - ISpeckleKit implementation, converter loading/discovery
- **Interfaces.cs** - Standard object interfaces (IDisplayValue, IHasBoundingBox, IHasArea, etc.)
- **EncodingOptimisations.cs** - Geometric encoding utilities

### Dependencies
- Speckle.Core (Core libraries)
- NetTopologySuite 2.5.0 (Spatial operations)
- Nullable reference types enabled

---

## 2. DOMAIN MODEL ORGANIZATION

Objects directory contains hierarchical, discipline-specific folders:

### 2.1 BuiltElements (Architecture/BIM)
**Location:** `/Objects/Objects/BuiltElements/`

Core architectural objects:
- **Alignment.cs** - Alignment definition
- **Beam.cs** - Structural beams with baseLine and displayValue
- **Column.cs** - Structural columns with baseLine
- **Wall.cs** - Walls with height, baseLine, and nested elements
- **Floor.cs** - Floors with outline, voids, and nested elements
- **Level.cs** - Levels with name and elevation
- **Ceiling.cs** - Ceiling slabs
- **Roof.cs** - Roof surfaces
- **Door.cs**, **Window.cs** - Fenestration objects
- **Stair.cs**, **Railing.cs** - Vertical circulation
- **Room.cs**, **Zone.cs** - Space definitions
- **Area.cs** - Area calculations
- **Baseline.cs**, **GridLine.cs** - Reference geometry
- **MEP Objects:** Pipe, Duct, Conduit, CableTray
- **Opening.cs** - Wall/floor openings

**Application-Specific Subdirectories:**
- `Revit/` - RevitBeam, RevitColumn, RevitWall, RevitFloor (Revit-specific properties)
- `Civil/` - CivilCurve, CivilAlignment, CivilSurface (Civil3D objects)
- `AdvanceSteel/` - AsteelBeam, AsteelPlate, AsteelBolt, etc.
- `Archicad/` - ArchicadBeam, ArchicadColumn, ArchicadWall, etc.
- `TeklaStructures/` - TeklaBeam, TeklaColumn, etc.

### 2.2 Geometry (Primitives)
**Location:** `/Objects/Objects/Geometry/`

Core geometric types:
- **Point.cs** - 3D point with x, y, z, units (ITransformable<Point>)
- **Vector.cs** - 3D vector with magnitude and direction
- **Line.cs** - Infinite line from Point to Point (ICurve)
- **Polyline.cs** - Sequence of connected line segments
- **Arc.cs** - Circular arc
- **Circle.cs** - Circular boundary
- **Ellipse.cs** - Elliptical curve
- **Curve.cs** - General NURBS curve
- **Polycurve.cs** - Composite curve (multiple curve segments)
- **Mesh.cs** - Triangulated surface (vertices, faces, colors)
- **Brep.cs** - Boundary representation (faces, edges, trims, loops)
- **BrepFace.cs**, **BrepEdge.cs**, **BrepTrim.cs**, **BrepLoop.cs** - Brep components
- **Plane.cs** - Planar surface with origin and normal
- **Box.cs** - Axis-aligned bounding box
- **Extrusion.cs** - Extruded surface
- **Pointcloud.cs** - Collection of points
- **Transform.cs** - 4x4 transformation matrix

**Key Interfaces:**
- `ICurve` - length, domain, units (Polyline, Arc, Curve, Line, Polycurve)
- `ITransformable<T>` - TransformTo() for coordinate transformation
- `IHasBoundingBox` - bbox property
- `IHasArea` - area property
- `IHasVolume` - volume property

### 2.3 Structural Analysis
**Location:** `/Objects/Objects/Structural/`

Analysis and structural design:
- **Analysis/Model.cs** - Full structural analysis model
- **Analysis/ModelInfo.cs** - Model metadata (author, date, name)
- **Analysis/ModelSettings.cs** - Model settings (analysis type, units)
- **Analysis/UnitTypes.cs** - Unit definitions (force, length, temperature)
- **Axis.cs** - Coordinate system definition
- **Geometry/Node.cs** - Structural node/joint
- **Geometry/Element1D.cs** - Line elements (beams, columns, braces)
- **Geometry/Element2D.cs** - Surface elements (plates, walls, floors)
- **Geometry/Vector3.cs** - Load vectors
- **Loading/Load.cs** - Applied loads
- **Loading/LoadCase.cs** - Load cases and combinations
- **Loading/LoadPattern.cs** - Load patterns
- **Materials/StructuralMaterial.cs** - Material properties (E, G, density)
- **Materials/Steel.cs** - Steel material definition
- **Materials/Concrete.cs** - Concrete material definition
- **Properties/Property.cs** - Cross-section properties
- **Properties/Profiles/** - Section profiles (ISection, ChannelProfile, etc.)
- **Results/Result.cs** - Analysis results
- **Results/NodeResult.cs** - Node displacements/reactions
- **Results/Element1DResult.cs** - Beam internal forces
- **Results/Element2DResult.cs** - Plate stresses

**CSI-Specific Subdirectory** (`Structural/CSI/`):
- **CSIElement1D.cs** - SAP2000/ETABS line elements
- **CSIElement2D.cs** - CSI area elements
- **CSINode.cs** - CSI nodes with coordinates
- **CSIGridLines.cs** - ETABS grid definitions
- **CSIPier.cs**, **CSISpandrel.cs** - Bridge elements
- **CSITendon.cs** - Prestressing tendons
- **CSIStories.cs** - Story definitions
- **CSIConcrete.cs**, **CSIRebar.cs** - CSI material types

### 2.4 Organization & Spatial
**Location:** `/Objects/Objects/Organization/`

Project structure:
- **Document.cs** - Project/file wrapper
- **ProjectInfo.cs** - Project metadata
- **Collection.cs** - Named groupings

### 2.5 Other (Utilities/Generic)
**Location:** `/Objects/Objects/Other/`

Utility objects:
- **Block.cs** - Reusable block definition
- **Instance.cs** - Block instance
- **Material.cs** - Material definition with name, color, texture
- **MaterialQuantity.cs** - Material quantities
- **DataField.cs** - Generic key-value pairs
- **Dimension.cs** - Dimension annotations
- **DisplayStyle.cs** - Visual styling properties
- **Hatch.cs** - Hatch patterns
- **CivilDataField.cs**, **Civil/** - Civil-specific utilities

### 2.6 GIS (Geographic Information)
**Location:** `/Objects/Objects/GIS/`

Geospatial objects:
- **GIS primitives** for coordinate systems and spatial data

### 2.7 Primitive Types
**Location:** `/Objects/Objects/Primitive/`

Low-level primitives:
- **Interval.cs** - 1D range [start, end]
- **Interval2d.cs** - 2D parametric range
- **Chunk.cs** - Serialization unit

---

## 3. OBJECTS KIT - THE DEFAULT INTEROPERABILITY KIT

### What Makes Objects "Default"?

**File:** `/Objects/Objects/ObjectsKit.cs`

```csharp
public class ObjectsKit : ISpeckleKit
{
  public string Description => "The default Speckle Kit.";
  public string Name => "Objects";
  public string Author => "Speckle";
  
  // All object types in the assembly (200+ types)
  public IEnumerable<Type> Types =>
    Assembly.GetExecutingAssembly()
      .GetTypes()
      .Where(t => t.IsSubclassOf(typeof(Base)) && !t.IsAbstract);
  
  // Available converters discovered at runtime
  public IEnumerable<string> Converters => GetAvailableConverters();
}
```

### Key Responsibilities

1. **Type Enumeration** - Exposes all object types from assembly reflection
2. **Converter Loading** - Dynamically loads version-specific converters
3. **Converter Discovery** - Scans disk for available converter DLLs
4. **Version Validation** - Ensures converter versions match Objects version

### Converter Discovery Pattern

```csharp
public ISpeckleConverter LoadConverter(string app)
{
  // 1. Check if already loaded
  if (_loadedConverters.TryGetValue(app, out Type t))
  {
    return (ISpeckleConverter)Activator.CreateInstance(t);
  }
  
  // 2. Load from disk
  var path = Path.Combine(ObjectsFolder, $"Objects.Converter.{app}.dll");
  
  // 3. Validate version compatibility (Major.Minor must match Objects)
  AssemblyName assemblyToLoad = AssemblyName.GetAssemblyName(path);
  AssemblyName objects = Assembly.GetExecutingAssembly().GetName();
  
  if (assemblyToLoad.Version.Major != objects.Version.Major ||
      assemblyToLoad.Version.Minor != objects.Version.Minor)
  {
    throw new SpeckleException(
      $"Mismatch between Objects v{objects.Version} and Converter v{assemblyToLoad.Version}");
  }
  
  // 4. Instantiate and return
  return assembly.GetTypes()
    .Where(type => typeof(ISpeckleConverter).IsAssignableFrom(type))
    .Select(type => (ISpeckleConverter)Activator.CreateInstance(type))
    .FirstOrDefault(converter => converter.GetServicedApplications().Contains(app));
}
```

### Installation Paths

```
User Installation:  C:\Users\USERNAME\AppData\Roaming\Speckle\Kits\Objects
System-wide:        C:\ProgramData\Speckle\Kits\Objects
```

---

## 4. CONVERTERS DIRECTORY - 60+ PROJECTS

**Location:** `/Objects/Converters/`

### 4.1 Converter Organization by Application

#### Revit (10 projects)
- `ConverterRevit2020/` through `ConverterRevit2025/` (6 versions)
- `ConverterRevit2021/`, `ConverterRevit2022/`, `ConverterRevit2023/`, `ConverterRevit2024/`, `ConverterRevit2025/`
- Shared code: `ConverterRevitShared/` (partial class files)
- Test projects: `ConverterRevitTests/`

#### CSI Suite (8 projects)
- `ConverterETABS/`, `ConverterETABS22/`
- `ConverterSAP2000/`, `ConverterSAP2000-26/`
- `ConverterCSIBridge/`, `ConverterCSIBridge25/`, `ConverterCSIBridge26/`
- `ConverterSAFE/`
- Shared code: `ConverterCSIShared/`

#### Rhino/Grasshopper (6 projects)
- `ConverterRhino6/`, `ConverterRhino7/`, `ConverterRhino8/`
- `ConverterGrasshopper6/`, `ConverterGrasshopper7/`, `ConverterGrasshopper8/`
- Shared code: `ConverterRhinoGhShared/`

#### Dynamo (6 projects)
- `ConverterDynamoRevit/`, `ConverterDynamoRevit2021/` through `ConverterDynamoRevit2024/`
- `ConverterDynamoSandbox/`

#### AutoCAD/Civil3D (14 projects)
- **AutoCAD:** 2021, 2022, 2023, 2024, 2025 versions
- **Civil3D:** 2021, 2022, 2023, 2024, 2025 versions
- **Advance Steel:** 2023, 2024 versions

#### Bentley (4 projects)
- `ConverterMicroStation/`
- `ConverterOpenBuildings/`
- `ConverterOpenRail/`
- `ConverterOpenRoads/`

#### Tekla Structures (4 projects)
- `ConverterTeklaStructures2020/` through `ConverterTeklaStructures2023/`

#### Navisworks (6 projects)
- `ConverterNavisworks2020/` through `ConverterNavisworks2025/`

#### DXF (1 project)
- `ConverterDxf/` - Basic geometry (Point, Line, Mesh, Brep, Vector)

#### Utilities (1 project)
- `PolygonMesher/` - Structural utilities for polygon meshing

### 4.2 Converter Naming Convention

```
Objects.Converter.{ApplicationName}.dll

Examples:
- Objects.Converter.Revit2024.dll
- Objects.Converter.ETABS.dll
- Objects.Converter.SAP2000.dll
- Objects.Converter.Rhino8.dll
```

---

## 5. CONVERTER PATTERN - IMPLEMENTING ISpeckleConverter

### 5.1 Core Interface Definition

All converters implement `ISpeckleConverter`:

```csharp
public interface ISpeckleConverter
{
  // Metadata
  string Name { get; }
  string Description { get; }
  string Author { get; }
  string WebsiteOrEmail { get; }
  
  // Settings and context
  Dictionary<string, string> Settings { get; set; }
  ProgressReport Report { get; set; }
  ReceiveMode ReceiveMode { get; set; }
  
  // Required methods
  IEnumerable<string> GetServicedApplications();
  void SetContextDocument(object doc);
  void SetContextObjects(List<ApplicationObject> objects);
  void SetConverterSettings(object settings);
  
  // Single object conversion
  Base ConvertToSpeckle(object @object);
  object ConvertToNative(Base @base);
  
  // Batch conversion
  List<Base> ConvertToSpeckle(List<object> objects);
  List<object> ConvertToNative(List<Base> objects);
  
  // Capability checks
  bool CanConvertToSpeckle(object @object);
  bool CanConvertToNative(Base @base);
  bool CanConvertToNativeDisplayable(Base @base);
  
  // Display-only conversion
  object ConvertToNativeDisplayable(Base @base);
}
```

### 5.2 Example: ConverterRevit Implementation

**File:** `/Objects/Converters/ConverterRevit/ConverterRevitShared/ConverterRevit.cs`

```csharp
public partial class ConverterRevit : ISpeckleConverter
{
  // Application identification via conditional compilation
#if REVIT2025
  public static string RevitAppName = HostApplications.Revit.GetVersion(HostAppVersion.v2025);
#elif REVIT2024
  public static string RevitAppName = HostApplications.Revit.GetVersion(HostAppVersion.v2024);
  // ... etc for other versions
#endif

  public string Description => "Default Speckle Kit for Revit";
  public string Name => nameof(ConverterRevit);
  public string Author => "Speckle";
  public string WebsiteOrEmail => "https://speckle.systems";

  // List of Revit versions this converter supports
  public IEnumerable<string> GetServicedApplications() => new[] { RevitAppName };

  // Context storage
  public Document Doc { get; private set; }
  public Dictionary<string, ApplicationObject> ContextObjects { get; set; }
  public IReceivedObjectIdMap<Base, Element> PreviouslyReceivedObjectIds { get; set; }
  public Dictionary<string, BE.Level> Levels { get; private set; }
  public ProgressReport Report { get; private set; }
  public Dictionary<string, string> Settings { get; private set; }
  public ReceiveMode ReceiveMode { get; set; }

  public void SetContextDocument(object doc)
  {
    if (doc is Document document)
    {
      Doc = document;
      Report.Log($"Using document: {Doc.PathName}");
    }
  }

  public Base ConvertToSpeckle(object @object)
  {
    switch (@object)
    {
      case DB.Document o:
        return ModelToSpeckle(o);
      case DB.Floor o:
        return FloorToSpeckle(o);
      case DB.Wall o:
        return WallToSpeckle(o);
      case DB.Beam o:
        return BeamToSpeckle(o);
      // ... more types
      default:
        return null;
    }
  }

  public object ConvertToNative(Base @base)
  {
    switch (@base)
    {
      case Floor floor:
        return FloorToNative(floor);
      case Wall wall:
        return WallToNative(wall);
      case Beam beam:
        return BeamToNative(beam);
      // ... more types
      default:
        return null;
    }
  }

  public bool CanConvertToSpeckle(object @object)
  {
    return @object is Document or Floor or Wall or Beam or //...
  }

  public bool CanConvertToNative(Base @base)
  {
    return @base is Floor or Wall or Beam or //...
  }
}
```

### 5.3 Example: ConverterCSI Implementation

**File:** `/Objects/Converters/ConverterCSI/ConverterCSIShared/ConverterCSI.cs`

```csharp
public partial class ConverterCSI : ISpeckleConverter, IFinalizable
{
  // Conditional compilation for different CSI applications
#if ETABS
  public static string CSIAppName = HostApplications.ETABS.Name;
#elif SAP2000
  public static string CSIAppName = HostApplications.SAP2000.Name;
#elif CSIBRIDGE
  public static string CSIAppName = HostApplications.CSiBridge.Name;
#elif SAFE
  public static string CSIAppName = HostApplications.SAFE.Name;
#endif

  public string Name => nameof(ConverterCSI);
  public IEnumerable<string> GetServicedApplications() => new[] { CSIAppName };

  // CSI-specific context
  public cSapModel Model { get; private set; }
  public Model SpeckleModel { get; set; }
  public Dictionary<string, string> ExistingObjectGuids { get; set; }

  public void SetContextDocument(object doc)
  {
    Model = (cSapModel)doc;
    
    if (Settings["operation"] == "receive")
    {
      ExistingObjectGuids = GetAllGuids(Model);
    }
    else if (Settings["operation"] == "send")
    {
      SpeckleModel = ModelToSpeckle();
    }
  }

  public bool CanConvertToNative(Base @object)
  {
    switch (@object)
    {
      case Element2D: return true;
      case Element1D: return true;
      case Node: return true;
      case GridLine: return true;
      case Load: return true;
      default: return false;
    }
  }

  public object ConvertToNative(Base @object)
  {
    switch (@object)
    {
      case CSIAreaSpring o:
        return AreaSpringPropertyToNative(o);
      case Element2D o:
        return Element2DToNative(o);
      case Element1D o:
        return Element1DToNative(o);
      // ... more types
      default:
        return null;
    }
  }
}
```

### 5.4 Example: ConverterDxf Implementation

**File:** `/Objects/Converters/ConverterDxf/ConverterDxf/SpeckleDxfConverter.cs`

```csharp
public partial class SpeckleDxfConverter
{
  public string Name => "Speckle DXF Converter";
  public string Description => "The Objects DXF Converter";
  public IEnumerable<string> GetServicedApplications() => new[] { HostApplications.Dxf.Slug };

  // DXF is write-only (ToNative only)
  public List<Base> ConvertToSpeckle(List<object> objects) 
    => throw new NotImplementedException();

  public Base ConvertToSpeckle(object @object) 
    => throw new NotImplementedException();

  public bool CanConvertToSpeckle(object @object) 
    => throw new NotImplementedException();

  // Geometry-only conversion
  public bool CanConvertToNative(Base @base)
  {
    return @base switch
    {
      Vector _ => true,
      Point _ => true,
      Line _ => true,
      Mesh _ => true,
      Brep _ => true,
      _ => false,
    };
  }

  public object ConvertToNative(Base @base)
  {
    return @base switch
    {
      Point pt => PointToNative(pt),
      Vector vec => VectorToNative(vec),
      Line line => LineToNative(line),
      Mesh mesh => MeshToNative(mesh),
      Brep brep => BrepToNative(brep),
      _ => null,
    };
  }
}
```

---

## 6. BIDIRECTIONAL CONVERSION - ToNative vs ToSpeckle

### 6.1 Conversion Directions

| Direction | Purpose | Example |
|-----------|---------|---------|
| **ToSpeckle** | Import from native app → Speckle objects | Revit Floor → Floor |
| **ToNative** | Export from Speckle objects → native app | Floor → Revit Floor |

### 6.2 ToSpeckle Pattern - From Native to Speckle

**Example: Revit Beam to Speckle**

```csharp
// From ConverterRevit.cs
public Base BeamToSpeckle(DB.FamilyInstance revitBeam, out List<string> notes)
{
  notes = new();
  
  // 1. Extract geometry from native object
  var location = revitBeam.Location as LocationCurve;
  ICurve baseLine = CurveToSpeckle(location.Curve);
  
  // 2. Extract metadata
  string? familyName = revitBeam.Symbol?.FamilyName;
  string? typeName = revitBeam.Symbol?.Name;
  var level = LevelToSpeckle(revitBeam.Level);
  
  // 3. Create Speckle object
  var speckleBeam = new Beam
  {
    baseLine = baseLine,
    level = level,
    units = ModelUnits,
    applicationId = revitBeam.UniqueId
  };
  
  // 4. Store additional properties
  speckleBeam["revitFamily"] = familyName;
  speckleBeam["revitType"] = typeName;
  
  return speckleBeam;
}
```

### 6.3 ToNative Pattern - From Speckle to Native

**Example: Speckle Beam to Revit**

```csharp
// From ConvertBeam.cs
public ApplicationObject BeamToNative(Beam speckleBeam)
{
  var appObj = new ApplicationObject(speckleBeam.id, speckleBeam.speckle_type)
  {
    applicationId = speckleBeam.applicationId
  };

  // 1. Validate input
  if (speckleBeam.baseLine == null)
  {
    appObj.Update(status: ApplicationObject.State.Failed, 
      logItem: "Only line based Beams are currently supported.");
    return appObj;
  }

  // 2. Convert curve geometry
  var familySymbol = GetElementType<FamilySymbol>(speckleBeam, appObj);
  if (familySymbol == null) return appObj;
  
  var baseLine = CurveToNative(speckleBeam.baseLine).get_Item(0);

  // 3. Get/create level
  var level = ConvertLevelToRevit(speckleBeam.level);

  // 4. Create or update in Revit
  DB.FamilyInstance revitBeam = null;
  var docObj = GetExistingElementByApplicationId(speckleBeam.applicationId);

  if (docObj != null)
  {
    // Update existing element
    revitBeam = (DB.FamilyInstance)docObj;
    var existingCurve = (revitBeam.Location as LocationCurve).Curve;
    existingCurve = baseLine;
    
    if (revitBeam.Symbol.Id != familySymbol.Id)
      revitBeam.ChangeTypeId(familySymbol.Id);
  }
  else
  {
    // Create new element
    revitBeam = Doc.Create.NewFamilyInstance(
      baseLine, familySymbol, level, StructuralType.Beam);
  }

  // 5. Apply additional properties
  UpdateParameters(revitBeam, speckleBeam);
  
  appObj.Update(status: ApplicationObject.State.Success, 
    logItem: $"Element {revitBeam.Id} successfully created");
  appObj["revitElement"] = revitBeam;
  
  return appObj;
}
```

### 6.4 CSI Pattern - Structural Analysis

**Example: CSI Element1D (Beam)**

```csharp
// To Speckle - Extract from analysis model
public Element1D Element1DToSpeckle(string name)
{
  var element = new Element1D { name = name };
  
  // Get nodes
  int numPoints = 0;
  Model.FrameObj.GetEndPoints(name, ref points[0], ref points[1]);
  element.startNode = NodeToSpeckle(points[0]);
  element.endNode = NodeToSpeckle(points[1]);
  
  // Get section properties
  string propName = "";
  Model.FrameObj.GetProperty(name, ref propName);
  element.property = PropertyToSpeckle(propName);
  
  // Get releases/offsets
  bool[] releases = new bool[6];
  Model.FrameObj.GetReleases(name, ref releases);
  element.releases = releases;
  
  return element;
}

// To Native - Create in analysis model
public string Element1DToNative(Element1D element)
{
  // Verify nodes exist
  string startNodeName = GetOrCreateNode(element.startNode);
  string endNodeName = GetOrCreateNode(element.endNode);
  
  // Create frame object
  int ret = Model.FrameObj.SetFrame(element.name, startNodeName, endNodeName);
  
  // Assign property
  if (element.property != null)
  {
    Model.FrameObj.SetProperty(element.name, element.property.name);
  }
  
  // Set releases
  if (element.releases != null)
  {
    Model.FrameObj.SetReleases(element.name, element.releases);
  }
  
  return element.name;
}
```

---

## 7. CONVERTER DISCOVERY - RUNTIME LOADING MECHANISM

### 7.1 How Converters Are Loaded

**Flow:**
1. Application calls `kit.LoadConverter("Revit2024")`
2. ObjectsKit checks if converter already loaded in `_loadedConverters` dict
3. If not, searches disk for `Objects.Converter.Revit2024.dll`
4. Validates version compatibility (Major.Minor must match Objects)
5. Uses reflection to instantiate ISpeckleConverter type
6. Calls `GetServicedApplications()` to verify match
7. Caches instance in `_loadedConverters`

### 7.2 Converter Discovery Algorithm

**File:** `/Objects/Objects/ObjectsKit.cs`

```csharp
public List<string> GetAvailableConverters()
{
  var basePath = Path.GetDirectoryName(
    Assembly.GetExecutingAssembly().Location);
  
  // Find all Objects.Converter.*.dll files
  var allConverters = Directory.EnumerateFiles(
    basePath!, "Objects.Converter.*.dll").ToArray();

  // Fallback to kit folder
  if (allConverters.Length == 0)
  {
    allConverters = Directory.EnumerateFiles(
      ObjectsFolder, "Objects.Converter.*.dll").ToArray();
  }

  // Filter by version compatibility
  var objects = Assembly.GetExecutingAssembly().GetName();
  var availableConverters = new List<string>();
  
  foreach (var converter in allConverters)
  {
    AssemblyName assemblyName = AssemblyName.GetAssemblyName(converter);
    
    // Major.Minor version must match Objects
    if (assemblyName.Version.Major == objects.Version.Major &&
        assemblyName.Version.Minor == objects.Version.Minor)
    {
      availableConverters.Add(converter);
    }
    else
    {
      SpeckleLog.Logger.Warning(
        $"Skipped converter (version mismatch): {converter}");
    }
  }

  // Extract app names from paths
  // Objects.Converter.ETABS.dll → "ETABS"
  var finalList = availableConverters
    .Select(dllPath => dllPath.Split('.').Reverse().ElementAt(1))
    .Distinct()
    .ToList();

  return finalList;
}
```

### 7.3 Search Locations (Priority Order)

1. **Same directory as Objects.dll**
   ```
   C:\...\bin\Objects.dll
   C:\...\bin\Objects.Converter.Revit2024.dll
   ```

2. **Kit folder** (fallback)
   ```
   C:\Users\USERNAME\AppData\Roaming\Speckle\Kits\Objects\
   Objects.Converter.Revit2024.dll
   ```

### 7.4 Version Compatibility Checking

```csharp
AssemblyName objects = Assembly.GetExecutingAssembly().GetName();

// Ensures Revit2024 converter v2.x matches Objects v2.y
if (assemblyToLoad.Version.Major != objects.Version.Major ||
    assemblyToLoad.Version.Minor != objects.Version.Minor)
{
  throw new SpeckleException(
    $"Mismatch: Objects v{objects.Version} Converter v{assemblyToLoad.Version}");
}
```

**Key Point:** Build version (third number) can differ, but Major.Minor must match exactly.

---

## 8. ADDING NEW OBJECT TYPES - EXTENSION PATTERN

### 8.1 Step 1: Define the Object Class

Create a new file in appropriate directory (`/Objects/Objects/{Category}/NewType.cs`):

```csharp
using Objects.Geometry;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;

namespace Objects.BuiltElements;

/// <summary>
/// Represents a customized beam with specialized properties
/// </summary>
public class CustomBeam : Base, IDisplayValue<List<Mesh>>
{
  public CustomBeam() { }

  /// <summary>
  /// Constructor with SchemaBuilder support
  /// </summary>
  [SchemaInfo("CustomBeam", "Creates a Speckle custom beam", "BIM", "Structure")]
  public CustomBeam(
    [SchemaMainParam] ICurve baseLine,
    [SchemaParamInfo("Custom offset")] double offset = 0,
    List<Base>? elements = null
  )
  {
    this.baseLine = baseLine;
    this.offset = offset;
    this.elements = elements;
  }

  // Geometry
  public ICurve baseLine { get; set; }
  
  // Standard properties
  public Level? level { get; set; }
  public string units { get; set; }
  
  // Custom properties
  public double offset { get; set; }
  public string customProfile { get; set; }
  public bool isPrecast { get; set; }
  
  // Relationships
  [DetachProperty]
  public List<Base>? elements { get; set; }
  
  // Display geometry (for viewers)
  [DetachProperty]
  public List<Mesh> displayValue { get; set; }
}
```

### 8.2 Step 2: Implement Application-Specific Subclass (Optional)

For application-specific properties, create a subclass:

```csharp
// File: Objects/Objects/BuiltElements/Revit/RevitCustomBeam.cs
namespace Objects.BuiltElements.Revit;

public class RevitCustomBeam : CustomBeam
{
  // Revit-specific properties
  public string? revitFamily { get; set; }
  public string? revitType { get; set; }
  public string? phase { get; set; }
  
  [DetachProperty]
  public List<Parameter> parameters { get; set; } = new();
}
```

### 8.3 Step 3: Add ToSpeckle Conversion (Sending)

In converter's partial class file:

```csharp
// File: Converters/ConverterRevit/ConverterRevitShared/PartialClasses/ConvertCustomBeam.cs
namespace Objects.Converter.Revit;

public partial class ConverterRevit
{
  // Convert native Revit element to Speckle
  public RevitCustomBeam CustomBeamToSpeckle(DB.FamilyInstance revitBeam)
  {
    var location = revitBeam.Location as LocationCurve;
    var baseLine = CurveToSpeckle(location.Curve);
    
    // Create Speckle object
    var speckleBeam = new RevitCustomBeam
    {
      baseLine = baseLine,
      level = LevelToSpeckle(revitBeam.Level),
      units = ModelUnits,
      applicationId = revitBeam.UniqueId,
      
      // Custom properties
      offset = GetParameterValue(revitBeam, "Offset") ?? 0.0,
      isPrecast = GetParameterValue(revitBeam, "IsPrecast") ?? false,
      
      // Revit-specific
      revitFamily = revitBeam.Symbol.FamilyName,
      revitType = revitBeam.Symbol.Name,
    };
    
    // Extract Revit parameters
    speckleBeam.parameters = GetElementParameters(revitBeam);
    
    return speckleBeam;
  }

  public bool CanConvertCustomBeamToSpeckle(object obj) 
    => obj is DB.FamilyInstance inst && 
       inst.Symbol.FamilyName == "Custom Beam Family";
}
```

### 8.4 Step 4: Add ToNative Conversion (Receiving)

```csharp
// File: Converters/ConverterRevit/ConverterRevitShared/PartialClasses/ConvertCustomBeam.cs (continued)
public partial class ConverterRevit
{
  // Convert Speckle object back to Revit
  public ApplicationObject CustomBeamToNative(RevitCustomBeam speckleBeam)
  {
    var appObj = new ApplicationObject(speckleBeam.id, speckleBeam.speckle_type)
    {
      applicationId = speckleBeam.applicationId
    };

    try
    {
      // Get or find family symbol
      var familySymbol = FindFamilySymbol("Custom Beam Family");
      if (familySymbol == null)
      {
        appObj.Update(status: ApplicationObject.State.Failed,
          logItem: "Custom Beam family not found");
        return appObj;
      }

      // Convert curve
      var baseLine = CurveToNative(speckleBeam.baseLine).FirstOrDefault();
      if (baseLine == null)
      {
        appObj.Update(status: ApplicationObject.State.Failed);
        return appObj;
      }

      // Get level
      var level = ConvertLevelToRevit(speckleBeam.level);

      // Create or update element
      var docObj = GetExistingElementByApplicationId(speckleBeam.applicationId);
      DB.FamilyInstance revitBeam;

      if (docObj != null && docObj is DB.FamilyInstance existing)
      {
        revitBeam = existing;
        // Update geometry
        (revitBeam.Location as LocationCurve).Curve = baseLine;
      }
      else
      {
        // Create new instance
        revitBeam = Doc.Create.NewFamilyInstance(
          baseLine, familySymbol, level, StructuralType.Beam);
      }

      // Set custom properties
      SetParameterValue(revitBeam, "Offset", speckleBeam.offset);
      SetParameterValue(revitBeam, "IsPrecast", speckleBeam.isPrecast ? 1 : 0);

      // Apply parameters
      if (speckleBeam.parameters != null)
      {
        foreach (var param in speckleBeam.parameters)
        {
          UpdateParameter(revitBeam, param);
        }
      }

      appObj.Update(status: ApplicationObject.State.Success,
        logItem: $"CustomBeam {revitBeam.Id} created");
      appObj["revitElement"] = revitBeam;

      return appObj;
    }
    catch (Exception ex)
    {
      appObj.Update(status: ApplicationObject.State.Failed,
        logItem: ex.Message);
      return appObj;
    }
  }

  public bool CanConvertCustomBeamToNative(Base obj) 
    => obj is RevitCustomBeam or CustomBeam;
}
```

### 8.5 Step 5: Register in Converter Switch Statements

Update converter's main `ConvertToSpeckle` and `ConvertToNative` methods:

```csharp
// In ConverterRevit.cs ConvertToSpeckle method
public Base ConvertToSpeckle(object @object)
{
  switch (@object)
  {
    case DB.FamilyInstance inst 
      when inst.Symbol.FamilyName == "Custom Beam Family":
      return CustomBeamToSpeckle(inst);
    
    case DB.Beam o:
      return BeamToSpeckle(o);
    
    // ... other types
    default:
      return null;
  }
}

// In ConverterRevit.cs ConvertToNative method
public object ConvertToNative(Base @base)
{
  return @base switch
  {
    RevitCustomBeam cb => CustomBeamToNative(cb),
    CustomBeam cb => CustomBeamToNative(new RevitCustomBeam 
    { 
      baseLine = cb.baseLine,
      level = cb.level,
      offset = cb.offset,
      isPrecast = cb.isPrecast
    }),
    Beam b => BeamToNative(b),
    
    // ... other types
    _ => null
  };
}
```

### 8.6 Step 6: Update Converter Capability Checks

```csharp
public bool CanConvertToSpeckle(object @object)
{
  return @object switch
  {
    DB.FamilyInstance inst 
      when inst.Symbol.FamilyName == "Custom Beam Family" => true,
    DB.Beam => true,
    // ... other types
    _ => false
  };
}

public bool CanConvertToNative(Base @base)
{
  return @base switch
  {
    CustomBeam => true,
    RevitCustomBeam => true,
    Beam => true,
    // ... other types
    _ => false
  };
}
```

### 8.7 Step 7: Add Unit Tests (Optional)

```csharp
// File: Objects/Tests/Objects.Tests.Unit/CustomBeamTests.cs
[TestClass]
public class CustomBeamTests
{
  [TestMethod]
  public void CustomBeamConstructor_WithBaseLine_CreatesValidObject()
  {
    var line = new Line(
      new Point(0, 0, 0),
      new Point(10, 0, 0)
    );
    
    var beam = new CustomBeam(line, offset: 0.5);
    
    Assert.IsNotNull(beam.baseLine);
    Assert.AreEqual(0.5, beam.offset);
  }

  [TestMethod]
  public void CustomBeamSerialization_SerializesCustomProperties()
  {
    var beam = new CustomBeam(
      new Line(new Point(0, 0, 0), new Point(10, 0, 0)),
      offset: 1.0
    )
    {
      isPrecast = true,
      customProfile = "IPE300"
    };

    var dict = beam.ToDictionary();
    
    Assert.AreEqual(1.0, dict["offset"]);
    Assert.AreEqual(true, dict["isPrecast"]);
    Assert.AreEqual("IPE300", dict["customProfile"]);
  }
}
```

### 8.8 Common Patterns Checklist

When adding new object types:

- **Interfaces Implementation**
  - `IDisplayValue<T>` - For viewer-compatible objects
  - `ITransformable<T>` - For geometric objects with coordinate systems
  - `IHasBoundingBox` - For spatial objects
  - `IHasArea` / `IHasVolume` - For measured objects

- **Attributes**
  - `[SchemaInfo]` - For SchemaBuilder UI generation
  - `[SchemaMainParam]` - Primary geometry/identifier parameter
  - `[SchemaParamInfo]` - Parameter descriptions
  - `[DetachProperty]` - Large/nested properties for optimization
  - `[Obsolete]` - For deprecated properties

- **Nesting**
  - Use `List<Base> elements` for child objects
  - Use `Level level` for floor association
  - Use `IDisplayValue` for fallback geometry

- **Application-Specific Subclasses**
  - Extend base class for app-specific properties
  - Keep base class generic and app-agnostic
  - Use subclass in app-specific converters

---

## SUMMARY TABLE

| Aspect | Details |
|--------|---------|
| **Total Object Types** | 200+ across all disciplines |
| **Total Converter Projects** | 60+ (59 csproj files) |
| **Primary Converters** | Revit, CSI (ETABS/SAP2000/Bridge), Rhino, AutoCAD, Tekla |
| **Assembly Pattern** | NetStandard 2.0, ISpeckleConverter interface |
| **Version Support** | 10+ years of app versions (2020-2025) |
| **Loading Mechanism** | Dynamic assembly reflection at runtime |
| **Bidirectional** | ToSpeckle (import) and ToNative (export) |
| **Key Files** | ObjectsKit.cs (discovery), Interfaces.cs (contracts) |
| **Installation** | Local: AppData/Roaming/Speckle, System-wide: ProgramData |

