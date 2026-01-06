# Speckle Sharp Connectors Architecture Documentation

This document provides a comprehensive overview of connector implementations across the Speckle Sharp ecosystem, covering patterns, architecture, and best practices.

## Table of Contents

1. [Overview](#overview)
2. [Connector Types](#connector-types)
3. [Core Architecture](#core-architecture)
4. [Common Patterns](#common-patterns)
5. [Connector-Specific Patterns](#connector-specific-patterns)
6. [Implementation Guide](#implementation-guide)
7. [Version Management](#version-management)
8. [Integration with Objects and Converters](#integration-with-objects-and-converters)

---

## Overview

Connectors are the bridge between Speckle and host applications (Revit, Rhino, AutoCAD, etc.). They enable users to:
- Send design data from host applications to Speckle streams
- Receive design data from streams into host applications
- Manage object conversions between application-native formats and Speckle objects
- Persist stream metadata within application files

All connectors share a common base architecture through:
- **DesktopUI2**: The unified UI framework for all desktop connectors
- **ConnectorCore**: Shared utilities (DLL conflict management, logging, analytics)
- **ConnectorBindings**: The bridge between host applications and the UI framework

---

## Connector Types

### 1. Desktop Application Connectors

**Examples**: Revit, AutoCAD, Civil 3D, Rhino

These connectors integrate with desktop CAD/BIM applications via plugin/add-in architecture:

- **Revit**: Uses External Application and External Command interfaces
- **AutoCAD/Civil 3D**: Uses IExtensionApplication interface
- **Rhino**: Extends PlugIn base class from RhinoCommon
- **AdvanceSteel**: Inherits AutoCAD integration patterns

**Characteristics**:
- Full access to host application API
- Run in host application process
- Handle document/file-specific operations
- Manage object selection and conversion

### 2. Visual Programming / Procedural Connectors

**Examples**: Grasshopper, Dynamo

These connectors provide nodes/components for data workflows:

- **Grasshopper**: Assembly-priority plugin providing GH components
- **Dynamo**: NodeModel-based components integrated into Dynamo graph

**Characteristics**:
- Extend host visual programming environment
- Component-based architecture (nodes/components)
- Can run in headless mode (compute-friendly)
- Handle streaming, conversion, and data operations

### 3. Structural Analysis Connectors

**Examples**: ETABS, SAP2000, CSIBridge, SAFE

These connectors integrate with CSI's structural analysis software:

- Use CSI's C++ API wrapper (`cSapModel`, `cPluginCallback`)
- Plugin-based loading mechanism
- Window integration for dialog management
- API-driven operations without visual editor

**Characteristics**:
- COM/C++ API integration
- Callback-based model interactions
- Specialized for structural data
- Version-specific implementations (ETABS22 vs legacy)

---

## Core Architecture

### ConnectorBindings Base Class

Located in: `/DesktopUI2/DesktopUI2/ConnectorBindings.cs`

The abstract base class that all connectors must implement:

```csharp
public abstract class ConnectorBindings
{
    // Application Information
    public abstract string GetHostAppName();
    public abstract string GetHostAppNameVersion();
    public abstract string GetFileName();
    public abstract string GetDocumentId();
    public abstract string GetDocumentLocation();
    public abstract string GetActiveViewName();
    
    // Stream Persistence
    public abstract List<StreamState> GetStreamsInFile();
    public abstract void WriteStreamsToFile(List<StreamState> streams);
    
    // Send/Receive Operations
    public abstract Task<string> SendStream(StreamState state, ProgressViewModel progress);
    public abstract Task<StreamState> ReceiveStream(StreamState state, ProgressViewModel progress);
    
    // Selection and Filtering
    public abstract List<string> GetSelectedObjects();
    public abstract List<string> GetObjectsInView();
    public abstract void SelectClientObjects(List<string> objs, bool deselect = false);
    public abstract List<ISelectionFilter> GetSelectionFilters();
    
    // Configuration
    public abstract List<ReceiveMode> GetReceiveModes();
    public abstract void ResetDocument();
    
    // Optional Features
    public virtual bool CanReceive => true;
    public virtual bool CanPreviewSend => false;
    public virtual bool CanPreviewReceive => false;
    public virtual bool CanOpen3DView => false;
    public virtual Task Open3DView(List<double> viewCoordinates, string viewName = "") => Task.CompletedTask;
}
```

**Key Responsibilities**:
- Bridge between host application and DesktopUI2
- Abstract application-specific operations
- Provide stream persistence interface
- Handle object conversion coordination

### DesktopUI2 Integration

The DesktopUI2 framework provides:
- Unified UI across all connectors (WPF + Avalonia)
- ViewModel architecture (MainViewModel, ProgressViewModel, etc.)
- Stream management and account handling
- Analytics and logging infrastructure

**Entry Point Pattern**:
1. Host application loads connector assembly
2. Connector initializes DesktopUI2 and creates `ConnectorBindings` instance
3. Creates `MainViewModel` with bindings instance
4. DesktopUI2 uses bindings to coordinate operations

---

## Common Patterns

### 1. Plugin/Add-in Registration

#### Revit Pattern (IExternalApplication)

```csharp
[Transaction(TransactionMode.Manual)]
public class App : IExternalApplication
{
    public static UIApplication AppInstance { get; set; }
    public static UIControlledApplication UICtrlApp { get; set; }
    
    public Result OnStartup(UIControlledApplication application)
    {
        // 1. Setup DLL conflict management
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        
        // 2. Initialize Speckle core
        Setup.Init(ConnectorBindingsRevit.HostAppNameVersion, ConnectorBindingsRevit.HostAppName);
        
        // 3. Create and register bindings
        ConnectorBindingsRevit bindings = new(AppInstance);
        bindings.RegisterAppEvents();
        SpeckleRevitCommand.Bindings = bindings;
        
        // 4. Setup UI (Ribbon, buttons, dockable panels)
        InitializeUiPanel(application);
        
        return Result.Succeeded;
    }
    
    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}
```

**Key Elements**:
- `IExternalApplication` interface for startup/shutdown
- Assembly resolution for dependency management
- Ribbon UI setup during `OnStartup`
- Static bindings instance for command access

#### AutoCAD/Civil3D Pattern (IExtensionApplication)

```csharp
public class App : IExtensionApplication
{
    public void Initialize()
    {
        // Wait for ribbon to be available
        ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
        
        // Setup bindings and core
        var bindings = new ConnectorBindingsAutocad();
        Setup.Init(bindings.GetHostAppNameVersion(), bindings.GetHostAppName());
        
        SpeckleAutocadCommand.Bindings = bindings;
    }
    
    public void Terminate() { }
}
```

#### Rhino Pattern (PlugIn)

```csharp
public class SpeckleRhinoConnectorPlugin : PlugIn
{
    public static SpeckleRhinoConnectorPlugin Instance { get; private set; }
    public ConnectorBindingsRhino Bindings { get; private set; }
    
    protected override LoadReturnCode OnLoad(ref string errorMessage)
    {
        // Initialize logging
        SpeckleLog.Initialize(HostApplications.Rhino.Slug, version, logConfig);
        
        // Initialize UI and bindings
        Init();
        
        // Register dockable panels
        Panels.RegisterPanel(this, typeof(DuiPanel), "Speckle");
        
        return LoadReturnCode.Success;
    }
    
    public void Init()
    {
        appBuilder = BuildAvaloniaApp().SetupWithoutStarting();
        Bindings = new ConnectorBindingsRhino();
        
        // Subscribe to document events
        RhinoDoc.BeginOpenDocument += RhinoDoc_BeginOpenDocument;
        RhinoDoc.EndOpenDocument += RhinoDoc_EndOpenDocument;
    }
}
```

#### Grasshopper Pattern (GH_AssemblyPriority)

```csharp
public class Loader : GH_AssemblyPriority
{
    public override GH_LoadingInstruction PriorityLoad()
    {
        // Initialize at high priority before other plugins
        Setup.Init(GetRhinoHostAppVersion(), HostApplications.Rhino.Slug, logConfig);
        
        // Register component categories and icons
        Instances.ComponentServer.AddCategoryIcon(ComponentCategories.PRIMARY_RIBBON, Resources.speckle_logo);
        
        // Setup menu system
        Instances.CanvasCreated += OnCanvasCreated;
        
        return GH_LoadingInstruction.Proceed;
    }
}
```

### 2. Command Implementation

#### Revit External Command

```csharp
[Transaction(TransactionMode.Manual)]
public class SpeckleRevitCommand : IExternalCommand
{
    public static ConnectorBindingsRevit Bindings { get; set; }
    
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        if (UseDockablePanel)
        {
            RegisterPane();
            var panel = App.AppInstance.GetDockablePane(PanelId);
            panel.Show();
        }
        else
        {
            CreateOrFocusSpeckle();
        }
        
        return Result.Succeeded;
    }
    
    public static void CreateOrFocusSpeckle(bool showWindow = true)
    {
        if (MainWindow == null)
        {
            var viewModel = new MainViewModel(Bindings);
            MainWindow = new MainWindow { DataContext = viewModel };
        }
        
        if (showWindow)
        {
            MainWindow.Show();
            MainWindow.Activate();
        }
    }
}
```

**Key Pattern Elements**:
- Implements host-specific command interface
- Uses static bindings instance
- Creates or activates window on command execute
- Passes bindings to ViewModel for UI coordination

### 3. Object Selection and Filtering

#### Revit Selection Pattern

```csharp
public partial class ConnectorBindingsRevit : ConnectorBindings
{
    public override List<string> GetSelectedObjects()
    {
        var selectedObjects = new List<string>();
        var selection = CurrentDoc.Selection;
        
        foreach (var elementId in selection.GetElementIds())
        {
            var element = CurrentDoc.Document.GetElement(elementId);
            if (element != null)
            {
                selectedObjects.Add(element.UniqueId);
            }
        }
        
        return selectedObjects;
    }
    
    public override List<ISelectionFilter> GetSelectionFilters()
    {
        return new List<ISelectionFilter>
        {
            new CategorySelectionFilter(),
            new ViewFilterSelectionFilter(),
            // ... other filters
        };
    }
}
```

#### Rhino Selection Pattern

```csharp
public partial class ConnectorBindingsRhino : ConnectorBindings
{
    public override List<string> GetSelectedObjects()
    {
        var rhinoDoc = RhinoDoc.ActiveDoc;
        var selected = rhinoDoc.Objects.GetSelectedObjects(false, false);
        return selected.Select(obj => obj.Id.ToString()).ToList();
    }
}
```

### 4. Send/Receive Operations

#### Send Pattern

```csharp
public override async Task<string> SendStream(StreamState state, ProgressViewModel progress)
{
    try
    {
        progress.IsLoading = true;
        progress.Message = "Converting objects...";
        
        // 1. Get objects to send
        var selectedObjects = GetSelectedObjects();
        
        // 2. Convert to Speckle
        var converter = KitManager.GetDefaultKit().LoadConverter(HostAppName);
        var speckleObjects = converter.ConvertToSpeckle(selectedObjects);
        
        // 3. Create commit
        var client = new Client(state.Account);
        var commitId = await client.CommitObjects(state.StreamId, speckleObjects);
        
        // 4. Update stream state
        state.LastCommitId = commitId;
        WriteStreamsToFile(new List<StreamState> { state });
        
        return commitId;
    }
    catch (Exception ex)
    {
        progress.Message = $"Error: {ex.Message}";
        throw;
    }
}
```

#### Receive Pattern

```csharp
public override async Task<StreamState> ReceiveStream(StreamState state, ProgressViewModel progress)
{
    try
    {
        progress.IsLoading = true;
        progress.Message = "Receiving objects...";
        
        // 1. Get commit from server
        var client = new Client(state.Account);
        var commit = await client.CommitGet(state.StreamId, state.CommitId);
        
        // 2. Convert from Speckle
        var converter = KitManager.GetDefaultKit().LoadConverter(HostAppName);
        var nativeObjects = converter.ConvertToNative(commit.ReferencedObject);
        
        // 3. Create/update in document
        CreateOrUpdateObjects(nativeObjects, state.ReceiveMode);
        
        return state;
    }
    catch (Exception ex)
    {
        progress.Message = $"Error: {ex.Message}";
        throw;
    }
}
```

### 5. ConnectorBindings Implementation Structure

Connectors typically split bindings into partial classes:

```
ConnectorBindingsRevit.cs              // Main class definition
ConnectorBindingsRevit.Send.cs         // Send operations
ConnectorBindingsRevit.Receive.cs      // Receive operations
ConnectorBindingsRevit.Selection.cs    // Selection operations
ConnectorBindingsRevit.Events.cs       // Document/app events
ConnectorBindingsRevit.Settings.cs     // Configuration
ConnectorBindingsRevit.Previews.cs     // Preview functionality (optional)
```

**Benefits**:
- Better code organization
- Easier to navigate large implementations
- Logical grouping of related functionality
- Reduces file size and complexity

---

## Connector-Specific Patterns

### Revit Connector Patterns

**Entry Points**:
- `IExternalApplication.OnStartup()` - Plugin initialization
- `IExternalCommand.Execute()` - Ribbon button clicks
- `UpdaterAPI` - Change tracking and updates
- `DocumentClosing`, `ApplicationInitialized` events

**Specific Features**:
- **Type Mapping**: `ElementTypeMapper`, `RevitHostType` for BIM elements
- **Transaction Management**: `TransactionManager` wraps all document changes
- **Document Caching**: `RevitDocumentAggregateCache` for performance
- **Stream Persistence**: Stored in document parameters via `StreamStateManager`
- **Category Filtering**: `Categories` helper for BIM category access

**Version Handling**:
```csharp
// Compile-time version detection
#if REVIT2020
    // Revit 2020-specific code
#elif REVIT2021
    // Revit 2021-specific code
#elif REVIT2025
    // Revit 2025-specific code
#endif
```

**Project Structure**:
```
ConnectorRevit/
├── ConnectorRevit/              # Shared code
│   ├── Entry/                  # App.cs, SpeckleRevitCommand.cs
│   ├── UI/                     # ConnectorBindingsRevit files
│   ├── Storage/                # Document state management
│   └── TypeMapping/            # Type conversion
├── ConnectorRevit2020/         # Version-specific project
├── ConnectorRevit2021/         # Version-specific project
└── ...
```

### Rhino Connector Patterns

**Entry Points**:
- `PlugIn.OnLoad()` - Plugin initialization
- `Panels.RegisterPanel()` - Dockable UI panels
- `RhinoDoc` events - Document operations
- `RhinoApp.Idle` - Periodic checks

**Specific Features**:
- **Cross-Platform**: macOS and Windows support
- **Document Strings**: `RhinoDoc.Strings` for stream metadata
- **Custom Properties**: `UserDictionary` for object data
- **Geometry Conversion**: RhinoCommon geometry directly
- **Headless Support**: Works in Rhino.Compute

**Platform Handling**:
```csharp
#if MAC
    // macOS-specific code (Cocoa integration)
#else
    // Windows-specific code (WPF panels)
#endif
```

### Grasshopper Connector Patterns

**Component Base Classes**:

1. **GH_SpeckleComponent**: Synchronous base
2. **GH_SpeckleTaskCapableComponent**: Task-based (recommended)
3. **GH_SpeckleAsyncComponent**: Async/await support

**Example Component**:
```csharp
public class ToSpeckleComponent : GH_SpeckleTaskCapableComponent
{
    public ToSpeckleComponent() : base("ToSpeckle", "ToSpeckle", "Converts data to Speckle", "Speckle 2", "Conversion")
    {
    }
    
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new GH_SpeckleBaseParam("Object", "Obj", "Object to convert"));
    }
    
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new SpeckleBaseParam("Speckle Object", "Speckle", "Converted Speckle object"));
    }
    
    public override void SolveInstanceWithLogContext(IGH_DataAccess DA)
    {
        var obj = null;
        if (!DA.GetData(0, ref obj)) return;
        
        var speckleObj = Converter.ConvertToSpeckle(obj);
        DA.SetData(0, speckleObj);
    }
}
```

**Component Features**:
- `ComponentCategories.PRIMARY_RIBBON` - Main Speckle tab
- `ISpeckleTrackingComponent` - Analytics tracking
- `GH_SpeckleGoo` - Custom parameter types
- Deferred/async execution support

### AutoCAD/Civil 3D Connector Patterns

**Entry Points**:
- `IExtensionApplication.Initialize()` - Startup
- `ComponentManager.ItemInitialized` - Ribbon ready
- Ribbon button clicks
- `Document` events

**Specific Features**:
- **Ribbon UI**: WPF-based ribbon integration
- **Workspace Handling**: Responds to workspace changes
- **Document Manager**: Access to multiple open documents
- **Database Access**: `Database` objects for geometry

**Assembly Loading**:
```csharp
static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
{
    Assembly a = null;
    var name = args.Name.Split(',')[0];
    string path = Path.GetDirectoryName(typeof(App).Assembly.Location);
    string assemblyFile = Path.Combine(path, name + ".dll");
    
    if (File.Exists(assemblyFile))
    {
        a = Assembly.LoadFrom(assemblyFile);
    }
    
    return a;
}
```

### CSI Connectors (ETABS, SAP2000, CSIBridge) Patterns

**Plugin Loading**:
```csharp
class cPlugin : iPluginCallback
{
    public bool Main(cSapModel model)
    {
        try
        {
            var bindings = new ConnectorBindingsCSI(model);
            var viewModel = new MainViewModel(bindings);
            // Create window and show
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

**API Interaction**:
```csharp
public partial class ConnectorBindingsCSI : ConnectorBindings
{
    public static cSapModel Model { get; set; }
    
    public string GetHostAppName(cSapModel model)
    {
        var name = "";
        var ver = "";
        var type = "";
        model.GetProgramInfo(ref name, ref ver, ref type);
        return name.ToLower();
    }
}
```

**Version Handling**:
```csharp
#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;
#endif
```

**Key Characteristics**:
- Direct COM/C++ API access
- Window integration via `cPluginCallback`
- Model-driven operations
- No traditional UI framework (uses native dialogs or DesktopUI2)

### Dynamo Connector Patterns

**Node Base Class**:
```csharp
[NodeName("Receive")]
[NodeCategory("Speckle 2")]
[NodeDescription("Receive data from a Speckle server")]
[NodeSearchTags("receive", "speckle")]
[IsDesignScriptCompatible]
public class Receive : NodeModel
{
    public override void SetupCustomUIElements(dynNodeUI nodeUI)
    {
        // Custom UI setup
    }
    
    public override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, List<Func<string, object>> lookupFunctions)
    {
        // DesignScript AST generation
    }
}
```

**Key Patterns**:
- Extends `NodeModel` for serialization support
- DesignScript AST generation for evaluation
- Async operations with `OnAsyncTaskInvoked`
- UI binding via properties and events

---

## Implementation Guide

### Creating a New Connector

#### Step 1: Create Project Structure

```
ConnectorNewApp/
├── ConnectorNewApp/                    # Shared code
│   ├── Entry/
│   │   ├── App.cs                    # IExtensionApplication/IExternalApplication
│   │   └── NewAppCommand.cs          # Main command
│   ├── UI/
│   │   ├── ConnectorBindingsNewApp.cs
│   │   ├── ConnectorBindingsNewApp.Send.cs
│   │   ├── ConnectorBindingsNewApp.Receive.cs
│   │   ├── ConnectorBindingsNewApp.Selection.cs
│   │   └── ConnectorBindingsNewApp.Events.cs
│   └── Storage/
│       └── StreamStateManager.cs     # Persist stream data
└── ConnectorNewApp.csproj
```

#### Step 2: Implement ConnectorBindings

```csharp
public partial class ConnectorBindingsNewApp : ConnectorBindings
{
    private static NewAppDocument _document;
    
    public ConnectorBindingsNewApp(NewAppDocument doc)
    {
        _document = doc;
    }
    
    // Implement all abstract methods
    public override string GetHostAppName() => "NewApp";
    public override string GetHostAppNameVersion() => "NewApp 2024";
    public override string GetFileName() => _document.Name;
    public override string GetDocumentId() => _document.Id.ToString();
    public override string GetDocumentLocation() => _document.FilePath;
    public override string GetActiveViewName() => _document.ActiveView?.Name ?? "Default";
    
    public override List<StreamState> GetStreamsInFile()
    {
        return StreamStateManager.ReadStreams(_document);
    }
    
    public override void WriteStreamsToFile(List<StreamState> streams)
    {
        StreamStateManager.WriteStreams(_document, streams);
    }
    
    // ... implement other methods
}
```

#### Step 3: Implement Entry Point

```csharp
public class App : IExtensionApplication
{
    public void Initialize()
    {
        try
        {
            var doc = NewApp.GetActiveDocument();
            var bindings = new ConnectorBindingsNewApp(doc);
            
            // Initialize Speckle
            Setup.Init(bindings.GetHostAppNameVersion(), bindings.GetHostAppName());
            
            // Initialize UI
            bindings.RegisterAppEvents();
            NewAppCommand.Bindings = bindings;
            NewAppCommand.InitAvalonia();
        }
        catch (Exception ex)
        {
            SpeckleLog.Logger.Fatal(ex, "Failed to initialize Speckle");
        }
    }
    
    public void Terminate() { }
}
```

#### Step 4: Register Commands and UI

```csharp
public class NewAppCommand : IExternalCommand
{
    public static ConnectorBindingsNewApp Bindings { get; set; }
    
    public void Execute()
    {
        if (MainWindow == null)
        {
            var viewModel = new MainViewModel(Bindings);
            MainWindow = new MainWindow { DataContext = viewModel };
        }
        
        MainWindow.Show();
    }
}
```

#### Step 5: Handle Conversion

```csharp
public override async Task<string> SendStream(StreamState state, ProgressViewModel progress)
{
    var converter = KitManager.GetDefaultKit().LoadConverter(GetHostAppName());
    var selectedObjects = GetSelectedObjects();
    var speckleObjects = selectedObjects.Select(id => converter.ConvertToSpeckle(id)).ToList();
    
    var client = new Client(state.Account);
    return await client.CommitObjects(state.StreamId, speckleObjects);
}
```

---

## Version Management

### Approach 1: Separate Projects per Version (Revit, AutoCAD)

Each version has its own thin project referencing shared code:

**ConnectorRevit2025.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <DefineConstants>$(DefineConstants);REVIT2025</DefineConstants>
  </PropertyGroup>
  
  <Import Project="..\ConnectorRevit\ConnectorRevit.projitems" Label="Shared" />
</Project>
```

**Shared Code** (ConnectorRevit.projitems):
```csharp
#if REVIT2025
    // Revit 2025-specific code
#elif REVIT2024
    // Revit 2024-specific code
#endif
```

**Advantages**:
- Supports multiple versions simultaneously
- Each version can have specific dependencies
- Clean separation of concerns

**Disadvantages**:
- Requires conditional compilation
- Multiple projects to maintain
- Build complexity

### Approach 2: Single Project with Version Detection (Rhino, Grasshopper)

Single project detects version at runtime:

```csharp
public static string GetRhinoHostAppVersion()
{
    return RhinoApp.Version.Major switch
    {
        6 => HostApplications.Rhino.GetVersion(HostAppVersion.v6),
        7 => HostApplications.Rhino.GetVersion(HostAppVersion.v7),
        8 => HostApplications.Rhino.GetVersion(HostAppVersion.v8),
        _ => throw new NotSupportedException($"Rhino {RhinoApp.Version.Major} not supported")
    };
}
```

**Advantages**:
- Single distribution package
- Simpler build process
- Works with different installed versions

**Disadvantages**:
- Runtime version detection required
- API differences need reflection/dynamic handling
- Limited to compatible API versions

### Approach 3: Unified with Version-Specific NuGet Packages (CSI)

Use conditional compilation with version-specific API packages:

**ConnectorCSIBridge25.csproj**:
```xml
<ItemGroup>
  <PackageReference Include="CSI.API" Version="25.*" />
</ItemGroup>
<PropertyGroup>
  <DefineConstants>$(DefineConstants);ETABS22</DefineConstants>
</PropertyGroup>
```

---

## Integration with Objects and Converters

### Object Conversion Flow

```
Host Application Object
        ↓
  [Converter.ConvertToSpeckle]
        ↓
  Speckle Base Object
        ↓
 [Serialization]
        ↓
  JSON/BSON
        ↓
 [Upload to Server/Stream]
```

### Converter Loading and Usage

```csharp
// Load converter for application
var converter = KitManager.GetDefaultKit().LoadConverter(GetHostAppName());

// Convert single object
var speckleObject = converter.ConvertToSpeckle(nativeObject);

// Convert collection
var speckleObjects = nativeObjects.Select(obj => converter.ConvertToSpeckle(obj)).ToList();

// Receive and convert
var nativeObject = converter.ConvertToNative(speckleObject);
```

### Converter Implementation Pattern

Converters inherit from `ISpeckleConverter`:

```csharp
public interface ISpeckleConverter
{
    public string Name { get; set; }
    public string Description { get; set; }
    
    // To Speckle
    public object ConvertToSpeckle(object @object);
    public List<object> ConvertToSpeckle(List<object> objects);
    
    // To Native
    public object ConvertToNative(object @object);
    public List<object> ConvertToNative(List<object> objects);
    
    // Settings and features
    public bool CanConvertToSpeckle(object @object);
    public bool CanConvertToNative(object @object);
}
```

### Objects Package Integration

The `Objects` package provides:
- Schema definitions for common BIM/CAD elements
- Version-specific object types (e.g., `RevitWall`, `RhinoMesh`)
- Built-in converters for standard geometries
- Serialization support

**Usage**:
```csharp
// Create typed Speckle objects
var wall = new RevitWall 
{ 
    height = 3.5,
    baseOffset = 0,
    family = "Basic Wall",
    type = "Generic - 200mm"
};

// Objects automatically handle serialization
var json = JsonConvert.SerializeObject(wall);
```

### Stream State and Persistence

Objects stored with reference metadata:

```csharp
public class StreamState
{
    public string StreamId { get; set; }
    public string CommitId { get; set; }
    public string BranchName { get; set; }
    public string Name { get; set; }
    public DateTime LastReceive { get; set; }
    public ReceiveMode ReceiveMode { get; set; }
    public List<string> ReceiveObjects { get; set; }
    public List<string> SendObjects { get; set; }
}
```

Persisted in document via:
- **Revit**: Document parameters
- **Rhino**: `RhinoDoc.Strings`
- **AutoCAD**: Extended data
- **Grasshopper**: Component properties
- **CSI**: Model data store

---

## Best Practices

### Error Handling

1. **Use structured logging**:
   ```csharp
   SpeckleLog.Logger.Error(ex, "Operation failed: {operation}", operationName);
   ```

2. **Provide user feedback**:
   ```csharp
   progress.Message = $"Error: {ex.Message}";
   ```

3. **Handle DLL conflicts**:
   ```csharp
   AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
   ```

### Performance Considerations

1. **Cache converters**:
   ```csharp
   private ISpeckleConverter _converter;
   public ISpeckleConverter Converter => 
       _converter ??= KitManager.GetDefaultKit().LoadConverter(HostAppName);
   ```

2. **Use async for long operations**:
   ```csharp
   public override async Task<string> SendStream(...)
   ```

3. **Batch operations**:
   ```csharp
   var objects = GetSelectedObjects();
   var converted = objects.Select(ConvertObject).ToList();  // Parallel if safe
   ```

### Thread Safety

1. **Use host app's thread for API calls**:
   ```csharp
   RevitTask.RunAsync(() => {
       // Revit API calls only on Revit thread
   });
   ```

2. **Synchronize state access**:
   ```csharp
   lock (_streamStates) {
       // Update shared state
   }
   ```

### Testing

1. **Mock ConnectorBindings for UI testing**:
   ```csharp
   var mockBindings = new Mock<ConnectorBindings>();
   mockBindings.Setup(b => b.GetSelectedObjects()).Returns(new List<string>());
   ```

2. **Test conversion separately**:
   ```csharp
   var converter = new CustomConverter();
   var result = converter.ConvertToSpeckle(testObject);
   Assert.NotNull(result);
   ```

---

## Resources

- **DesktopUI2**: `/DesktopUI2/DesktopUI2/` - Shared UI framework
- **Objects**: Core package providing object schemas
- **Core**: `/Core/Core/` - Speckle core functionality
- **Analytics**: Integrated via `Speckle.Core.Logging.Analytics`
- **Kit Manager**: `KitManager` for converter discovery and loading

---

## Troubleshooting

### Common Issues

**DLL Conflicts**: Implement assembly resolution in plugin initialization
**Converter Not Found**: Ensure kit package includes converter for app
**Thread Access**: Host APIs may require specific thread context
**Version Mismatch**: Check DefineConstants and version-specific code paths

For additional help, visit [Speckle Community Forum](https://speckle.community)

