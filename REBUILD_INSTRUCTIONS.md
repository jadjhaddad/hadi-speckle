# ETABS22 Rebuild Instructions

## Problem
You're still getting the serialization error because the old DLLs are still being used. The project reference changes need to be built to take effect.

## Build Permission Issues - FIXED

If you were getting permission errors during build (related to `dotnet tool restore` or `husky install`), this has been fixed:

**Changes made:**
- Added `ContinueOnError="true"` to Husky install commands in `Core.csproj`
- Added `/p:SkipHusky=true` parameter to skip Husky installation entirely
- Replaced all `Exec` tasks with `Copy` tasks in `Directory.Build.targets`
- Created automated build script (`build-etabs22.ps1`) that handles everything

**You can now build without permission issues using either:**
1. The automated build script (recommended)
2. Manual builds with `/p:SkipHusky=true` flag

## Complete Rebuild Steps

### Option A: Use the Build Script (RECOMMENDED)

The easiest way is to use the provided PowerShell script:

```powershell
# In PowerShell, navigate to your solution root
cd C:\Users\jjhaddad\source\repos\speckle-sharp-main

# Run the build script
.\build-etabs22.ps1
```

This script will:
- Clean the solution
- Remove all bin/obj folders
- Clean the ETABS Plug-Ins folder
- Build all projects in the correct order with the `/p:SkipHusky=true` flag
- Verify that all critical DLLs were copied

### Option B: Manual Build (If Permission Issues)

If you're experiencing permission issues with `dotnet tool restore` or `husky install`, use the SkipHusky flag:

#### 1. Clean Everything

First, clean all old binaries and the ETABS Plug-Ins folder:

```powershell
# In PowerShell, navigate to your solution root
cd C:\Users\jjhaddad\source\repos\speckle-sharp-main

# Clean the solution
dotnet clean All.sln

# Delete all bin and obj folders (force clean)
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force

# Clean the ETABS Plug-Ins folder (IMPORTANT!)
Remove-Item "C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\*.dll" -Force
```

#### 2. Build in Correct Order with SkipHusky Flag

Build the projects in dependency order with the `/p:SkipHusky=true` flag to avoid permission issues:

```powershell
# 1. Build Core
dotnet build Core\Core\Core.csproj -c Debug /p:SkipHusky=true

# 2. Build Objects
dotnet build Objects\Objects\Objects.csproj -c Debug /p:SkipHusky=true

# 3. Build DesktopUI2
dotnet build DesktopUI2\DesktopUI2\DesktopUI2.csproj -c Debug /p:SkipHusky=true

# 4. Build Converter ETABS22
dotnet build Objects\Converters\ConverterCSI\ConverterETABS22\ConverterETABS22.csproj -c Debug /p:SkipHusky=true

# 5. Build Connector ETABS22
dotnet build ConnectorCSI\ConnectorETABS22\ConnectorETABS22.csproj -c Debug /p:SkipHusky=true
```

### 3. Verify DLLs Were Copied

Check that the following files exist in the ETABS Plug-Ins folder:

```powershell
Get-ChildItem "C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\" -Filter *.dll | Select-Object Name, Length, LastWriteTime
```

You should see:
- `SpeckleConnectorCSI.dll` (the connector)
- `Objects.dll` (from the connector build)
- `Speckle.Core.dll` (from the connector build)
- `Objects.Converter.ETABS22.dll` (the converter)
- Other dependencies (DesktopUI2.dll, Avalonia DLLs, etc.)

### 4. Check Assembly Versions

The diagnostic logging I added will show you which assemblies are being loaded. After rebuilding, look for these log entries:

```
📍 Connector assembly location: ...
📍 Objects assembly location: ...
📍 Objects assembly version: ...
📍 Core assembly location: ...
📍 Core assembly version: ...
📍 Loading converter from: ...
📍 Converter assembly version: ...
📍 Converter references: Objects v...
📍 Converter references: Core v...
```

**All Objects and Core assemblies should be from the same location** (the Plug-Ins folder) and have the **same version numbers**.

### 5. Test in ETABS 22

1. Close ETABS 22 if it's running
2. Launch ETABS 22
3. Open your model
4. Try to send objects to Speckle
5. Check the logs for the diagnostic information

## Expected Log Output (After Successful Rebuild)

You should see something like:

```
📍 Connector assembly location: C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\SpeckleConnectorCSI.dll
📍 Objects assembly location: C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\Objects.dll
📍 Objects assembly version: 2.x.x.x
📍 Core assembly location: C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\Speckle.Core.dll
📍 Core assembly version: 2.x.x.x
📍 Loading converter from: C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\Objects.Converter.ETABS22.dll
📍 Converter references: Objects v2.x.x.x
📍 Converter references: Core v2.x.x.x
🔍 Model is Base? True
🔍 Is Model assignable to Base? True
```

**Key indicators of success:**
- ✅ All DLLs are from the same folder
- ✅ Objects and Core versions match between connector and converter
- ✅ "Model is Base?" shows **True**
- ✅ "Is Model assignable to Base?" shows **True**

## If Issues Persist

### Check for Leftover DLLs

Sometimes Windows caches DLLs. Check:

```powershell
# Check AppData Roaming (shouldn't be used anymore)
Get-ChildItem "$env:APPDATA\Speckle\Kits\Objects" -Filter *.dll

# Check if there are multiple versions
Get-ChildItem "C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\" -Recurse -Filter Objects.dll
Get-ChildItem "C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\" -Recurse -Filter Speckle.Core.dll
```

### Use Process Explorer

If you have Process Explorer, you can:
1. Launch ETABS 22
2. Find ETABS.exe in Process Explorer
3. View -> Lower Pane View -> DLLs
4. Search for "Objects.dll" and "Speckle.Core.dll"
5. Check which versions are loaded and from where

### Manual DLL Copy (Last Resort)

If the post-build events aren't working:

```powershell
# Copy connector and its dependencies
Copy-Item "ConnectorCSI\ConnectorETABS22\bin\Debug\net48\*.dll" "C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\" -Force

# Copy converter
Copy-Item "Objects\Converters\ConverterCSI\ConverterETABS22\bin\Debug\netstandard2.0\Objects.Converter.ETABS22.dll" "C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins\" -Force
```

## Technical Explanation

The issue is caused by **type identity** in .NET. When two assemblies define the same type (like `Objects.Structural.Analysis.Model`), .NET treats them as **different types** if they come from different assembly files or locations.

Before the fix:
- Connector loaded `Objects.dll` from `AppData\Roaming\Speckle\Kits\Objects` (old version)
- Converter was built against local `Objects.csproj` (new version)
- When converter returned a `Model` object, the serializer didn't recognize it as a `Base` object because the `Base` class in the old Objects.dll was different from the new one

After the fix:
- Both connector and converter reference the local `Objects.csproj`
- After rebuilding, they both use the same `Objects.dll` from the Plug-Ins folder
- Type identity is preserved, and serialization works correctly
