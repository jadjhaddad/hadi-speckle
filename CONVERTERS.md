# CONVERTERS - IMPLEMENTATION GUIDE

## Quick Reference

| Converter | Location | Type | Shared Code | Versions |
|-----------|----------|------|------------|----------|
| **Revit** | `ConverterRevit/` | BIM | ConverterRevitShared | 2020-2025 (6) |
| **CSI Suite** | `ConverterCSI/` | Analysis | ConverterCSIShared | ETABS, SAP2000, Bridge, SAFE |
| **Rhino/GH** | `ConverterRhinoGh/` | CAD/Parametric | ConverterRhinoGhShared | 6, 7, 8 |
| **AutoCAD** | `ConverterAutocadCivil/` | CAD | AutocadCivil Shared | 2021-2025 (5) |
| **Civil3D** | `ConverterAutocadCivil/` | Civil | AutocadCivil Shared | 2021-2025 (5) |
| **Dynamo** | `ConverterDynamo/` | Parametric | DynamoRevit Shared | 2021-2024 + Sandbox |
| **DXF** | `ConverterDxf/` | Geometry Only | None | Single |
| **Tekla** | `ConverterTeklaStructures/` | Structural BIM | None | 2020-2023 (4) |
| **Navisworks** | `ConverterNavisworks/` | Coordination | None | 2020-2025 (6) |
| **Bentley** | `ConverterBentley/` | Infrastructure | None | MicroStation, OpenBuildings, OpenRail, OpenRoads (4) |

---

## 1. CONVERTER PROJECT STRUCTURE PATTERNS

### 1.1 Shared Code Pattern (Preferred)

Most converters follow this organization to minimize duplication:

```
ConverterFamily/
├── ConverterShared/                    # Shared implementation
│   ├── Converter{App}.cs               # Main ISpeckleConverter
│   ├── Converter{App}.{Category}.cs    # Partial classes by category
│   ├── PartialClasses/
│   │   ├── Convert{ElementType1}.cs
│   │   ├── Convert{ElementType2}.cs
│   │   └── ...
│   ├── Extensions/
│   │   ├── {AppType}Extensions.cs
│   │   └── ...
│   ├── Models/
│   │   └── {HelperClasses}.cs
│   └── Converter{App}.projitems       # Shared project file
│
├── Converter2024/                      # Version-specific wrapper
│   ├── ConverterRevit2024.csproj
│   └── (imports Shared via .projitems)
│
├── Converter2023/                      # Another version
└── ...
```

### 1.2 Project File Pattern

**Version-specific project file:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <!-- Namespace matches shared code -->
    <RootNamespace>Objects.Converter.{AppName}</RootNamespace>
    <!-- DLL name format for discovery -->
    <AssemblyName>Objects.Converter.{AppName}{Version}</AssemblyName>
    <!-- Conditional compilation for version-specific APIs -->
    <DefineConstants>$(DefineConstants);{VERSION_SYMBOL}</DefineConstants>
  </PropertyGroup>

  <!-- Import shared code via shared project -->
  <Import Project="..\Converter{App}Shared\Converter{App}Shared.projitems" 
          Label="Shared" />

  <!-- Version-specific API references -->
  <ItemGroup>
    <PackageReference Include="App.API" Version="2024.0.0" />
  </ItemGroup>

  <!-- Required dependencies -->
  <ItemGroup>
    <ProjectReference Include="..\..\..\..\Core\Core\Core.csproj" />
    <ProjectReference Include="..\..\..\Objects\Objects.csproj" />
  </ItemGroup>
</Project>
```

---

## 2. REVIT CONVERTER - DETAILED EXAMPLE

### 2.1 Project Organization

```
ConverterRevit/
├── ConverterRevitShared/               # Shared for all versions
│   ├── ConverterRevit.cs               # Main class (partial)
│   ├── ConverterRevit.*.cs             # Partial files by concern
│   ├── PartialClasses/
│   │   ├── ConvertBeam.cs
│   │   ├── ConvertColumn.cs
│   │   ├── ConvertWall.cs
│   │   ├── ConvertFloor.cs
│   │   ├── ConvertRoof.cs
│   │   ├── ConvertMaterial.cs
│   │   ├── ConvertFamilyInstance.cs
│   │   ├── ConvertGeometry.cs
│   │   ├── ConvertModel.cs
│   │   └── ... (80+ conversion files)
│   ├── Extensions/
│   │   ├── CategoryExtensions.cs
│   │   ├── ParameterExtensions.cs
│   │   ├── ElementExtensions.cs
│   │   └── ...
│   ├── Models/
│   │   ├── ParameterToSpeckleData.cs
│   │   ├── Element2DOutlineBuilder.cs
│   │   └── ...
│   └── Revit/
│       ├── FamilyLoadOptions.cs
│       └── ...
│
├── ConverterRevit2024/
│   └── ConverterRevit2024.csproj
├── ConverterRevit2023/
│   └── ConverterRevit2023.csproj
├── ConverterRevit2022/
│   └── ConverterRevit2022.csproj
├── ConverterRevit2021/
│   └── ConverterRevit2021.csproj
├── ConverterRevit2020/
│   └── ConverterRevit2020.csproj
│
└── ConverterRevitTests/
    ├── ConverterRevitTests2024/
    ├── ConverterRevitTests2023/
    └── ...
