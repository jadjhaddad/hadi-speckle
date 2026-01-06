# Speckle Core SDK - Technical Reference

## Overview

**Speckle.Core** is the foundational .NET SDK (targeting .NET Standard 2.0) that provides the complete infrastructure for building Speckle connectors and applications. It contains approximately 19,900 lines of C# code across 142 files organized into distinct functional domains: serialization, data models, API clients, transport layers, kit systems, and credentials management.

The Core SDK is designed to be:
- **Extensible**: Through kits and converters that adapt Speckle objects to/from native applications
- **Transport-agnostic**: Objects can move through multiple storage backends (memory, disk, server, databases)
- **Strongly-typed with dynamic flexibility**: Inherits from a dynamic base class supporting both typed properties and dynamic attributes
- **Production-ready**: Used in 30+ official and community connectors across diverse platforms

---

## Table of Contents

1. [Project Structure](#project-structure)
2. [Base Object System](#base-object-system)
3. [Serialization System](#serialization-system)
4. [Transport Layer](#transport-layer)
5. [Kit System & Converters](#kit-system--converters)
6. [API Client](#api-client)
7. [Credentials Management](#credentials-management)
8. [Operations & Common Patterns](#operations--common-patterns)
9. [Design Patterns](#design-patterns)
10. [Advanced Topics](#advanced-topics)

---

## Project Structure

### Core Assembly Layout

```
Core/
├── Core/                          # Main SDK assembly (Speckle.Core.csproj)
│   ├── Api/                       # GraphQL client and server operations
│   │   ├── GraphQL/              # GraphQL client and models
│   │   ├── Operations/           # Send/Receive/Serialize operations
│   │   └── Helpers.cs            # HTTP and API utilities
│   ├── Credentials/              # Account and authentication
│   ├── Kits/                     # Kit discovery and converter interfaces
│   ├── Models/                   # Base classes and object definitions
│   │   ├── Base.cs              # Core Speckle object base class
│   │   ├── DynamicBase.cs       # Dynamic property support
│   │   ├── ApplicationObject.cs  # Host app object tracking
│   │   └── Extras.cs            # Utility models (Abstract, DataChunk)
│   ├── Serialisation/           # V2 serializer/deserializer
│   ├── Transports/              # ITransport implementations
│   ├── Logging/                 # Serilog-based logging
│   └── Helpers/                 # Utilities (crypto, HTTP, paths)
├── Transports/                    # Optional transport packages
│   ├── DiskTransport/
│   └── MongoDBTransport/
└── Tests/
    ├── Speckle.Core.Tests.Unit/
    └── Speckle.Core.Tests.Integration/
```

### Key Dependencies

```xml
<PackageReference Include="GraphQL.Client" Version="6.0.0" />
<PackageReference Include="Speckle.Newtonsoft.Json" Version="13.0.2" />
<PackageReference Include="Serilog" Version="2.12.0" />
<PackageReference Include="Polly" Version="7.2.3" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="7.0.5" />
<PackageReference Include="NetTopologySuite" Version="2.5.0" />
```

---

## Base Object System

### Overview

The **Base class** is the universal container for all Speckle data. It extends `DynamicBase`, providing:
- Unified serialization/deserialization
- Content-addressed identification (hash-based IDs)
- Dynamic property assignment
- Type information tracking

### DynamicBase: The Foundation

`DynamicBase` implements `IDynamicMetaObjectProvider`, allowing property access via both conventional and dynamic syntax:

```csharp
// Property bag for dynamic properties
private readonly Dictionary<string, object> _properties = new();

// Bracket notation (typed or dynamic)
myObject["dynamicProperty"] = 42;
var value = myObject["dynamicProperty"];

// Dot notation (dynamic objects only)
((dynamic)myObject).anotherProperty = "hello";
var another = ((dynamic)myObject).anotherProperty;
```

**Key Methods:**
- `this[string key]` - Get/set properties by name
- `TryGetMember(GetMemberBinder, out object)` - Dynamic property access
- `TrySetMember(SetMemberBinder, object)` - Dynamic property assignment

**Property Name Rules:**
```
"__propertyName"    → Ignored in serialization and hashing (private)
"@propertyName"     → Detached property (stored separately)
"@@propertyName"    → Invalid (error)
"property.with/bad" → Invalid (dots and slashes not allowed)
```

### Base Class: The Speckle Object

```csharp
public class Base : DynamicBase
{
    // Content-based hash identifier (null unless retrieved from storage)
    public virtual string id { get; set; }
    
    // Secondary application-specific identifier
    public string applicationId { get; set; }
    
    // Total number of detached children (populated from storage)
    public virtual long totalChildrenCount { get; set; }
    
    // Automatic type path (e.g., "Objects.Geometry.Point" or "Base")
    public virtual string speckle_type { get; }
    
    // Get hash ID - expensive operation (full serialization)
    public string GetId(bool decompose = false);
    
    // Count detached children without full serialization
    public long GetTotalChildrenCount();
    
    // Shallow copy (pointers to original property values)
    public Base ShallowCopy();
}
```

### Core-Provided Models

**Blob** - File reference with content hashing:
```csharp
public class Blob : Base
{
    public string filePath { get; set; }
    public string originalPath { get; set; }
    
    // Override: id is computed from file hash
    public override string id { get => GetFileHash(); set => base.id = value; }
    public string GetFileHash();
    public string GetLocalDestinationPath(string blobStorageFolder);
}
```

**Collection** - Container for multiple objects:
```csharp
public class Collection : Base
{
    public string name { get; set; }
    public List<Base> elements { get; set; }
}
```

**Abstract** - Wrapper for non-Speckle types:
```csharp
public class Abstract : Base
{
    public object @base { get; set; }
    public string assemblyQualifiedName { get; set; }
}
```

**DataChunk** - Large collection chunking:
```csharp
public class DataChunk : Base
{
    public List<object> data { get; set; } = new();
}
```

### ApplicationObject: Tracking Conversions

Bridges between Speckle and native application objects:

```csharp
public class ApplicationObject
{
    public enum State { Unknown, Created, Skipped, Updated, Failed, Removed }
    
    // Native app ID
    public string applicationId { get; set; }
    
    // Container in native app
    public string Container { get; set; }
    
    // Original Speckle ID (on receive) or native ID (on send)
    public string OriginalId { get; set; }
    
    // Native object IDs created from this Speckle object
    public List<string> CreatedIds { get; set; }
    
    // Conversion status
    public State Status { get; set; }
    
    // Conversion log/messages
    public List<string> Log { get; set; }
    
    // Fallback objects (typically displayValue)
    public List<ApplicationObject> Fallback { get; set; }
}
```

---

## Serialization System

### Architecture

Speckle uses a two-stage serialization process:

1. **Pre-serialization**: Convert complex .NET objects to primitive types
2. **JSON encoding**: Convert primitives to JSON string

### BaseObjectSerializerV2

The production serializer handling complex object graphs:

```csharp
public class BaseObjectSerializerV2
{
    // Transports where objects will be persisted
    public IReadOnlyCollection<ITransport> WriteTransports { get; }
    
    // Progress reporting
    private Action<string, int>? _onProgressAction;
    
    // Cancellation support
    public CancellationToken CancellationToken { get; set; }
    
    // Measure operation time
    public TimeSpan Elapsed { get; }
    
    // Main method: serialize Base object to JSON
    public string Serialize(Base baseObj);
    
    // Pre-serialization: convert to primitive types
    public object? PreserializeObject(
        object? obj,
        bool computeClosures = false,
        PropertyAttributeInfo inheritedDetachInfo = default
    );
}
```

**Serialization Flow:**

```
Input: Base object
  ↓
PreserializeBase() - Extract properties, handle detachment
  ↓
PreserializeObject() - Convert each property
  │   ├─ Primitives → passthrough
  │   ├─ Base objects → recursively preserialize
  │   ├─ Collections → iterate and convert each item
  │   ├─ Dictionaries → convert values
  │   └─ Enums → convert to int
  ↓
Dict2Json() - Convert dictionary to JSON string
  ↓
StoreObject() - Write to transports with ID as key
  ↓
Output: JSON string (with ID inside)
```

**Detachment and Closures:**

Large or frequently-referenced objects are stored separately:

```csharp
// Mark property for detachment
[DetachProperty(true)]
public List<Base> LargeElements { get; set; }

// Chunking long lists
[Chunkable(maxObjCountPerChunk: 10000)]
[DetachProperty(true)]
public List<Point> ManyPoints { get; set; }
```

The serializer maintains a closure map:
```json
{
  "id": "abc123...",
  "name": "root",
  "__closure": {
    "child_id_1": 5,
    "child_id_2": 3
  }
}
```

### BaseObjectDeserializerV2

Reconstructs objects from JSON with multi-threaded closure resolution:

```csharp
public sealed class BaseObjectDeserializerV2
{
    // Transport for retrieving serialized closures
    public ITransport ReadTransport { get; set; }
    
    // Worker thread pool configuration
    public int WorkerThreadCount { get; set; } = Math.Min(Environment.ProcessorCount, 6);
    
    // Progress callback
    public Action<string, int>? OnProgressAction { get; set; }
    
    // Main method: deserialize from JSON string
    public Base Deserialize(string rootObjectJson);
    
    // Elapsed time measurement
    public TimeSpan Elapsed { get; private set; }
}
```

**Deserialization Flow:**

```
Input: JSON string (root object)
  ↓
GetClosures() - Extract child IDs from __closure
  ↓
Sort by depth (deepest first)
  ↓
Parallel fetching from ReadTransport
  ↓
Deserialize each object → store in cache
  ↓
DeserializeTransportObject() - Reconstruct root with references
  │   ├─ Resolve detached properties
  │   ├─ Reconstruct collections
  │   └─ Apply type information
  ↓
Output: Fully reconstructed Base object
```

### Example: Custom Serialization

```csharp
using Speckle.Core.Api;
using Speckle.Core.Models;
using Speckle.Core.Serialisation;
using Speckle.Core.Transports;

// Serialize to JSON
var myObject = new Base { applicationId = "obj-1" };
((dynamic)myObject).customField = "value";

var serialized = Operations.Serialize(myObject);
// Result: {"id":"hash...","applicationId":"obj-1","customField":"value",...}

// Deserialize from JSON
var deserialized = Operations.Deserialize(serialized);
Console.WriteLine(deserialized.GetId()); // Same hash as original

// With transports
var memoryTransport = new MemoryTransport();
var serializer = new BaseObjectSerializerV2(new[] { memoryTransport });
string json = serializer.Serialize(myObject);
// Object stored in memoryTransport with ID as key
```

---

## Transport Layer

### ITransport Interface

Defines the contract for object persistence:

```csharp
public interface ITransport
{
    // Metadata
    string TransportName { get; set; }
    Dictionary<string, object> TransportContext { get; }
    
    // Lifecycle
    void BeginWrite();
    void EndWrite();
    Task WriteComplete();
    
    // Persistence
    void SaveObject(string id, string serializedObject);
    void SaveObject(string id, ITransport sourceTransport);
    
    // Retrieval
    string? GetObject(string id);
    
    // Copying
    Task<string> CopyObjectAndChildren(
        string id,
        ITransport targetTransport,
        Action<int>? onTotalChildrenCountKnown = null
    );
    
    // Existence check
    Task<Dictionary<string, bool>> HasObjects(IReadOnlyList<string> objectIds);
    
    // Progress & cancellation
    Action<string, int>? OnProgressAction { get; set; }
    CancellationToken CancellationToken { get; set; }
    
    // Timing
    TimeSpan Elapsed { get; }
    int SavedObjectCount { get; }
}

public interface IBlobCapableTransport
{
    string BlobStorageFolder { get; }
    void SaveBlob(Blob obj);
}
```

### MemoryTransport

In-memory storage using a dictionary:

```csharp
public sealed class MemoryTransport : ITransport, ICloneable
{
    public IDictionary<string, string> Objects { get; }
    
    public MemoryTransport()
        : this(new Dictionary<string, string>()) { }
    
    public MemoryTransport(IDictionary<string, string> objects)
    {
        Objects = objects;
    }
    
    // Fast, synchronous operations
    public void SaveObject(string id, string serializedObject)
    {
        Objects[id] = serializedObject;
        SavedObjectCount++;
    }
    
    public string? GetObject(string id) =>
        Objects.TryGetValue(id, out var obj) ? obj : null;
}
```

**Use Cases:**
- Testing and development
- Short-lived object caching
- Fallback when disk is unavailable

### SQLiteTransport

Persistent local storage in SQLite database:

```csharp
public sealed class SQLiteTransport : ITransport, IBlobCapableTransport, ICloneable
{
    // Connection to local database
    // Path pattern: {basePath}/{applicationName}/{scope}.db
    // Default: %APPDATA%\Speckle\Speckle\Data.db
    
    public SQLiteTransport(
        string? basePath = null,
        string? applicationName = null,
        string? scope = null
    )
    
    public string BlobStorageFolder { get; }
    public void SaveBlob(Blob obj);
}
```

**Features:**
- Thread-safe write queue with batching
- Automatic database schema creation
- Blob storage integration
- Configurable transaction size (default: 1000 objects)

**Example:**
```csharp
// Default local cache
var cache = new SQLiteTransport();
// Creates: %APPDATA%\Speckle\Speckle\Data.db

// Application-specific cache
var appCache = new SQLiteTransport(
    applicationName: "MyConnector",
    scope: "Cache"
);
// Creates: %APPDATA%\Speckle\MyConnector\Cache.db
```

### ServerTransport

Communication with Speckle Server:

```csharp
public sealed class ServerTransport : ITransport, IBlobCapableTransport
{
    // Initialize with account and stream
    public ServerTransport(
        Account account,
        string streamId,
        int timeoutSeconds = 60,
        string? blobStorageFolder = null
    )
    
    public Account Account { get; }
    public string StreamId { get; }
    public string BaseUri { get; }
    public int TimeoutSeconds { get; set; }
    public string BlobStorageFolder { get; set; }
    
    // Background sending thread for efficient batching
    internal ParallelServerApi Api { get; }
}
```

**Features:**
- Connection pooling and retry logic (Polly)
- Parallel uploads with configurable thread count
- Blob storage with hash-based deduplication
- Gzip payload compression
- Progress reporting via callbacks

**Example:**
```csharp
using var serverTransport = new ServerTransport(account, streamId: "abc123");

// Send object to server
var objId = await Operations.Send(
    myObject,
    serverTransport,
    useDefaultCache: true  // Also write to local cache
);

// Receive from server (with local cache fallback)
using var localCache = new SQLiteTransport();
var receivedObject = await Operations.Receive(
    objId,
    remoteTransport: serverTransport,
    localTransport: localCache
);
```

### Transport Patterns

**Pattern 1: Multi-Transport Pipeline**
```csharp
var transports = new ITransport[]
{
    new MemoryTransport(),              // Fast, temporary
    new SQLiteTransport(),              // Persistent
    new ServerTransport(account, streamId) // Remote
};

await Operations.Send(myObject, transports);
// Object written to all three
```

**Pattern 2: Local-First Receive**
```csharp
// Try local first, fall back to server
var received = await Operations.Receive(
    objectId,
    remoteTransport: serverTransport,  // Fallback
    localTransport: localCache          // Primary
);
```

**Pattern 3: Blob Handling**
```csharp
var blob = new Blob { filePath = "/path/to/file.txt" };
serverTransport.SaveBlob(blob);
// File is copied to serverTransport.BlobStorageFolder
// Deduplication by content hash (blob.GetFileHash())
```

---

## Kit System & Converters

### Overview

The **Kit System** enables adapting Speckle objects to/from native CAD/BIM application formats. Kits are discoverable plugins containing:
- Object type definitions (schemas)
- Converters (bidirectional transformations)

### ISpeckleKit Interface

```csharp
public interface ISpeckleKit
{
    // Available object types from this kit
    IEnumerable<Type> Types { get; }
    
    // Available converters (application names)
    IEnumerable<string> Converters { get; }
    
    // Metadata
    string Name { get; }
    string Description { get; }
    string Author { get; }
    string WebsiteOrEmail { get; }
    
    // Load a converter for specific application
    ISpeckleConverter LoadConverter(string app);
}
```

### ISpeckleConverter Interface

Performs bidirectional transformations between Speckle and native formats:

```csharp
public interface ISpeckleConverter
{
    // Metadata
    string Name { get; }
    string Description { get; }
    string Author { get; }
    string WebsiteOrEmail { get; }
    
    // Configuration
    ReceiveMode ReceiveMode { get; set; }
    ProgressReport Report { get; }
    
    // Speckle → Native conversions
    Base ConvertToSpeckle(object value);
    List<Base> ConvertToSpeckle(List<object> values);
    bool CanConvertToSpeckle(object value);
    
    // Native → Speckle conversions
    object ConvertToNative(Base value);
    List<object> ConvertToNative(List<Base> values);
    bool CanConvertToNative(Base value);
    
    // Fallback conversion (using displayValue)
    object ConvertToNativeDisplayable(Base value);
    bool CanConvertToNativeDisplayable(Base value);
    
    // Application support
    IEnumerable<string> GetServicedApplications();
    
    // Context setup
    void SetContextDocument(object doc);
    void SetContextObjects(List<ApplicationObject> objects);
    void SetPreviousContextObjects(List<ApplicationObject> objects);
    void SetConverterSettings(object settings);
}

public enum ReceiveMode
{
    Update,  // Update existing, delete removed, create new
    Create,  // Always create new
    Ignore   // Don't update existing, create new
}
```

### KitManager: Discovery & Loading

```csharp
public static class KitManager
{
    // Default kit folder location
    // Local: %APPDATA%\Speckle\Kits
    // Admin: %PROGRAMDATA%\Speckle\Kits
    public static string KitsFolder { get; set; }
    
    // All discovered kits
    public static IEnumerable<ISpeckleKit> Kits { get; }
    
    // All available types across kits
    public static IEnumerable<Type> Types { get; }
    
    // Check kit existence
    public static bool HasKit(string assemblyFullName);
    public static ISpeckleKit GetKit(string assemblyFullName);
    
    // Get default "Objects" kit
    public static ISpeckleKit GetDefaultKit();
    
    // Find kits with converters for an application
    public static IEnumerable<ISpeckleKit> GetKitsWithConvertersForApp(string app);
    
    // Manual initialization
    public static void Initialize(string kitFolderLocation);
    
    // Kit assembly discovery
    public static List<Assembly> GetReferencedAssemblies();
}
```

**Example: Loading a Converter**

```csharp
using Speckle.Core.Kits;
using Speckle.Core.Models;

// Get kit with Revit converter
var objectsKit = KitManager.GetDefaultKit();
var revitConverter = objectsKit.LoadConverter(HostApplications.Revit.Slug);

// Convert native Revit element to Speckle
var nativeWall = revitDocument.GetElement(wallId);
var speckleWall = revitConverter.ConvertToSpeckle(nativeWall);

// Set up context for converter
revitConverter.SetContextDocument(revitDocument);

// Convert back to Revit
var nativeResult = revitConverter.ConvertToNative(speckleWall);
```

### Supported Applications

The `HostApplications` class provides standardized application identifiers:

```csharp
HostApplications.Rhino           // "rhino"
HostApplications.Grasshopper     // "grasshopper"
HostApplications.Revit           // "revit"
HostApplications.Dynamo          // "dynamo"
HostApplications.AutoCAD         // "autocad"
HostApplications.Civil           // "civil3d"
HostApplications.MicroStation    // "microstation"
HostApplications.ETABS           // "etabs"
HostApplications.SAP2000         // "sap2000"
HostApplications.CSiBridge       // "csibridge"
HostApplications.TeklaStructures // "teklastructures"
// ... and 20+ more
```

### CommitObjectBuilder: Structuring Data

Base class for organizing converted objects into hierarchies:

```csharp
public abstract class CommitObjectBuilder<TNativeObjectData>
{
    // Objects converted from native format
    protected IDictionary<string, Base> Converted { get; }
    
    // Define how objects nest in the commit tree
    protected void SetRelationship(
        Base conversionResult,
        IList<NestingInstructions> nestingInstructionsList
    );
    
    // Add object to conversion result
    public abstract void IncludeObject(
        Base conversionResult,
        TNativeObjectData nativeElement
    );
    
    // Build final commit structure
    public virtual void BuildCommitObject(Base rootCommitObject);
}
```

---

## API Client

### Overview

The **Client** class provides GraphQL access to Speckle Server, with resource-specific operations organized into properties.

### Client Architecture

```csharp
public sealed partial class Client : ISpeckleGraphQLClient, IDisposable
{
    // Resource managers
    public ProjectResource Project { get; }
    public ModelResource Model { get; }
    public VersionResource Version { get; }
    public ActiveUserResource ActiveUser { get; }
    public OtherUserResource OtherUser { get; }
    public ProjectInviteResource ProjectInvite { get; }
    public CommentResource Comment { get; }
    public SubscriptionResource Subscription { get; }
    
    // Connection info
    public Account Account { get; }
    public string ServerUrl { get; }
    public string ApiToken { get; }
    public System.Version? ServerVersion { get; }
    
    // GraphQL client
    public GraphQLHttpClient GQLClient { get; }
    
    public Client(Account account)
    {
        // Initialize all resources
        // Set up HTTP client with auth
        // Configure GraphQL client
    }
}
```

### Example: Using the Client

```csharp
using Speckle.Core.Api;
using Speckle.Core.Credentials;

// Get authenticated account
var account = AccountManager.GetDefaultAccount();

// Create client
using var client = new Client(account);

// Query projects
var projects = await client.Project.List(10);
foreach (var project in projects.Items)
{
    Console.WriteLine($"{project.Name}: {project.Id}");
}

// Get project with models
var project = await client.Project.Get(projectId);
var models = await client.Model.List(projectId, limit: 20);

// Get version history
var versions = await client.Version.List(projectId, modelId, limit: 10);
var latestVersion = versions.Items.First();

// Create a new version
var newVersion = await client.Version.Create(
    projectId,
    modelId,
    objectId: "hash...",
    message: "Updated data"
);
```

### GraphQL Resources

Resources are organized by entity type with CRUD operations:

**ProjectResource**
- `List(limit, cursor)` - Paginated project list
- `Get(id)` - Get single project details
- `Create(input)` - Create new project
- `Update(id, input)` - Update project
- `Delete(id)` - Delete project

**ModelResource**
- `List(projectId, limit, cursor)` - List models in project
- `Get(projectId, modelId)` - Get model with versions
- `Create(projectId, input)` - Create model
- `Update(projectId, modelId, input)` - Update model
- `Delete(projectId, modelId)` - Delete model

**VersionResource**
- `List(projectId, modelId, limit, cursor)` - Version history
- `Get(projectId, modelId, versionId)` - Get specific version
- `Create(projectId, modelId, input)` - Create version
- `Delete(projectId, modelId, versionId)` - Delete version

### Subscriptions

Real-time updates via GraphQL subscriptions:

```csharp
var subscription = client.Subscription.OnProjectUpdated(
    projectId,
    (update) =>
    {
        Console.WriteLine($"Project updated: {update.Type}");
    }
);

// Later: unsubscribe
subscription.Dispose();
```

---

## Credentials Management

### Account Structure

```csharp
public class Account : IEquatable<Account>
{
    // Unique account identifier (computed from email + server URL)
    public string id { get; }
    
    // Authentication tokens
    public string token { get; set; }
    public string refreshToken { get; set; }
    
    // Server information
    public ServerInfo serverInfo { get; set; }
    
    // User information
    public UserInfo userInfo { get; set; }
    
    // Default account flag
    public bool isDefault { get; set; }
    
    // Connectivity status
    public bool isOnline { get; set; }
    
    // Validation against server
    public async Task<UserInfo> Validate();
    
    // Helper methods
    public string GetHashedEmail();
    public string GetHashedServer();
    public Uri GetLocalIdentifier();
}
```

### AccountManager: Credential Storage

```csharp
public static class AccountManager
{
    // Default server
    public const string DEFAULT_SERVER_URL = "https://app.speckle.systems";
    
    // Get server information
    public static async Task<ServerInfo> GetServerInfo(
        Uri server,
        CancellationToken cancellationToken = default
    );
    
    // Get user information given token
    public static async Task<UserInfo> GetUserInfo(
        string token,
        Uri server,
        CancellationToken cancellationToken = default
    );
    
    // Account storage and retrieval
    public static async Task<Account> GetAccount(string accountId);
    public static async Task<List<Account>> GetAccounts();
    public static async Task<Account?> GetDefaultAccount();
    public static async Task SaveAccount(Account account);
    public static async Task RemoveAccount(string accountId);
    
    // OAuth flow
    public static async Task<Account> GetAccountFromAccessCode(
        Uri serverUrl,
        string accessCode,
        string challenge
    );
}
```

**Example: Authentication Flow**

```csharp
using Speckle.Core.Credentials;

// Get server info
var server = new Uri("https://app.speckle.systems");
var serverInfo = await AccountManager.GetServerInfo(server);

// In real app: user logs in via browser, gets access code
string accessCode = "user_provided_code";

// Exchange for account
var account = await AccountManager.GetAccountFromAccessCode(
    server,
    accessCode,
    "challenge"
);

// Save for future use
await AccountManager.SaveAccount(account);

// Retrieve later
var savedAccount = await AccountManager.GetAccount(account.id);
```

---

## Operations & Common Patterns

### Operations.Send

Push objects to one or more transports:

```csharp
public static async Task<string> Send(
    Base value,
    ITransport transport,
    bool useDefaultCache = false,
    Action<ConcurrentDictionary<string, int>>? onProgressAction = null,
    CancellationToken cancellationToken = default
)
```

**Example:**

```csharp
using Speckle.Core.Api;
using Speckle.Core.Transports;

var myObject = new Base { applicationId = "wall-1" };
((dynamic)myObject).height = 3.5;

// Send to server (with local cache)
using var server = new ServerTransport(account, streamId);
var objectId = await Operations.Send(
    myObject,
    server,
    useDefaultCache: true,
    onProgressAction: (progress) =>
    {
        foreach (var kvp in progress)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value} objects");
        }
    }
);

Console.WriteLine($"Sent with ID: {objectId}");
```

### Operations.Receive

Pull objects from transports with fallback:

```csharp
public static async Task<Base> Receive(
    string objectId,
    ITransport? remoteTransport = null,
    ITransport? localTransport = null,
    Action<ConcurrentDictionary<string, int>>? onProgressAction = null,
    Action<int>? onTotalChildrenCountKnown = null,
    CancellationToken cancellationToken = default
)
```

**Example:**

```csharp
// Receive with two-tier strategy
var received = await Operations.Receive(
    objectId: "hash123...",
    localTransport: new SQLiteTransport(),      // Fast tier
    remoteTransport: serverTransport,           // Slow tier
    onTotalChildrenCountKnown: (count) =>
    {
        Console.WriteLine($"Will receive {count} objects total");
    }
);

Console.WriteLine($"Object type: {received.speckle_type}");
Console.WriteLine($"Children: {received.GetTotalChildrenCount()}");
```

### Operations.Serialize / Deserialize

Direct JSON conversion:

```csharp
public static string Serialize(Base obj);
public static Base Deserialize(string json);

// Example
var json = Operations.Serialize(myObject);
var restored = Operations.Deserialize(json);
Assert.AreEqual(myObject.GetId(), restored.GetId());
```

---

## Design Patterns

### 1. Transport Abstraction

**Pattern**: Strategy pattern for data persistence

```csharp
// Clients work with ITransport interface
async Task SendData(Base obj, ITransport transport)
{
    await Operations.Send(obj, transport);
}

// Can swap implementations without changing client code
await SendData(obj, new MemoryTransport());
await SendData(obj, new SQLiteTransport());
await SendData(obj, new ServerTransport(account, streamId));
```

### 2. Dynamic Property Bags

**Pattern**: DynamicObject for JSON-like flexibility with typed safety

```csharp
// Typed properties
var wall = new Wall { height = 3.0 };

// Dynamic properties (runtime-added)
((dynamic)wall).fireRating = "1-hour";
wall["customData"] = new { foo = "bar" };

// Both serializable and queryable
var json = Operations.Serialize(wall);
var restored = Operations.Deserialize(json);
Console.WriteLine(restored["customData"]);
```

### 3. Progressive Loading

**Pattern**: Content-addressed objects with on-demand closure resolution

```csharp
// Only root object is loaded initially
var root = await Operations.Receive(rootId, localTransport: cache);

// Children loaded on access via closure resolution
// SerializerV2 handles multi-threaded parallel fetching
var children = root.elements; // Transparently loaded
```

### 4. Commit Object Pattern

**Pattern**: Builder for constructing hierarchies from conversions

```csharp
class RevitCommitBuilder : CommitObjectBuilder<Element>
{
    public override void IncludeObject(Base converted, Element element)
    {
        // Track relationship
        SetRelationship(converted, new NestingInstructions
        {
            ContainerProp = "elements",
            ChildProp = null
        });
    }
    
    public override void BuildCommitObject(Base root)
    {
        // Apply relationships to root
        base.BuildCommitObject(root);
    }
}
```

### 5. Typed Properties with Attributes

**Pattern**: Metadata-driven serialization control

```csharp
public class Room : Base
{
    // Standard property
    public string name { get; set; }
    
    // Large collection - store separately
    [DetachProperty(true)]
    [Chunkable(maxObjCountPerChunk: 5000)]
    public List<Point> Points { get; set; }
    
    // Temporary - ignored in serialization
    [JsonIgnore]
    public object __tempCache { get; set; }
}
```

### 6. Error Handling with Resilience

**Pattern**: Polly retry policies for network operations

```csharp
// Built into Client class
var delay = Backoff.DecorrelatedJitterBackoffV2(
    TimeSpan.FromSeconds(1), 
    retryCount: 5
);

var policy = Policy
    .Handle<SpeckleGraphQLInternalErrorException>()
    .WaitAndRetryAsync(delay);

await policy.ExecuteAsync(async () =>
{
    return await client.Project.Get(projectId);
});
```

---

## Advanced Topics

### Custom Object Models

**Creating domain-specific types:**

```csharp
using Speckle.Core.Models;

namespace MyApp.Speckle.Objects
{
    public class Machine : Base
    {
        public string manufacturer { get; set; }
        public string model { get; set; }
        public double weight { get; set; }
        
        [DetachProperty(true)]
        public List<Component> components { get; set; }
    }
    
    public class Component : Base
    {
        public string partNumber { get; set; }
        public double cost { get; set; }
    }
}
```

**Registering with KitManager:**

The kit manager automatically discovers types implementing `ISpeckleKit`. Your assembly must:
1. Implement `ISpeckleKit`
2. Be placed in `KitsFolder`
3. Reference Speckle.Core

```csharp
public class MyKit : ISpeckleKit
{
    public IEnumerable<Type> Types =>
        typeof(MyKit).Assembly
            .GetTypes()
            .Where(t => typeof(Base).IsAssignableFrom(t));
    
    public IEnumerable<string> Converters => new[] { "MyApp" };
    
    public string Name => "MyKit";
    // ... implementation
}
```

### Large-Scale Data Handling

**For datasets with millions of objects:**

1. **Use Chunking**:
```csharp
[Chunkable(maxObjCountPerChunk: 10000)]
[DetachProperty(true)]
public List<Point> points { get; set; }
```

2. **Stream in Multiple Versions**:
```csharp
// Create incremental versions
for (int i = 0; i < totalPages; i++)
{
    var chunk = GetPageOfData(i, pageSize: 100000);
    var chunkObj = new DataChunk { data = chunk };
    
    var versionId = await client.Version.Create(
        projectId,
        modelId,
        objectId: await Operations.Send(chunkObj, server),
        message: $"Chunk {i}"
    );
}
```

3. **Parallel Transport**:
```csharp
var transports = Enumerable
    .Range(0, 4)
    .Select(_ => new ServerTransport(account, streamId))
    .Cast<ITransport>()
    .ToList();

await Operations.Send(largeObject, transports);
```

### Custom Converter Implementation

```csharp
public class MyConverter : ISpeckleConverter
{
    private Document _document;
    
    public string Name => "MyConverter";
    public string Description => "Converts to/from MyApp format";
    public ReceiveMode ReceiveMode { get; set; }
    public ProgressReport Report { get; } = new();
    
    public void SetContextDocument(object doc)
    {
        _document = (Document)doc;
    }
    
    public Base ConvertToSpeckle(object nativeObject)
    {
        if (nativeObject is Element elem)
        {
            return new Base
            {
                applicationId = elem.Id.ToString(),
                ["name"] = elem.Name
            };
        }
        return null;
    }
    
    public object ConvertToNative(Base value)
    {
        var elem = new Element();
        elem.Name = value["name"]?.ToString();
        return elem;
    }
    
    public bool CanConvertToSpeckle(object value) => value is Element;
    public bool CanConvertToNative(Base value) => true;
    
    // ... other required methods
}
```

### Subscription Handling

```csharp
// Subscribe to model updates
var modelSub = client.Subscription.OnCommitCreated(
    projectId,
    modelId,
    (newCommit) =>
    {
        Console.WriteLine($"New commit: {newCommit.Message}");
        
        // Load the new data
        var data = await Operations.Receive(
            newCommit.ReferencedObject.id,
            remoteTransport: new ServerTransport(account, streamId)
        );
    }
);

// Clean up
modelSub.Dispose();
```

### Performance Optimization

**Deserialization with custom thread count:**

```csharp
var deserializer = new BaseObjectDeserializerV2
{
    ReadTransport = localTransport,
    WorkerThreadCount = 8  // Use more threads for I/O bound ops
};

var obj = deserializer.Deserialize(json);
Console.WriteLine($"Elapsed: {deserializer.Elapsed.TotalSeconds}s");
```

**Lazy Loading Pattern:**

```csharp
// Retrieve object ID without full deserialization
var rootJson = serverTransport.GetObject(objectId);
var rootObject = Operations.Deserialize(rootJson);

// Closures not loaded yet - they're fetched on access
var children = rootObject.elements; // Triggers multi-threaded loading
```

---

## Troubleshooting & Common Issues

### Issue: Objects not found in transport

**Symptoms**: `TransportException: "Cannot find object..."`

**Solutions**:
1. Ensure `localTransport` is not null in `Receive()`
2. Verify transport initialization: `transport.BeginWrite()` / `transport.EndWrite()`
3. Check `transport.GetObject(id)` directly to diagnose

### Issue: Custom properties not serializing

**Check**:
```csharp
// Works: bracket notation
obj["custom"] = value;

// Works: dynamic notation
((dynamic)obj).custom = value;

// Doesn't work: Property without getter (reflection can't access)
obj.CustomProperty = value; // If not defined in class
```

### Issue: Circular references

**Speckle automatically ignores** circular references during serialization. Dynamic properties referencing the same object are set to `null`:

```csharp
var obj = new Base();
obj["self"] = obj; // Will deserialize as null
```

### Issue: KitManager returns empty

**Solutions**:
1. Ensure kit assemblies are in `KitsFolder`
2. Kit assemblies must implement `ISpeckleKit`
3. Call `KitManager.Initialize(path)` before accessing kits if custom folder
4. Check kit assembly loads: `KitManager.GetReferencedAssemblies()`

---

## Summary Table

| Component | Purpose | Key Classes | Typical Usage |
|-----------|---------|------------|---------------|
| **Base/DynamicBase** | Object container | `Base`, `DynamicBase` | Foundation for all Speckle objects |
| **Serialization** | JSON conversion | `BaseObjectSerializerV2`, `BaseObjectDeserializerV2` | Persist/load objects |
| **Transports** | Data storage | `ITransport`, `MemoryTransport`, `SQLiteTransport`, `ServerTransport` | Multi-tier persistence |
| **Kits** | Domain adapters | `ISpeckleKit`, `ISpeckleConverter`, `KitManager` | CAD app integration |
| **API Client** | Server communication | `Client`, `ProjectResource`, etc. | Project/model management |
| **Credentials** | Authentication | `Account`, `AccountManager` | User login/storage |
| **Operations** | High-level API | `Operations.Send`, `.Receive`, `.Serialize` | Common workflows |

---

## Resources

- **Official Docs**: https://speckle.guide/dev/
- **GitHub**: https://github.com/specklesystems/speckle-sharp
- **Community Forum**: https://discourse.speckle.works
- **Nuget Package**: `Speckle.Core` on nuget.org

