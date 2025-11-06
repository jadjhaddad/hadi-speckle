# DesktopUI2 - Speckle Cross-Platform Desktop User Interface

## Overview

DesktopUI2 is a cross-platform desktop user interface for Speckle connectors built with **Avalonia**, **ReactiveUI**, and **MVVM** architectural pattern. It provides a standardized UI framework that allows different connector implementations (Revit, Rhino, Navisworks, Bentley, CSIBridge, etc.) to integrate seamlessly with a modern, reactive, and data-driven interface.

## Technology Stack

### Core Framework
- **Avalonia 0.10.18**: Cross-platform XAML-based UI framework (Windows, Mac, Linux)
- **Avalonia.Desktop**: Desktop platform support
- **Avalonia.ReactiveUI**: Integration between Avalonia and ReactiveUI
- **Avalonia.Diagnostics**: Development-time diagnostics and debugging

### State Management & Reactivity
- **ReactiveUI**: Modern reactive extensions framework based on System.Reactive
  - Implements MVVM pattern with automatic property change notifications
  - Event-driven architecture using Observables
  - Enables declarative data bindings

### Styling & Design
- **Material Design**: Material.Styles (Material Design for Avalonia)
- **Material Icons**: Material.Icons.Avalonia for consistent iconography
- **NetTopologySuite 2.5.0**: Geometry and spatial operations
- **Speckle Color Scheme**: Primary (Blue #3B82F6), Secondary (Light Blue #83B4FF), Accent (Gold #FFBF00)

### Target Framework
- **.NET Standard 2.0**: Cross-platform library compatibility
- Distributed as NuGet package `Speckle.DesktopUI`

## Project Structure

```
DesktopUI2/
├── App.xaml                          # Application root, theme setup
├── App.xaml.cs                       # Application initialization
├── ConnectorBindings.cs              # Abstract interface for connector integration
├── ConnectorHelpers.cs               # Shared helper utilities for connectors
├── ViewLocator.cs                    # Dynamic view/viewmodel resolution
├── Utils.cs                          # Utility functions
├── DummyBindings.cs                  # Test/fallback bindings
├── Models/
│   ├── StreamState.cs                # Individual stream card state
│   ├── StreamAccountWrapper.cs       # Account + Stream container
│   ├── NotificationManager.cs        # Toast notifications system
│   ├── ConfigManager.cs              # Application configuration persistence
│   ├── MenuItem.cs                   # Custom menu item definitions
│   ├── Filters/                      # Selection filtering system
│   │   ├── ISelectionFilter.cs       # Base filter interface
│   │   ├── AllSelectionFilter.cs     # All objects
│   │   ├── ManualSelectionFilter.cs  # User-selected objects
│   │   ├── ListSelectionFilter.cs    # Predefined list of objects
│   │   ├── PropertySelectionFilter.cs# Property-based filtering
│   │   ├── TreeSelectionFilter.cs    # Tree hierarchy filtering
│   │   └── SelectionFilterConverter.cs # JSON serialization
│   ├── Settings/                     # Connector settings system
│   │   ├── ISetting.cs               # Base setting interface
│   │   ├── TextBoxSetting.cs         # Text input
│   │   ├── CheckBoxSetting.cs        # Boolean toggle
│   │   ├── ListBoxSetting.cs         # Single selection dropdown
│   │   ├── MultiSelectBoxSetting.cs  # Multiple selection dropdown
│   │   ├── NumericSetting.cs         # Number input
│   │   ├── MappingSetting.cs         # Type mapping definition
│   │   └── SettingsConverter.cs      # JSON serialization
│   ├── Scheduler/                    # Task scheduling
│   │   └── Trigger.cs                # Scheduled operation trigger
│   └── TypeMappingOnReceive/         # Receive type mapping
│       ├── ITypeMap.cs
│       ├── HostType.cs
│       ├── MappingValue.cs
│       └── ... (type mapping models)
├── ViewModels/                       # Core MVVM layer
│   ├── ViewModelBase.cs              # Base reactive object
│   ├── MainViewModel.cs              # Root viewmodel, navigation, dialogs
│   ├── HomeViewModel.cs              # Saved streams list, stream management
│   ├── StreamViewModel.cs            # Individual stream card state
│   ├── StreamSelectorViewModel.cs    # Stream selection UI
│   ├── BranchViewModel.cs            # Branch/model selection
│   ├── FilterViewModel.cs            # Filter configuration
│   ├── SettingViewModel.cs           # Individual setting binding
│   ├── SettingsPageViewModel.cs      # Settings page collection
│   ├── ProgressViewModel.cs          # Send/Receive progress tracking
│   ├── AccountViewModel.cs           # Account management
│   ├── LogInViewModel.cs             # Authentication
│   ├── OneClickViewModel.cs          # One-click mode
│   ├── ActivityViewModel.cs          # Stream activity/history
│   ├── CollaboratorsViewModel.cs     # Collaborator management
│   ├── CommentViewModel.cs           # Comments display
│   ├── NotificationViewModel.cs      # Individual notification
│   ├── NotificationsViewModel.cs     # Notification center
│   ├── DialogViewModel.cs            # Base dialog handler
│   ├── TypeMappingOnReceiveViewModel.cs # Receive type mapping UI
│   ├── ApplicationObjectViewModel.cs # Object conversion tracking
│   ├── ImportFamiliesDialogViewModel.cs # Family import dialog
│   ├── MenuItemViewModel.cs          # Menu item representation
│   ├── SchedulerViewModel.cs         # Task scheduling UI
│   └── DesignViewModels/             # Design-time data for XAML preview
│       ├── DesignHomeViewModel.cs
│       ├── DesignNotificationsViewModel.cs
│       ├── ... (design-time VMs)
├── Views/                            # XAML UI layer
│   ├── MainWindow.xaml               # Root window
│   ├── MainUserControl.xaml          # Root control (router + dialog + notifications)
│   ├── Pages/
│   │   ├── HomeView.xaml/cs          # Saved streams list page
│   │   ├── StreamEditView.xaml/cs    # Stream details page
│   │   ├── LogInView.xaml/cs         # Authentication page
│   │   ├── OneClickView.xaml/cs      # One-click mode
│   │   ├── SettingsView.xaml/cs      # Settings page
│   │   ├── NotificationsView.xaml/cs # Notification center
│   │   └── CollaboratorsView.xaml/cs # Collaborators page
│   ├── Controls/
│   │   ├── StreamSelector.xaml       # Stream/account selector
│   │   ├── StreamDetails.xaml        # Stream info display
│   │   ├── SavedStreams.xaml         # Saved streams list
│   │   ├── StreamEditControls/
│   │   │   ├── Send.xaml             # Send operation UI
│   │   │   ├── Receive.xaml          # Receive operation UI
│   │   │   ├── Activity.xaml         # Stream activity
│   │   │   ├── Comments.xaml         # Comments display
│   │   │   └── Report.xaml           # Operation report
│   │   ├── CollaboratorsControl.xaml # Collaborators list
│   │   └── PreviewButton.xaml        # Preview button component
│   ├── Filters/
│   │   ├── AllFilterView.xaml        # All objects filter UI
│   │   ├── ManualFilterView.xaml     # Manual selection filter UI
│   │   ├── ListFilterView.xaml       # List filter UI
│   │   ├── PropertyFilterView.xaml   # Property filter UI
│   │   └── TreeFilterView.xaml       # Tree filter UI
│   ├── Settings/
│   │   ├── TextBoxSettingView.xaml   # Text setting UI
│   │   ├── CheckBoxSettingView.xaml  # Boolean setting UI
│   │   ├── ListBoxSettingView.xaml   # Dropdown setting UI
│   │   ├── MultiSelectBoxSettingView.xaml # Multi-select setting UI
│   │   └── NumericSettingView.xaml   # Numeric setting UI
│   ├── Windows/Dialogs/              # Modal dialogs
│   │   ├── Dialog.xaml               # Base dialog wrapper
│   │   ├── AddAccountDialog.xaml
│   │   ├── AddFromUrlDialog.xaml
│   │   ├── NewStreamDialog.xaml
│   │   ├── NewBranchDialog.xaml
│   │   ├── ImportFamiliesDialog.xaml
│   │   ├── MappingViewDialog.xaml
│   │   ├── MissingIncomingTypesDialog.xaml
│   │   ├── ImportExportAlert.xaml
│   │   ├── ChangeRoleDialog.xaml
│   │   └── QuickOpsDialog.xaml
│   ├── Scheduler.xaml                # Scheduler UI
│   ├── Share.xaml                    # Sharing UI
│   ├── MappingsControl.xaml          # Mappings editor
│   └── MappingsWindow.xaml
├── Styles/                           # XAML styles and templates
│   ├── Styles.xaml                   # Global styles
│   ├── Text.xaml                     # Typography
│   ├── Button.xaml                   # Button styles
│   ├── ComboBox.xaml                 # ComboBox styles
│   ├── ListBox.xaml                  # ListBox styles
│   ├── ChipListBox.xaml              # Chip list styles
│   ├── TextBox.xaml                  # TextBox styles
│   ├── MenuItem.xaml                 # Menu item styles
│   ├── Dialog.xaml                   # Dialog styles
│   ├── Expander.xaml                 # Expander styles
│   ├── FloatingButton.xaml           # Floating action button
│   ├── NotificationCard.xaml         # Toast notification card
│   ├── NotificationManager.xaml      # Notification container
│   └── Playground.xaml               # Design testing
├── Assets/                           # Resources
│   ├── SpaceGrotesk-VariableFont_wght.ttf
│   ├── icon.ico
│   ├── logo.png
│   ├── instructions.gif
│   └── mapping.png
└── DesktopUI2.csproj                 # Project configuration

```

## Key Features

### 1. Stream/Branch/Commit Selection UI

**StreamViewModel** manages a single stream card displayed in the home view:

```csharp
public class StreamViewModel : ReactiveObject
{
    public StreamState StreamState { get; set; }      // Persistent state
    public Stream Stream { get; set; }                // Live API data
    public Client Client { get; set; }                // API client
    public bool IsReceiver { get; set; }              // Receive mode flag
    public ReceiveMode SelectedReceiveMode { get; set; }
    
    // Branch selection (dynamically loaded from API)
    public ObservableCollection<BranchViewModel> BranchesViewModel { get; }
    public BranchViewModel SelectedBranch { get; set; }
}
```

**Key Features**:
- Real-time branch/commit list fetching from Speckle server
- Caching of stream metadata for offline access
- Role-based access control (reviewer vs. contributor vs. owner)
- Automatic role detection to restrict UI capabilities

### 2. Send/Receive Operations

**Send Flow**:
1. User selects a branch in the Send control
2. Configures selection filter (which objects to send)
3. Applies connector-specific settings
4. Executes `ConnectorBindings.SendStream(StreamState, ProgressViewModel)`
5. Progress updates flow through ProgressViewModel

**Receive Flow**:
1. User selects a branch and commit in the Receive control
2. Optionally configures receive mode (transform, replace, etc.)
3. Executes `ConnectorBindings.ReceiveStream(StreamState, ProgressViewModel)`
4. Type mapping dialog appears if needed (CSIBridge, Revit)
5. Progress tracking and error reporting

**ProgressViewModel**:
```csharp
public class ProgressViewModel : ReactiveObject
{
    public CancellationTokenSource CancellationTokenSource { get; set; }
    public CancellationToken CancellationToken { get; }
    public ProgressReport Report { get; set; }
    public double Value { get; set; }                 // Current progress
    public double Max { get; set; }                   // Total items
    public string ProgressTitle { get; set; }
    public string ProgressSummary { get; set; }
    public bool IsProgressing { get; set; }
}
```

### 3. Filters and Selection Sets

Filters control which objects get sent. They are connector-specific and implement `ISelectionFilter`:

```csharp
public interface ISelectionFilter
{
    string Name { get; set; }                        // Display name
    string Type { get; }                             // Discriminator
    string Icon { get; set; }                        // Material icon name
    string Slug { get; set; }                        // Unique identifier
    string Summary { get; }                          // "5 objects selected"
    string Description { get; set; }                 // Help text
    List<string> Selection { get; set; }             // Selected object IDs
    Type ViewType { get; }                           // Associated UI control
}
```

**Built-in Filter Types**:
- **AllSelectionFilter**: Send/receive all objects
- **ManualSelectionFilter**: User manually selects objects from host app
- **ListSelectionFilter**: Predefined list of object IDs
- **PropertySelectionFilter**: Filter by object properties (e.g., "Category == Walls")
- **TreeSelectionFilter**: Hierarchical tree-based selection

Each filter has a corresponding View (ManualFilterView, ListFilterView, etc.) that displays in the Send/Receive panels.

### 4. Settings Management

Connector-specific settings are returned by `GetSettings()` and implement `ISetting`:

```csharp
public interface ISetting
{
    string Name { get; set; }                        // "Export Families"
    string Icon { get; set; }                        // Material icon
    string Slug { get; set; }                        // "export_families"
    string Type { get; }                             // Discriminator
    string Summary { get; }                          // "Enabled" / "Disabled"
    string Description { get; set; }                 // Help text
    string Selection { get; set; }                   // Current value
    Type ViewType { get; }                           // Associated UI control
}
```

**Built-in Setting Types**:
- **TextBoxSetting**: Free text input
- **CheckBoxSetting**: Boolean toggle
- **ListBoxSetting**: Single-selection dropdown
- **MultiSelectBoxSetting**: Multi-selection dropdown
- **NumericSetting**: Integer/decimal input
- **MappingSetting**: Type mapping configuration

Settings are serialized to StreamState and persist with the document.

## Integration Pattern

### How Connectors Embed and Communicate with DesktopUI2

#### 1. Create ConnectorBindings Implementation

Each connector creates a class extending `ConnectorBindings`:

```csharp
public class ConnectorBindingsRevit : ConnectorBindings
{
    // Properties required by connector
    private UIApplication _revitApp;
    private Document _revitDoc;
    
    // Constructor receives connector context
    public ConnectorBindingsRevit(UIApplication revitApp, Document revitDoc)
    {
        _revitApp = revitApp;
        _revitDoc = revitDoc;
    }
    
    // Implement abstract methods...
}
```

#### 2. Connector-Specific Methods

These are overridden by each connector to provide native app integration:

```csharp
// Document information
public override string GetHostAppNameVersion() => "Revit 2024";
public override string GetHostAppName() => "revit";
public override string GetFileName() => _revitDoc.Title;
public override string GetDocumentId() => _revitDoc.PathName.GetHashCode().ToString();
public override string GetDocumentLocation() => Path.GetDirectoryName(_revitDoc.PathName);

// View management
public override string GetActiveViewName() => _revitDoc.ActiveView.Name;

// Object selection
public override List<string> GetSelectedObjects()
{
    return _revitDoc.Selection.GetElementIds()
        .Select(id => id.ToString())
        .ToList();
}

public override void SelectClientObjects(List<string> objs, bool deselect = false)
{
    var ids = objs.Select(id => new ElementId(int.Parse(id))).ToList();
    _revitDoc.Selection.SetElementIds(ids);
}

// Stream persistence
public override List<StreamState> GetStreamsInFile()
{
    // Read from Revit project parameters, DB, or file
}

public override void WriteStreamsToFile(List<StreamState> streams)
{
    // Write to Revit project parameters, DB, or file
}

// Core operations
public override async Task<string> SendStream(StreamState state, ProgressViewModel progress)
{
    // 1. Get objects using selected filter
    // 2. Convert to Speckle using kit
    // 3. Send to server via ConnectorHelpers
}

public override async Task<StreamState> ReceiveStream(StreamState state, ProgressViewModel progress)
{
    // 1. Fetch commit from server via ConnectorHelpers
    // 2. Convert from Speckle using kit
    // 3. Place objects in Revit document
}
```

#### 3. Register Bindings with Application

The connector must register its bindings with the UI:

```csharp
// In connector startup/initialization
var bindings = new ConnectorBindingsRevit(_revitApp, _revitDoc);

// Pass to UI (implementation-specific, often via separate process bridge)
MainViewModel.Instance.Bindings = bindings;
```

### Communication Bridge Pattern

For **out-of-process connectors** (like CSIBridge), communication happens via:

1. **Named pipes or sockets** for IPC between connector and DesktopUI2
2. **Event delegates** for callbacks:
   - `UpdateSavedStreams` - Fired when document streams change
   - `UpdateSelectedStream` - Fired when active stream changes
3. **File-based persistence** for StreamState (project parameters, xml, or custom format)

Example from CSIBridge:

```csharp
// DriverCSharp.exe (separate process) hosts DesktopUI2
var mainViewModel = new MainViewModel(new CSIBridgeBindings());

// CSIBridgeBindings registers callbacks
bindings.UpdateSavedStreams += (streams) =>
{
    // Persist streams back to CSI document
};

bindings.UpdateSelectedStream += () =>
{
    // UI notified of selection changes
};
```

## Bindings System (Avalonia + ReactiveUI)

### XAML Data Binding

DesktopUI2 uses Avalonia's XAML binding syntax with ReactiveUI reactive properties:

```xaml
<!-- Two-way binding to ViewModel property -->
<TextBox Text="{Binding StreamName, Mode=TwoWay}" />

<!-- One-way binding with automatic updates -->
<TextBlock Text="{Binding StreamId}" />

<!-- Binding to command (ICommand) -->
<Button Command="{Binding SendCommand}" Content="Send" />

<!-- Binding with value conversion -->
<ProgressBar Value="{Binding Progress.Value, Mode=OneWay}" Maximum="{Binding Progress.Max}" />

<!-- Binding with fallback -->
<TextBlock Text="{Binding SelectedBranch.Name, FallbackValue='No branch selected'}" />

<!-- Binding to collections -->
<ListBox Items="{Binding SavedStreams}" SelectedItem="{Binding SelectedStream, Mode=TwoWay}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Stream.name}" />
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

### Reactive Property Changes

ViewModels inherit from `ViewModelBase : ReactiveObject`:

```csharp
public class HomeViewModel : ReactiveObject
{
    private bool _isLoading;
    
    // Property that raises change notifications
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }
    
    // Reactive command
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    
    public HomeViewModel()
    {
        RefreshCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            IsLoading = true;
            await RefreshStreamsAsync();
            IsLoading = false;
        });
    }
}
```

### View Registration

The `ViewLocator` automatically maps ViewModels to Views by convention:

```csharp
// ViewLocator.cs
public class ViewLocator : IDataTemplate
{
    public IControl Build(object data)
    {
        // HomeViewModel -> DesktopUI2.Views.Pages.HomeView
        var name = data.GetType().FullName.Replace("ViewModel", "View");
        var type = Type.GetType(name);
        return (Control)Activator.CreateInstance(type);
    }
}
```

Usage in XAML:
```xaml
<ContentControl Content="{Binding CurrentViewModel}" />
<!-- Automatically renders appropriate View -->
```

## State Management (ReactiveUI Patterns)

### Observable Collections

For dynamic lists:

```csharp
public ObservableCollection<StreamViewModel> SavedStreams { get; } = new();