```

### 2.2 Converter Class Structure

**File:** `ConverterRevitShared/ConverterRevit.cs`

```csharp
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Objects.BuiltElements;
using Objects.Structural.Properties.Profiles;
using Speckle.Core.Kits;
using Speckle.Core.Models;

namespace Objects.Converter.Revit;

/// <summary>
/// Converts between Revit documents and Speckle objects.
/// Uses conditional compilation to support multiple Revit versions.
/// </summary>
public partial class ConverterRevit : ISpeckleConverter
{
  #region Version Selection
  
  // Conditional compilation for version identification
#if REVIT2025
  public static string RevitAppName = HostApplications.Revit.GetVersion(HostAppVersion.v2025);
#elif REVIT2024
  public static string RevitAppName = HostApplications.Revit.GetVersion(HostAppVersion.v2024);
#elif REVIT2023
  public static string RevitAppName = HostApplications.Revit.GetVersion(HostAppVersion.v2023);
#elif REVIT2022
  public static string RevitAppName = HostApplications.Revit.GetVersion(HostAppVersion.v2022);
#elif REVIT2021
  public static string RevitAppName = HostApplications.Revit.GetVersion(HostAppVersion.v2021);
#elif REVIT2020
  public static string RevitAppName = HostApplications.Revit.GetVersion(HostAppVersion.v2020);
#endif
  
  #endregion Version Selection
  
  #region ISpeckleConverter Properties
  
  public string Description => "Default Speckle Kit for Revit";
  public string Name => nameof(ConverterRevit);
  public string Author => "Speckle";
  public string WebsiteOrEmail => "https://speckle.systems";
  
  /// <summary>Returns Revit version this converter handles</summary>
  public IEnumerable<string> GetServicedApplications() => new[] { RevitAppName };
  
  #endregion ISpeckleConverter Properties
  
  #region Context & State
  
  /// <summary>Revit document being converted from/to</summary>
  public Document Doc { get; private set; }
  
  /// <summary>
  /// Tracks objects being converted (for parent-child relationships).
  /// Prevents duplicate conversion of nested elements.
  /// </summary>
  public Dictionary<string, ApplicationObject> ContextObjects { get; set; }
  
  /// <summary>
  /// Maps previously received Speckle objects to Revit elements.
  /// Enables updates instead of delete/recreate.
  /// </summary>
  public IReceivedObjectIdMap<Base, Element> PreviouslyReceivedObjectIds { get; set; }
  
  /// <summary>Set of already converted element IDs (prevent duplicates)</summary>
  public ISet<string> ConvertedObjects { get; private set; } = new HashSet<string>();
  
  /// <summary>Cached Revit levels for reuse</summary>
  public Dictionary<string, Objects.BuiltElements.Level> Levels { get; private set; }
  
  /// <summary>Cached Revit phases</summary>
  public Dictionary<string, Phase> Phases { get; private set; }
  
  /// <summary>Cached family symbol definitions</summary>
  public Dictionary<string, Objects.BuiltElements.Revit.RevitSymbolElementType> Symbols 
    { get; private set; }
  
  /// <summary>Cached section profiles</summary>
  public Dictionary<string, SectionProfile> SectionProfiles { get; private set; }
  
  /// <summary>Cached material definitions</summary>
  public Dictionary<string, Objects.Other.Material> Materials { get; private set; }
  
  /// <summary>Conversion progress tracking</summary>
  public ProgressReport Report { get; private set; } = new();
  
  /// <summary>Converter settings from parent application</summary>
  public Dictionary<string, string> Settings { get; private set; }
  
  /// <summary>How to handle existing elements (Create, Update, Ignore, etc.)</summary>
  public ReceiveMode ReceiveMode { get; set; }
  
  #endregion Context & State
  
  #region Initialization
  
