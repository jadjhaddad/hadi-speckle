# CSIBridge 25 Plugin Crash Investigation

**Status:** IN PROGRESS - Added detailed logging to diagnose crash
**Date:** 2025-11-06
**Issue:** CSIBridge 25 crashes when showing Speckle plugin window, while ETABS works fine

---

## Problem Summary

### The Crash
When the Speckle connector plugin loads in **CSIBridge 25**, it crashes during window initialization:

```
Error Location: Avalonia.Layout.LayoutManager.RaiseEffectiveViewportChanged()

Stack Trace:
  at Avalonia.Layout.LayoutManager.RaiseEffectiveViewportChanged()
  at Avalonia.Layout.LayoutManager.ExecuteLayoutPass()
  at Avalonia.Controls.WindowBase.HandleResized()
  at Avalonia.Win32.WindowImpl.AppWndProc()
```

**The crash happens during `MainWindow.Show()` when Avalonia tries to calculate the initial window layout.**

### Key Facts
- ✅ **ETABS connector works perfectly** with the same shared code
- ✅ **SAP2000 connector works perfectly** with the same shared code
- ❌ **CSIBridge 25 connector crashes** with the same shared code
- All connectors share `ConnectorCSIShared/cPlugin.cs`
- All connectors use Avalonia 0.10.18
- Assembly loading works correctly (no more version mismatch errors)

---

## What We've Fixed So Far

### ✅ Issue #1: Avalonia Assembly Version Mismatch (SOLVED)

**Problem:**
- DesktopUI2 was compiled with Avalonia 0.10.18 references
- Plug-Ins folder contained Avalonia 0.10.21 DLLs
- Material.Avalonia package requires Avalonia 0.10.18

**Error:**
```
FileLoadException: Could not load file or assembly 'Avalonia.Base, Version=0.10.18.0'
```

**Solution Applied:**
- Downgraded DesktopUI2 to consistently use Avalonia 0.10.18
- Modified: `/DesktopUI2/DesktopUI2/DesktopUI2.csproj` lines 264-272
- Changed all Avalonia packages from 0.10.21 → 0.10.18

**File Changed:**
```xml
<ItemGroup>
    <PackageReference Include="Avalonia" Version="0.10.18" />
    <PackageReference Include="Avalonia.Desktop" Version="0.10.18" />
    <PackageReference Include="Avalonia.Diagnostics" Version="0.10.18" />
    <PackageReference Include="Avalonia.ReactiveUI" Version="0.10.18" />
</ItemGroup>
```

---

### ✅ Issue #2: CSIBridge-Specific Code Changes (APPLIED)

**Problem:** Changes to shared code could break ETABS and other working connectors

**Solution:** Used `#if CSIBRIDGE` conditional compilation to make changes CSIBridge-specific

**File:** `ConnectorCSIShared/cPlugin.cs`

**Changes Made:**

1. **Disabled GPU rendering for CSIBridge** (lines 38-44):
```csharp
#if CSIBRIDGE
  // Disable GPU rendering to avoid conflicts with CSIBridge
  .With(new Win32PlatformOptions { AllowEglInitialization = false, EnableMultitouch = false, UseWgl = false })
#else
  // ETABS: Use standard GPU rendering
  .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
#endif
```

2. **Changed message loop for CSIBridge** (lines 134-155):
```csharp
#if CSIBRIDGE
  // Don't use app.Run() - just show the window
  MainWindow.Show();
#else
  // ETABS: Use standard app.Run()
  app.Run(MainWindow);
#endif
```

**Compiler Defines:**
- CSIBridge 25: `CSIBRIDGE` and `CSIBRIDGE25`
- CSIBridge 26: `CSIBRIDGE` and `CSIBRIDGE26`
- ETABS: `ETABS`
- ETABS 22: `ETABS` and `ETABS22`

---

### ✅ Issue #3: Added Comprehensive Logging (COMPLETED)

**Problem:** Need to see exactly where and why the crash happens

**Solution:** Added detailed logging throughout the entire plugin initialization flow

**What's Being Logged:**

1. **cPlugin.Main()** - Plugin entry point:
   - Assembly preloading (System.Numerics.Vectors, all Avalonia DLLs)
   - Version of each loaded assembly
   - Complete exception details if anything fails