// When updating streams from connector
internal void UpdateSavedStreams(List<StreamState> states)
{
    SavedStreams.Clear();
    foreach (var state in states)
    {
        SavedStreams.Add(new StreamViewModel(state, HostScreen));
    }
    this.RaisePropertyChanged(nameof(SavedStreams));
}
```

### Reactive Commands

For user actions:

```csharp
// Simple command
public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

// Command with parameter
public ReactiveCommand<string, Unit> SelectStreamCommand { get; }

// Command with validation
public ReactiveCommand<Unit, Unit> SendCommand { get; }

// In constructor
SendCommand = ReactiveCommand.CreateFromTask(
    async () => await SendStreamAsync(),
    canExecute: this.WhenAnyValue(
        x => x.SelectedStream,
        x => x.IsProgressing,
        (stream, progressing) => stream != null && !progressing
    )
);
```

### Navigation (Router)

The app uses `RoutingState` for page navigation:

```csharp
// MainViewModel
public RoutingState Router { get; private set; }

// Navigate to a page
Router.Navigate.Execute(new HomeViewModel(this));

// Go back
Router.NavigateBack.Execute();

// In XAML
<reactiveUi:RoutedViewHost Router="{Binding Router}">
    <reactiveUi:RoutedViewHost.PageTransition>
        <CrossFade Duration="0.15" />
    </reactiveUi:RoutedViewHost.PageTransition>