  public ConverterRevit()
  {
    var ver = System.Reflection.Assembly.GetAssembly(typeof(ConverterRevit))
      .GetName().Version;
    Report.Log($"Using converter: {Name} v{ver}");
  }
  
  public void SetContextDocument(object doc)
  {
    if (doc is Document document)
    {
      Doc = document;
      Report.Log($"Using document: {Doc.PathName}");
    }
  }
  
  public void SetContextObjects(List<ApplicationObject> objects)
  {
    ContextObjects = new(objects.Count);
    foreach (var ao in objects)
    {
      var key = ao.applicationId ?? ao.OriginalId;
      if (!ContextObjects.ContainsKey(key))
      {
        ContextObjects.Add(key, ao);
      }
    }
  }
  
  public void SetConverterSettings(object settings)
  {
    Settings = settings as Dictionary<string, string>;
  }
  
  #endregion Initialization
  
  #region Main Conversion Methods
  
  /// <summary>Convert native Revit object to Speckle</summary>
  public Base ConvertToSpeckle(object @object)
  {
    Base returnObject = null;
    List<string> notes = new();

    switch (@object)
    {
      case DB.Document doc:
        returnObject = ModelToSpeckle(doc, !doc.IsFamilyDocument);
        break;
      
      case DB.Floor floor:
        returnObject = FloorToSpeckle(floor, out notes);
        break;
      
      case DB.Wall wall:
        returnObject = WallToSpeckle(wall, out notes);
        break;
      
      case DB.FamilyInstance inst:
        returnObject = FamilyInstanceToSpeckle(inst, out notes);
        break;
      
      case DB.Level level:
        returnObject = LevelToSpeckle(level);
        break;
      
      // ... many more types
      default:
        return null;
    }

    if (returnObject != null && notes.Count > 0)
    {
      returnObject["notes"] = notes;
    }

    return returnObject;
  }
  
  /// <summary>Convert Speckle object to native Revit</summary>
  public object ConvertToNative(Base @base)
  {
    return @base switch
    {
      Floor floor => FloorToNative(floor),
      Wall wall => WallToNative(wall),
      Beam beam => BeamToNative(beam),
      Column column => ColumnToNative(column),
      Level level => LevelToNative(level),
      // ... more types
      _ => null
    };
  }
  
  /// <summary>Batch conversion (delegates to single-object method)</summary>
  public List<Base> ConvertToSpeckle(List<object> objects) 
    => objects.Select(ConvertToSpeckle).ToList();
  
  /// <summary>Batch conversion (delegates to single-object method)</summary>
  public List<object> ConvertToNative(List<Base> objects) 
    => objects.Select(ConvertToNative).ToList();
  
  #endregion Main Conversion Methods
  
  #region Capability Checks
  
  public bool CanConvertToSpeckle(object @object)
  {
    return @object switch
    {
      Document => true,
      Floor => true,
      Wall => true,
      Beam => true,
      Column => true,
      Ceiling => true,
      Roof => true,
      FamilyInstance => true,
      Level => true,
      Material => true,
      // ... more types
      _ => false
    };
  }
  
  public bool CanConvertToNative(Base @object)
  {
    return @object switch
    {
      Floor => true,
      Wall => true,
      Beam => true,
      Column => true,
      Ceiling => true,
      Roof => true,
      Level => true,
      Objects.BuiltElements.Revit.RevitElement => true,
      // ... more types
      _ => false
    };
  }
  
  public bool CanConvertToNativeDisplayable(Base @object)
  {
    return @object switch
    {
      Objects.Geometry.Mesh => true,
      Objects.Geometry.Brep => true,
      Objects.Geometry.Line => true,
      Objects.Geometry.Polyline => true,
      _ => false
    };
  }
  
  #endregion Capability Checks
}
```

### 2.3 Partial Class Pattern - Element Conversion

**File:** `ConverterRevitShared/PartialClasses/ConvertBeam.cs`

```csharp
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Objects.BuiltElements;
using Objects.BuiltElements.Revit;
using Speckle.Core.Models;
using DB = Autodesk.Revit.DB;

namespace Objects.Converter.Revit;

/// <summary>
/// Partial class for Beam conversion logic.
/// Isolates beam-specific conversion to improve code organization.
/// </summary>
public partial class ConverterRevit
{
  const string StructuralFraming = "Structural Framing";