2. **OpenOrFocusSpeckle()** - CSI model binding:
   - ConnectorBindingsCSI creation
   - Host app name and version
   - Setup.Init() call

3. **CreateOrFocusSpeckle()** - Window manager:
   - Whether MainWindow needs creation or reuse
   - Call to BuildAvaloniaApp().Start(AppMain)
   - **Window properties before Show()**: Width, Height, IsVisible
   - **Detailed exception handling around MainWindow.Show()**
   - MainWindow.Activate() call

4. **AppMain()** - Window creation:
   - MainViewModel creation
   - Streams retrieval
   - MainWindow creation with type info
   - **Event handlers for diagnostics:**
     - `Window.Opened` event
     - `Window.LayoutUpdated` event
     - `PropertyChanged` for Bounds/ClientSize
   - **CSIBridge-specific path confirmation**
   - **Window initial state**: Width, Height, WindowState
   - **Try-catch specifically around MainWindow.Show()** with exception type and inner exception

**Logs Location:**
```
C:\Users\jjhaddad\AppData\Roaming\Speckle\Logs\Coreunknown\SpeckleCoreLog[DATE].txt
```

---

## Current Status

### ❌ Still Crashing - Need Diagnostic Info

The connector still crashes at the same place (`LayoutManager.RaiseEffectiveViewportChanged()`), but now we have comprehensive logging that will tell us:

1. ✅ Assembly versions being loaded
2. ✅ Host application details
3. ✅ Window state before crash
4. ✅ Exact exception type and message
5. ✅ Inner exception details
6. ✅ Which events fire before crash
7. ✅ Whether CSIBridge-specific code path is being used

### Files Modified

1. **ConnectorCSIShared/cPlugin.cs** - Added logging and CSIBridge-specific behavior
2. **DesktopUI2/DesktopUI2/DesktopUI2.csproj** - Downgraded Avalonia to 0.10.18

### Build Status

**Last compilation error fixed:** Changed `e.PropertyName` → `e.Property?.Name` for Avalonia compatibility

**Ready to rebuild and test** when CSIBridge 25 license is available.

---

## Next Steps - When License Available

### 1. Rebuild and Test
```bash
# Close CSIBridge 25 completely
# Rebuild ConnectorCSIBridge25
dotnet build ConnectorCSI/ConnectorCSIBridge25/ConnectorCSIBridge25.csproj --configuration Debug

# Or build entire solution
dotnet build ConnectorCSI/ConnectorCSI.sln --configuration Debug
```

### 2. Run Plugin and Capture Logs

**Before running:**
- Clear or note the timestamp of existing log files
- Make sure no other Speckle processes are running

**Run:**
1. Open CSIBridge 25
2. Load the Speckle plugin
3. Note the exact time it crashes
4. Close CSIBridge

**Collect logs:**
```
C:\Users\jjhaddad\AppData\Roaming\Speckle\Logs\Coreunknown\SpeckleCoreLog[DATE].txt
```

### 3. Analyze Logs

Look for these key log entries:

**Last successful log before crash:**
```
🔧 Calling MainWindow.Show()...
   Window initial state: Width=..., Height=..., WindowState=...
```

**Expected crash log:**
```
❌ CRASH in MainWindow.Show() within AppMain
   Exception type: ...
   Inner exception: ...
```

**What to look for:**
- Window dimensions (Width/Height) - Are they valid? NaN? Negative?
- WindowState - Is it Normal/Maximized/Minimized?
- Any PropertyChanged or LayoutUpdated events that fired
- Inner exception details that might explain the root cause
- Differences from working ETABS logs (if available)

---

## Possible Root Causes to Investigate

Based on the logs, we'll need to investigate:

### 1. Invalid Window Metrics
- CSIBridge might report invalid window dimensions
- DPI/scaling issues
- **Check:** Window.Width, Window.Height, Bounds values in logs

### 2. Graphics Context Conflicts
- CSIBridge 25 might use DirectX/OpenGL that conflicts with Avalonia
- Tried disabling GPU (EGL/WGL) but still crashes
- **Next:** Try forcing software rendering entirely