</reactiveUi:RoutedViewHost>
```

### Dialog Management

Modals are managed through `MainViewModel`:

```csharp
// Show dialog
MainViewModel.Instance.DialogBody = new SomeDialogView 
{ 
    DataContext = new SomeDialogViewModel() 
};

// Close dialog
MainViewModel.CloseDialog();

// Detect dialog visibility
IsVisible="{Binding DialogVisible}"
```

## Connector Integration - Detailed Flow

### Instantiation

```csharp
// 1. Connector creates bindings instance
var bindings = new ConnectorBindingsRevit(revitApp, revitDoc);

// 2. Create MainViewModel with bindings
var mainViewModel = new MainViewModel(bindings);

// 3. Resolve bindings for dependency injection
Locator.CurrentMutable.Register(() => bindings, typeof(ConnectorBindings));

// 4. Create UI window/control
var mainWindow = new MainWindow { DataContext = mainViewModel };
mainWindow.Show();
```

### Stream Initialization Flow

```
1. Application starts
2. MainViewModel.Init() called
3. Checks if user is logged in
4. If yes, navigates to HomeViewModel
5. HomeViewModel calls Bindings.GetStreamsInFile()
6. Connector loads streams from document
7. Each stream becomes a StreamViewModel
8. UI renders stream cards in SavedStreams list
```

### Send Operation Flow

```
StreamViewModel.SendCommand clicked
    ↓