  /// <summary>Convert Speckle beam to Revit family instance</summary>
  public ApplicationObject BeamToNative(Beam speckleBeam, 
    StructuralType structuralType = StructuralType.Beam)
  {
    var appObj = new ApplicationObject(speckleBeam.id, speckleBeam.speckle_type)
    {
      applicationId = speckleBeam.applicationId
    };

    // Validation: Beam must have a curve
    if (speckleBeam.baseLine == null)
    {
      appObj.Update(status: ApplicationObject.State.Failed, 
        logItem: "Only line based Beams are currently supported.");
      return appObj;
    }

    // Find or get family symbol
    var familySymbol = GetElementType<FamilySymbol>(speckleBeam, appObj, 
      out bool isExactMatch);
    if (familySymbol == null)
    {
      appObj.Update(status: ApplicationObject.State.Failed);
      return appObj;
    }

    // Convert curve geometry
    var baseLine = CurveToNative(speckleBeam.baseLine).get_Item(0);

    // Get or create level
    var levelState = ApplicationObject.State.Unknown;
    double baseOffset = 0.0;
    DB.Level level =
      (speckleBeam.level != null)
        ? ConvertLevelToRevit(speckleBeam.level, out levelState)
        : ConvertLevelToRevit(baseLine, out levelState, out baseOffset);

    // Try to get existing element for update
    var docObj = GetExistingElementByApplicationId(speckleBeam.applicationId);
    DB.FamilyInstance revitBeam = null;
    var isUpdate = false;

    try
    {
      if (docObj != null)
      {
        var revitType = Doc.GetElement(docObj.GetTypeId()) as ElementType;

        // If family changed, delete old and create new
        if (familySymbol.FamilyName != revitType.FamilyName)
        {
          Doc.Delete(docObj.Id);
        }
        else
        {
          // Update existing beam
          revitBeam = (DB.FamilyInstance)docObj;
          var existingCurve = (revitBeam.Location as LocationCurve).Curve;
          existingCurve = baseLine;

          // Update type if needed
          if (isExactMatch && revitType.Id.IntegerValue != familySymbol.Id.IntegerValue)
          {
            revitBeam.ChangeTypeId(familySymbol.Id);
          }

          isUpdate = true;
        }
      }

      // Create new beam if not updating
      if (!isUpdate)
      {
        revitBeam = Doc.Create.NewFamilyInstance(baseLine, familySymbol, 
          level, structuralType);
      }

      // Apply Revit-specific properties if present
      if (speckleBeam is RevitBeam revitSpeckleBeam)
      {
        if (revitSpeckleBeam.parameters != null)
        {
          foreach (var param in revitSpeckleBeam.parameters)
          {
            UpdateParameter(revitBeam, param);
          }
        }
      }

      appObj.Update(status: ApplicationObject.State.Success, 
        logItem: $"Beam {revitBeam.Id} {(isUpdate ? "updated" : "created")}");
      appObj["revitElement"] = revitBeam;

      return appObj;
    }
    catch (Exception ex)
    {
      appObj.Update(status: ApplicationObject.State.Failed, 
        logItem: $"Failed to create beam: {ex.Message}");
      return appObj;
    }
  }

  /// <summary>Convert Revit beam family instance to Speckle</summary>
  public Base BeamToSpeckle(DB.FamilyInstance revitBeam, 
    out List<string> notes)
  {
    notes = new();

    try
    {
      // Extract geometry
      var location = revitBeam.Location as LocationCurve;
      if (location == null)
      {
        notes.Add("Could not extract curve location");
        return null;
      }

      ICurve baseLine = CurveToSpeckle(location.Curve);
      
      // Get level
      var level = LevelToSpeckle(revitBeam.Level);
      
      // Get family and type info
      var familySymbol = revitBeam.Symbol;
      var family = familySymbol.Family;

      // Create Speckle object
      var speckleBeam = new RevitBeam
      {
        baseLine = baseLine,
        level = level,
        units = ModelUnits,
        applicationId = revitBeam.UniqueId,
        revitFamily = family.Name,
        revitType = familySymbol.Name,
        parameters = GetElementParameters(revitBeam)
      };

      // Get display geometry for viewers
      var displayGeom = GetGeometry(revitBeam);
      if (displayGeom != null)
      {
        speckleBeam.displayValue = displayGeom;
      }

      return speckleBeam;
    }
    catch (Exception ex)
    {
      notes.Add($"Error converting beam: {ex.Message}");
      return null;
    }
  }
}
```

### 2.4 Helper Extensions

**File:** `ConverterRevitShared/Extensions/ParameterExtensions.cs`

```csharp
using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Speckle.Core.Models;

namespace Objects.Converter.Revit;