### 3. Threading Issues
- CSIBridge might handle UI thread differently than ETABS
- Window created on wrong thread
- **Check:** Any threading-related exceptions in logs

### 4. CSIBridge 25 Specific Environment
- CSIBridge 25 is newer than ETABS 21
- Might have more sophisticated rendering that conflicts
- **Check:** Compare CSIBridge 25 vs CSIBridge 26 (if available)

### 5. Avalonia 0.10.18 Bug
- This specific Avalonia version might have a bug triggered by CSIBridge
- **Next:** Try upgrading to Avalonia 0.10.21 (requires upgrading Material.Avalonia too)

---

## Alternative Approaches to Try

If logs don't reveal the issue, try these:

### Option 1: Different Window Initialization
```csharp
// Set explicit window properties before showing
MainWindow = new MainWindow
{
    DataContext = viewModel,
    Width = 400,
    Height = 650,
    WindowStartupLocation = WindowStartupLocation.Manual,
    Position = new PixelPoint(100, 100)
};
```

### Option 2: Force Software Rendering
```csharp
// In BuildAvaloniaApp()
.With(new SkiaOptions
{
    MaxGpuResourceSizeBytes = 8096000,
    CustomGpuFactory = null  // Force software rendering
})
```

### Option 3: Delay Window Show
```csharp
// Show window after a delay to let CSIBridge settle
await Task.Delay(500);
MainWindow.Show();
```

### Option 4: Try Avalonia 0.10.21
- Upgrade Material.Avalonia to support 0.10.21
- Or remove Material.Avalonia dependency if possible
- Rebuild DesktopUI2 with Avalonia 0.10.21

### Option 5: Create Separate CSIBridge-Specific Window Class
- Create a custom window class for CSIBridge that initializes differently
- Avoid the standard Avalonia layout manager
- More invasive but might bypass the crash

---

## Important Context

### Why This Is Tricky

**Same code works in ETABS but not CSIBridge:**
- All CSI connectors share `ConnectorCSIShared/cPlugin.cs`
- All use the same DesktopUI2 component
- All use the same Avalonia 0.10.18
- Something about CSIBridge 25's host environment is incompatible

**Can't debug directly:**
- Crash happens deep in Avalonia's native code
- Can't step through Avalonia.Layout.LayoutManager
- Need to infer the problem from logs and window state

### Reference Files

**Connector Projects:**
- ConnectorCSIBridge25/ConnectorCSIBridge25.csproj
- ConnectorCSIBridge26/ConnectorCSIBridge26.csproj
- ConnectorETABS/ConnectorETABS.csproj
- ConnectorETABS22/ConnectorETABS22.csproj

**Shared Code:**
- ConnectorCSIShared/cPlugin.cs (main file)
- ConnectorCSIShared/ConnectorCSIShared.projitems

**UI Component:**
- DesktopUI2/DesktopUI2/DesktopUI2.csproj

**Previous Documentation:**
- AVALONIA_VERSION_MISMATCH_FIX.md (explains the assembly version issue)

---

## Questions to Answer

With the diagnostic logs, we should be able to answer:

1. **What are the window dimensions when Show() is called?**
   - Valid? NaN? Negative? Zero?

2. **Does CSIBridge report the correct path?**
   - Should see "CSIBridge mode: Using MainWindow.Show()" in logs

3. **Do any events fire before the crash?**
   - Window.Opened?
   - Window.LayoutUpdated?
   - PropertyChanged?

4. **What's the full exception hierarchy?**
   - Main exception type
   - Inner exceptions
   - Stack trace details

5. **Are all assemblies loading correctly?**
   - All Avalonia DLLs showing version 0.10.18?
   - Any load failures?

---

## Contact/Resume

When ready to continue:

1. Rebuild ConnectorCSIBridge25
2. Test with CSIBridge 25
3. Collect the log file from `C:\Users\jjhaddad\AppData\Roaming\Speckle\Logs\Coreunknown\`
4. Share the relevant portion of the log (around the crash time)
5. Analyze the window state and exception details
6. Try one of the alternative approaches based on findings

The detailed logging should finally tell us what makes CSIBridge 25 different from ETABS and why Avalonia's layout manager can't handle it.