UI: User selects branch from BranchesViewModel
    ↓
UI: User selects filter (which objects to send)
    ↓
UI: User clicks "Send" button
    ↓
ProgressViewModel created, dialog shown
    ↓
Bindings.SendStream(StreamState, ProgressViewModel) called
    ↓
Connector implementation:
  1. Gets selected objects from filter
  2. Converts objects using kit/converter
  3. Sends using ConnectorHelpers.CreateCommit()
  4. Updates ProgressViewModel.Value during send
  ↓
Progress dialog updates in real-time
    ↓
On completion: Notification shown, commit recorded
```

### Receive Operation Flow

```
StreamViewModel.ReceiveCommand clicked
    ↓
UI: User selects branch and commit
    ↓
UI: User configures receive mode (if applicable)
    ↓
ProgressViewModel created, dialog shown
    ↓
Bindings.ReceiveStream(StreamState, ProgressViewModel) called
    ↓
Connector implementation:
  1. Fetches commit using ConnectorHelpers.GetCommitFromState()
  2. Deserializes objects from server
  3. Converts using kit/converter
  4. Determines placement strategy (new, update, transform)
  5. Places objects in document
  6. Updates ProgressViewModel during operation
  ↓
If type mapping required (CSIBridge):
  → MissingIncomingTypesDialog shown
  → User maps types
  → Operation completes
    ↓