/// <summary>Extensions for working with Revit parameters</summary>
public static class ParameterExtensions
{
  /// <summary>Extract all parameters from Revit element</summary>
  public static List<Parameter> GetElementParameters(this Element element)
  {
    var parameters = new List<Parameter>();

    foreach (Parameter param in element.Parameters)
    {
      if (param.StorageType == StorageType.None)
        continue;

      var value = param.StorageType switch
      {
        StorageType.Integer => param.AsInteger(),
        StorageType.Double => param.AsDouble(),
        StorageType.String => param.AsString(),
        StorageType.ElementId => param.AsElementId()?.IntegerValue.ToString(),
        _ => null
      };

      if (value != null)
      {
        parameters.Add(new Parameter
        {
          name = param.Definition.Name,
          value = value,
          applicationId = param.Definition.Name
        });
      }
    }

    return parameters;
  }

  /// <summary>Set Revit parameter value from Speckle</summary>
  public static void UpdateParameter(this Element element, Parameter param)
  {
    foreach (Parameter revitParam in element.Parameters)
    {
      if (revitParam.Definition.Name == param.name)
      {
        if (revitParam.IsReadOnly) continue;

        try
        {
          switch (revitParam.StorageType)
          {
            case StorageType.Integer:
              if (int.TryParse(param.value?.ToString(), out int intVal))
                revitParam.Set(intVal);
              break;

            case StorageType.Double:
              if (double.TryParse(param.value?.ToString(), out double dblVal))
                revitParam.Set(dblVal);
              break;

            case StorageType.String:
              revitParam.Set(param.value?.ToString() ?? "");
              break;
          }
        }
        catch (Exception ex)
        {
          // Parameter set failed - log and continue
        }

        break;
      }
    }
  }
}
```

---

## 3. CSI CONVERTER - STRUCTURAL ANALYSIS PATTERN

### 3.1 Project Organization

```
ConverterCSI/
├── ConverterCSIShared/
│   ├── ConverterCSI.cs                 # Main converter
│   ├── ConverterCSI.*.cs               # Partial files
│   ├── PartialClasses/
│   │   ├── Analysis/
│   │   │   ├── ConvertModel.cs
│   │   │   ├── ConvertModelInfo.cs
│   │   │   └── ConvertModelSettings.cs
│   │   ├── Geometry/
│   │   │   ├── ConvertNode.cs
│   │   │   ├── ConvertElement1D.cs
│   │   │   ├── ConvertElement2D.cs
│   │   │   └── ConvertGridLines.cs
│   │   ├── Loading/
│   │   │   ├── ConvertLoad.cs
│   │   │   ├── ConvertLoadCase.cs
│   │   │   └── ConvertLoadPattern.cs
│   │   └── Results/
│   │       ├── ConvertAnalysisResults.cs
│   │       └── ResultsConverter.cs
│   ├── Models/
│   │   ├── ApiResultValidator.cs
│   │   ├── ResultsConverter.cs
│   │   └── DatabaseTableWrapper.cs
│   └── Extensions/
│       ├── CurveExtensions.cs
│       ├── LineExtensions.cs
│       └── ...
│
├── ConverterETABS/                     # ETABS 2023+
│   └── ConverterETABS.csproj
├── ConverterETABS22/                   # ETABS 2022
│   └── ConverterETABS22.csproj
├── ConverterSAP2000/                   # SAP2000 standard
│   └── ConverterSAP2000.csproj
├── ConverterSAP2000-26/                # SAP2000 v26+
│   └── ConverterSAP2000-26.csproj
├── ConverterCSIBridge/                 # CSiBridge standard
│   └── ConverterCSIBridge.csproj
├── ConverterCSIBridge25/               # CSiBridge v25
│   └── ConverterCSIBridge25.csproj
├── ConverterCSIBridge26/               # CSiBridge v26
│   └── ConverterCSIBridge26.csproj
└── ConverterSAFE/                      # SAFE
    └── ConverterSAFE.csproj
