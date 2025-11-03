# ETABS22 Serialization Error Fix

## Problem

When attempting to send objects from ETABS22, the following error occurred:

```
Speckle.Core.Serialisation.SpeckleSerializeException: Failed to extract (pre-serialize) properties from the Speckle.Core.Models.Base
 ---> System.ArgumentException: Unsupported value in serialization: Objects.Structural.Analysis.Model
```

Additionally, there was a warning about assembly version mismatch:
```
Could not merge converter report - assembly version mismatch
Microsoft.CSharp.RuntimeBinder.RuntimeBinderException: The best overloaded method match for 'Speckle.Core.Models.ProgressReport.Merge(Speckle.Core.Models.ProgressReport)' has some invalid arguments
```

## Root Cause

The issue was caused by **assembly version mismatch** and **type identity problems**:

1. **ConnectorETABS22.csproj** was referencing:
   - `Objects.dll` from the Roaming AppData folder (pre-installed version)
   - `Core.csproj` as a local project reference

2. **ConverterETABS22.csproj** was referencing:
   - `Objects.csproj` as a local project reference
   - `Core.csproj` as a local project reference with `<Private>False</Private>`

3. When the connector dynamically loaded the converter:
   - The connector's AppDomain had the old Objects.dll from Roaming loaded
   - The converter was built against the new local Objects.csproj
   - The `Model` class from the new build was not recognized as a `Base` class from the old build
   - This caused the serializer to reject it as an "unsupported value"

## Solution

### 1. Fixed ConnectorETABS22.csproj

**Removed** the reference to the pre-built Objects.dll:
```xml
<!-- REMOVED -->
<Reference Include="Objects">
  <HintPath>..\..\..\..\..\AppData\Roaming\Speckle\Kits\Objects\Objects.dll</HintPath>
</Reference>
```

**Added** project reference to Objects:
```xml
<ProjectReference Include="..\..\Objects\Objects\Objects.csproj" />
```

### 2. Fixed ConverterETABS22.csproj

**Changed** Core reference from `<Private>False</Private>` to default (true):
```xml
<!-- BEFORE -->
<ProjectReference Include="..\..\..\..\Core\Core\Core.csproj">
  <Private>False</Private>
</ProjectReference>

<!-- AFTER -->
<ProjectReference Include="..\..\..\..\Core\Core\Core.csproj" />
```

### 3. Added Converter Copy Target

**Added** post-build event to copy converter DLL to ETABS Plug-Ins folder:
```xml
<Target Name="CopyToETABSPlugins" AfterTargets="PostBuildEvent">
  <Message Text="Copying converter to ETABS 22 Plug-Ins folder" />
  <Copy Condition="$([MSBuild]::IsOsPlatform('Windows'))" SourceFiles="$(TargetPath)" DestinationFolder="C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\" />
</Target>
```

## Why This Works

1. **Unified Assembly Versions**: Both the connector and converter now reference the same local builds of Core and Objects, ensuring type identity across assemblies.

2. **Proper Dependency Copy**: Removing `<Private>False</Private>` ensures Core.dll is copied to the converter's output directory and included in deployments.

3. **Co-location**: The converter DLL is now copied to the same folder as the connector (ETABS Plug-Ins), making dynamic loading more reliable.

## Testing Instructions

1. **Clean the solution**:
   ```
   dotnet clean
   ```

2. **Rebuild both projects**:
   ```
   dotnet build ConnectorCSI/ConnectorETABS22/ConnectorETABS22.csproj
   dotnet build Objects/Converters/ConverterCSI/ConverterETABS22/ConverterETABS22.csproj
   ```

3. **Verify files are copied** to:
   - `C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\`
   - Should include: `SpeckleConnectorCSI.dll`, `Objects.dll`, `Speckle.Core.dll`, `Objects.Converter.ETABS22.dll`

4. **Launch ETABS 22** and test sending objects

## Files Changed

- `ConnectorCSI/ConnectorETABS22/ConnectorETABS22.csproj`
- `Objects/Converters/ConverterCSI/ConverterETABS22/ConverterETABS22.csproj`

## Additional Notes

- This issue is common with plugins that use dynamic assembly loading
- Always ensure all components reference the same version of shared assemblies
- The `<Private>False</Private>` setting should be avoided unless you're certain the assembly will be available in the GAC or another shared location