Progress dialog updates
    ↓
On completion: Notification shown
```

## Customization Points

### 1. Custom Filters

Create a new filter class:

```csharp
public class CustomFilter : ISelectionFilter
{
    public string Name { get; set; } = "Custom Filter";
    public string Type => typeof(CustomFilter).FullName;
    public string Icon { get; set; } = "FilterVariant";
    public string Slug { get; set; } = "custom";
    public string Description { get; set; } = "Filter objects by custom logic";
    public List<string> Selection { get; set; } = new();
    public Type ViewType => typeof(CustomFilterView);
    
    public string Summary => $"{Selection.Count} objects selected";
}

// Return from GetSelectionFilters()
public override List<ISelectionFilter> GetSelectionFilters()
{
    return new()
    {
        new CustomFilter(),
        new ManualSelectionFilter(),
    };
}
```

Create the associated View:

```xaml
<!-- CustomFilterView.xaml -->
<UserControl x:Class="MyConnector.Views.CustomFilterView"
    xmlns="https://github.com/avaloniaui">
    <!-- Custom UI for this filter -->
</UserControl>
```

### 2. Custom Settings

```csharp
public override List<ISetting> GetSettings()
{
    return new()
    {
        new CheckBoxSetting
        {
            Name = "Export Families",
            Slug = "export_families",
            Icon = "FamilyTree",
            Description = "Include Revit families in export",
            Selection = "true"
        },
        new NumericSetting
        {
            Name = "Conversion Tolerance",
            Slug = "conversion_tolerance",
            Icon = "Ruler",
            Description = "Geometry tolerance in millimeters",
            Selection = "0.01"
        },
    };
}
```

### 3. Custom Menu Items

```csharp
public override List<MenuItem> GetCustomStreamMenuItems()
{
    return new()
    {
        new MenuItem
        {
            Name = "Export to DWG",
            Icon = "FileExport",
            Action = () => ExportToDwg(selectedStream)
        },
    };
}
```

### 4. Preview Support

If connector supports previewing before send/receive:

```csharp
public override bool CanPreviewSend => true;