```

### 3.2 CSI Converter Class

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;
#endif
using Objects.Structural.Analysis;
using Objects.Structural.CSI.Analysis;
using Objects.Structural.Geometry;
using Objects.Structural.Loading;
using Speckle.Core.Kits;
using Speckle.Core.Models;

namespace Objects.Converter.CSI;

/// <summary>
/// Converts between CSI applications (SAP2000, ETABS, CSiBridge, SAFE)
/// and Speckle structural analysis objects.
/// Uses conditional compilation to handle API version differences.
/// </summary>
public partial class ConverterCSI : ISpeckleConverter, IFinalizable
{
  #region Version Selection
  
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
  
  #endregion Version Selection
  
  #region ISpeckleConverter Implementation
  
  public string Description => "Default Speckle Kit for CSI";
  public string Name => nameof(ConverterCSI);
  public string Author => "Speckle";
  public string WebsiteOrEmail => "https://speckle.systems";
  
  public IEnumerable<string> GetServicedApplications() => new[] { CSIAppName };
  
  #endregion ISpeckleConverter Implementation
  
  #region Context & State
  
  /// <summary>CSI API model object</summary>
  public cSapModel Model { get; private set; }
  
  /// <summary>CSI application version string</summary>
  public string ProgramVersion { get; private set; }
  
  /// <summary>Converted structural model</summary>
  public Model SpeckleModel { get; set; }
  
  /// <summary>How to handle receiving (Create, Update, etc.)</summary>
  public ReceiveMode ReceiveMode { get; set; }
  
  /// <summary>
  /// Maps element GUIDs to names. CSI doesn't guarantee stable GUIDs,
  /// so we track both for robust element matching.
  /// </summary>
  public Dictionary<string, string> ExistingObjectGuids { get; set; }
  
  /// <summary>Objects being converted (parent-child tracking)</summary>
  public List<ApplicationObject> ContextObjects { get; set; } = new();
  
  /// <summary>Objects from previous receive (for updates)</summary>
  public List<ApplicationObject> PreviousContextObjects { get; set; } = new();
  
  /// <summary>Converter settings from parent</summary>
  public Dictionary<string, string> Settings { get; private set; } = new();
  
  /// <summary>Progress tracking</summary>
  public ProgressReport Report { get; private set; } = new();
  
  #endregion Context & State
  
  #region Initialization
  
  public void SetContextDocument(object doc)
  {
    Model = (cSapModel)doc;
    
    // Get CSI version
    double version = 0;
    string versionString = null;
    Model.GetVersion(ref versionString, ref version);
    ProgramVersion = versionString;

    // Validate settings
    if (!Settings.ContainsKey("operation"))
    {
      throw new Exception("'operation' setting must be set before calling SetContextDocument");
    }

    // Load existing objects or prepare for send
    if (Settings["operation"] == "receive")
    {
      // Loading into CSI: get existing elements to avoid duplicates
      ExistingObjectGuids = GetAllGuids(Model);
    }
    else if (Settings["operation"] == "send")
    {
      // Sending from CSI: convert model to Speckle
      SpeckleModel = ModelToSpeckle();
    }
  }
  
  public void SetContextObjects(List<ApplicationObject> objects) 
    => ContextObjects = objects;
  
  public void SetPreviousContextObjects(List<ApplicationObject> objects) 
    => PreviousContextObjects = objects;
  
  public void SetConverterSettings(object settings)
  {
    Settings = settings as Dictionary<string, string>;
  }
  
  #endregion Initialization
  
  #region Main Conversion Methods
  
  /// <summary>Convert native CSI object to Speckle (SEND operation)</summary>
  public bool CanConvertToSpeckle(object @object)
  {
    if (@object == null) return false;

    // Check against list of supported CSI API types
    foreach (var type in Enum.GetNames(typeof(eItemType)))
    {
      if (type == @object.ToString())
        return true;
    }
    return false;
  }
  
  /// <summary>Convert Speckle object to native CSI (RECEIVE operation)</summary>
  public bool CanConvertToNative(Base @object)
  {
    return @object switch
    {
      Objects.Structural.Geometry.Element2D => true,
      Objects.Structural.Geometry.Element1D => true,
      Objects.Structural.Geometry.Node => true,
      Objects.Structural.Geometry.GridLine => true,
      Objects.Structural.Loading.Load => true,
      Objects.BuiltElements.Beam => true,
      Objects.BuiltElements.Column => true,
      Objects.BuiltElements.Brace => true,
      Objects.Structural.Materials.StructuralMaterial => true,
      _ => false
    };
  }

  public object ConvertToNative(Base @object)
  {
    var appObj = new ApplicationObject(@object.id, @object.speckle_type) 
    { 
      applicationId = @object.applicationId 
    };

    try
    {
      string convertedName = @object switch
      {
        CSIAreaSpring area => AreaSpringPropertyToNative(area),
        CSIDiaphragm diaphragm => DiaphragmToNative(diaphragm),
        CSILinearSpring spring => LinearSpringPropertyToNative(spring),
        Objects.Structural.Geometry.Element2D elem2D => Element2DToNative(elem2D),
        Objects.Structural.Geometry.Element1D elem1D => Element1DToNative(elem1D),
        Objects.Structural.Geometry.Node node => NodeToNative(node),
        Objects.Structural.Geometry.GridLine gridLine => GridLineToNative(gridLine),
        Objects.Structural.Loading.Load load => LoadToNative(load),
        _ => null
      };

      if (convertedName != null)
      {
        appObj.Update(status: ApplicationObject.State.Success, 
          logItem: $"Created {convertedName}");
      }
      else
      {
        appObj.Update(status: ApplicationObject.State.Failed, 
          logItem: "Unsupported object type");
      }

      return appObj;
    }
    catch (Exception ex)
    {
      appObj.Update(status: ApplicationObject.State.Failed, 
        logItem: $"Error: {ex.Message}");
      return appObj;
    }
  }
  
  #endregion Main Conversion Methods
  
  #region Capability Checks
  
  public bool CanConvertToNativeDisplayable(Base @object) => false;
  
  public object ConvertToNativeDisplayable(Base @base) 
    => throw new NotImplementedException();
  
  #endregion Capability Checks
}
```