public override void PreviewSend(StreamState state, ProgressViewModel progress)
{
    // Highlight objects that would be sent without actually sending
    var objectsToSend = GetObjectsFromFilter(state.Filter);
    HighlightInHostApp(objectsToSend);
}

public override bool CanPreviewReceive => true;

public override async Task<StreamState> PreviewReceive(StreamState state, ProgressViewModel progress)
{
    // Fetch and convert but don't place in document
    var commit = await ConnectorHelpers.GetCommitFromState(state);
    var objects = await ReceiveCommit(commit, state, progress);
    // Preview in 3D view or temporary layer
    return state; // Return updated state
}
```

### 5. 3D View Integration

```csharp
public override bool CanOpen3DView => true;

public override async Task Open3DView(List<double> viewCoordinates, string viewName = "")
{
    // viewCoordinates: [camX, camY, camZ, targetX, targetY, targetZ]
    // Open 3D view in host app with given camera position
    var camera = new Point3D(viewCoordinates[0], viewCoordinates[1], viewCoordinates[2]);
    var target = new Point3D(viewCoordinates[3], viewCoordinates[4], viewCoordinates[5]);
    _hostApp.Set3DView(camera, target, viewName);
}
```

## Cross-Platform Considerations

### Platform-Specific Code

Use conditional compilation:

```csharp
#if WIN
    // Windows-specific implementation
    var result = WindowsNativeMethod();