### 3.3 Element1D Conversion Example

```csharp
// File: ConverterCSIShared/PartialClasses/Geometry/ConvertElement1D.cs

public partial class ConverterCSI
{
  /// <summary>
  /// Convert CSI frame/beam element to Speckle Element1D
  /// </summary>
  public Element1D Element1DToSpeckle(string elementName)
  {
    var element = new Element1D 
    { 
      name = elementName,
      applicationId = elementName 
    };

    // Get start and end nodes
    string startNodeName = "";
    string endNodeName = "";
    int ret = Model.FrameObj.GetEndPoints(elementName, ref startNodeName, ref endNodeName);
    
    if (ret == 0) // Success
    {
      element.startNode = NodeToSpeckle(startNodeName);
      element.endNode = NodeToSpeckle(endNodeName);
    }

    // Get property assignment
    string propName = "";
    Model.FrameObj.GetProperty(elementName, ref propName);
    if (!string.IsNullOrEmpty(propName))
    {
      element.property = PropertyToSpeckle(propName);
    }

    // Get member releases
    bool[] startReleases = new bool[6];
    bool[] endReleases = new bool[6];
    Model.FrameObj.GetReleases(elementName, ref startReleases, ref endReleases);
    
    element.startReleases = startReleases.ToList();
    element.endReleases = endReleases.ToList();

    return element;
  }

  /// <summary>
  /// Convert Speckle Element1D to CSI frame element
  /// </summary>
  public string Element1DToNative(Element1D element)
  {
    // Ensure nodes exist or create them
    string startNodeName = GetOrCreateNode(element.startNode);
    string endNodeName = GetOrCreateNode(element.endNode);

    // Create frame element
    int ret = Model.FrameObj.SetFrame(element.name, startNodeName, endNodeName);
    
    if (ret != 0)
    {
      throw new Exception($"Failed to create frame element {element.name}");
    }

    // Assign property
    if (element.property != null)
    {
      Model.FrameObj.SetProperty(element.name, element.property.name);
    }

    // Set releases if specified
    if (element.startReleases != null && element.endReleases != null)
    {
      bool[] startRel = element.startReleases.ToArray();
      bool[] endRel = element.endReleases.ToArray();
      Model.FrameObj.SetReleases(element.name, startRel, endRel);
    }

    return element.name;
  }
}
```

---

## 4. RHINO/GRASSHOPPER CONVERTER

### 4.1 Project Organization

```
ConverterRhinoGh/
├── ConverterRhinoGhShared/
│   ├── ConverterRhinoGh.cs             # Main converter
│   ├── ConverterRhinoGh.*.cs           # Partial files by category
│   ├── BrepEncoder.cs                  # Brep encoding/decoding
│   ├── KnotListEncoder.cs              # NURBS knot handling
│   └── ...
├── ConverterRhino6/
│   └── ConverterRhino6.csproj
├── ConverterRhino7/
│   └── ConverterRhino7.csproj
├── ConverterRhino8/
│   └── ConverterRhino8.csproj
├── ConverterGrasshopper6/
│   └── ConverterGrasshopper6.csproj
├── ConverterGrasshopper7/
│   └── ConverterGrasshopper7.csproj
└── ConverterGrasshopper8/
    └── ConverterGrasshopper8.csproj
```

### 4.2 Key Differences from BIM Converters

- **Bidirectional geometry-focused** - Both ToSpeckle and ToNative are important
- **NURBS and Brep support** - Advanced geometric conversions
- **Parametric integration** - Grasshopper has special requirements
- **Display geometry** - Emphasis on visualization-compatible conversion

---

## 5. TESTING PATTERN

### 5.1 Test Structure

```csharp
// File: ConverterRevitTests/ConverterRevitTests2024/BeamConversionTests.cs

[TestClass]
public class BeamConversionTests
{
  private ConverterRevit converter;
  private Document testDoc;

  [TestInitialize]
  public void Setup()
  {
    converter = new ConverterRevit();
    testDoc = // Create test document
  }

  [TestMethod]
  public void BeamToSpeckle_WithValidBeam_CreatesValidSpeckleObject()
  {
    // Arrange
    var revitBeam = CreateTestBeam(testDoc);

    // Act
    var speckleBeam = converter.BeamToSpeckle(revitBeam, out var notes);

    // Assert
    Assert.IsNotNull(speckleBeam);
    Assert.IsNotNull(speckleBeam.baseLine);
    Assert.IsInstanceOfType(speckleBeam, typeof(Beam));
  }

  [TestMethod]
  public void BeamToNative_WithValidSpeckleBeam_CreatesRevitElement()
  {
    // Arrange
    var speckleBeam = new Beam
    {
      baseLine = new Line(
        new Point(0, 0, 0),
        new Point(10, 0, 0)
      ),
      level = new Level { name = "Level 1", elevation = 0 }
    };

    // Act
    var result = converter.BeamToNative(speckleBeam);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(ApplicationObject.State.Success, 
      ((ApplicationObject)result).Status);
  }
}
```

---

## 6. COMMON PITFALLS & SOLUTIONS

| Issue | Cause | Solution |
|-------|-------|----------|
| **Version Mismatch** | Converter DLL built against different Objects version | Ensure Major.Minor versions match |
| **Assembly Not Found** | Converter DLL in wrong location | Check kit folder or same directory as Objects.dll |
| **Null Reference in Conversion** | Missing null checks on optional properties | Always validate geometry before conversion |
| **Circular References** | Parent-child elements converted twice | Use ConvertedObjects set to track already-converted elements |
| **Unit Conversion Issues** | Forgetting to apply unit scaling | Always include units in geometry objects |
| **Parameter Not Updated** | Parameter is read-only in target app | Check param.IsReadOnly before setting |
| **Geometry Invalid After Transform** | Wrong transform order | Apply transform consistently across all coordinates |

---

## 7. QUICK START - ADDING A NEW CONVERTER

### Step 1: Create Project Structure

```bash
mkdir ConverterNewApp
mkdir ConverterNewApp/ConverterNewAppShared
mkdir ConverterNewApp/ConverterNewApp2024
```

### Step 2: Create Shared Project File

```xml
<!-- ConverterNewAppShared/ConverterNewAppShared.projitems -->
<Project>
  <ItemGroup>
    <Compile Include="$(MSBuildThisFileDirectory)ConverterNewApp.cs" />
    <Compile Include="$(MSBuildThisFileDirectory)PartialClasses\*.cs" />
  </ItemGroup>
</Project>
```

### Step 3: Implement ISpeckleConverter

```csharp
// ConverterNewAppShared/ConverterNewApp.cs
public partial class ConverterNewApp : ISpeckleConverter
{
  public string Name => nameof(ConverterNewApp);
  public string Description => "Speckle converter for NewApp";
  public string Author => "Your Name";
  public string WebsiteOrEmail => "your@email.com";

  public IEnumerable<string> GetServicedApplications() 
    => new[] { HostApplications.NewApp.Slug };

  // Implement required methods...
}
```

### Step 4: Create Version-Specific Projects

Create wrapper projects for each app version that import shared code.

### Step 5: Register in ObjectsKit

The kit discovers converters automatically via assembly scanning. Ensure:
1. DLL named `Objects.Converter.NewApp.dll`
2. Version matches Objects major.minor
3. Implements ISpeckleConverter
4. GetServicedApplications() returns correct app name

---

## REFERENCE - CONDITI ONAL COMPILATION SYMBOLS

```
Revit:      REVIT2020, REVIT2021, REVIT2022, REVIT2023, REVIT2024, REVIT2025
ETABS:      ETABS, ETABS22
SAP2000:    SAP2000
CSiBridge:  CSIBRIDGE, CSIBRIDGE25, CSIBRIDGE26
SAFE:       SAFE
Rhino:      RHINO6, RHINO7, RHINO8
Grasshopper: GH6, GH7, GH8
```

These are set in `.csproj` PropertyGroup:
```xml
<DefineConstants>$(DefineConstants);REVIT2024</DefineConstants>
```