#elif MAC
    // macOS-specific implementation
    var result = MacNativeMethod();
#elif LINUX
    // Linux-specific implementation
    var result = LinuxNativeMethod();
#endif
```

### Path Handling

Always use platform-independent path methods:

```csharp
// Good
var path = Path.Combine(basePath, fileName);

// Bad
var path = basePath + "\\" + fileName;  // Windows-only
```

### File Operations

Use `System.IO` for cross-platform file access:

```csharp
// Reading config
var configPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Speckle",
    "config.json"
);
var config = File.ReadAllText(configPath);
```

### Threading

Avalonia/ReactiveUI handle UI thread marshaling automatically:

```csharp
// Safe to call from any thread
this.RaisePropertyChanged(nameof(MyProperty));

// For long operations, use Task
ReactiveCommand.CreateFromTask(async () =>
{
    await LongRunningOperation();
    // UI updates automatically
});
```

## Key Classes and Their Responsibilities

| Class | Responsibility |
|-------|-----------------|
| **ConnectorBindings** | Abstract interface between DesktopUI2 and host application |
| **MainViewModel** | Root application state, navigation, dialogs, theme |
| **HomeViewModel** | Saved streams list, stream CRUD, filtering |
| **StreamViewModel** | Individual stream card state and operations |
| **FilterViewModel** | Filter configuration and object selection |
| **SettingViewModel** | Setting value binding and persistence |
| **ProgressViewModel** | Progress tracking, cancellation, error handling |
| **StreamState** | Persistent stream configuration (serialized to document) |
| **NotificationManager** | Toast notification display and queueing |
| **ConnectorHelpers** | Shared static utilities for common operations |
| **ViewLocator** | Convention-based View/ViewModel resolution |

## Best Practices

1. **Always Implement Progress Updates**: Use `ProgressViewModel` callbacks during send/receive:
   ```csharp
   progress.Update(new Dictionary<string, int> { ["Objects Sent"] = count });
   ```

2. **Handle Cancellation**: Check `progress.CancellationToken`:
   ```csharp
   progress.CancellationToken.ThrowIfCancellationRequested();
   ```

3. **Cache Expensive Operations**: StreamState has `CachedStream` for API results

4. **Use Dependency Injection**: Access bindings via Locator:
   ```csharp
   var bindings = Locator.Current.GetService<ConnectorBindings>();
   ```

5. **Persist Settings with StreamState**: Settings are serialized together with stream data

6. **Provide Meaningful Filter Summaries**: Used in collapsed UI:
   ```csharp
   // "5 walls" is better than "5 objects"
   public string Summary => $"{Selection.Count} {GetObjectType()}s";
   ```

7. **Test with DesignViewModels**: Use design-time data for XAML preview:
   ```xaml
   <Design.DataContext>
       <dvm:DesignHomeViewModel />
   </Design.DataContext>
   ```

8. **Log Failures Appropriately**: Use Speckle.Core.Logging.SpeckleLog:
   ```csharp
   SpeckleLog.Logger.Error(ex, "Failed to send stream {streamId}", state.StreamId);
   ```

## Summary

DesktopUI2 provides a comprehensive, reusable UI framework for Speckle connectors through:

- **Clean Separation of Concerns**: MVVM architecture separates UI logic from domain logic
- **Reactive Programming Model**: Automatic propagation of state changes
- **Extensibility**: Pluggable filters, settings, and menu items
- **Cross-Platform Support**: Single codebase runs on Windows, macOS, and Linux
- **Rich Integration Points**: Abstract base class defines all connector responsibilities
- **Modern UI**: Material Design with smooth animations and responsive layout

Connectors integrate by implementing `ConnectorBindings` and registering with the dependency injection container. The framework handles all UI concerns, leaving connector implementations to focus on host-app-specific logic.
