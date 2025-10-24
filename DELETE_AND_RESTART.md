# CRITICAL: Must Delete AppData DLL and Restart ETABS

## The Problem

Even though the build copies the converter to the bin folder, .NET loads assemblies only once per process. ETABS already loaded the old converter from AppData, so it keeps using that one.

From your logs:
```
📍 Loading converter from: ...\bin\Debug\net48\Objects.Converter.ETABS22.dll ← We TRY to load from here
🔬 rawInstance loaded from: ...\AppData\Roaming\...\Objects.Converter.ETABS22.dll ← But .NET uses this one!
```

## The Solution: Delete File AND Restart ETABS

### Step 1: Close ETABS
**Close ETABS completely** before deleting the file.

### Step 2: Delete the AppData File

**In Windows Explorer:**
1. Press `Win + R`
2. Type: `%APPDATA%\Speckle\Kits\Objects`
3. Press Enter
4. **Delete** `Objects.Converter.ETABS22.dll`

**Or in PowerShell:**
```powershell
Remove-Item "$env:APPDATA\Speckle\Kits\Objects\Objects.Converter.ETABS22.dll" -Force
```

**Or in Command Prompt:**
```cmd
del "%APPDATA%\Speckle\Kits\Objects\Objects.Converter.ETABS22.dll"
```

### Step 3: Verify It's Gone

Check the folder to make sure the file is really deleted:
```
%APPDATA%\Speckle\Kits\Objects
```

You should NOT see `Objects.Converter.ETABS22.dll` in that folder.

### Step 4: Start ETABS

Now launch ETABS fresh. It will load the converter from the bin folder since the AppData one is gone.

### Step 5: Test

Try sending objects. The logs should show:

```
🔬 rawInstance loaded from: ...\ConnectorETABS22\bin\Debug\net48\Objects.Converter.ETABS22.dll ✅
🔍 Model is Base? True ✅
🔍 Is Model assignable to Base? True ✅
```

## Why This Happens

.NET's Assembly Loading Rules:
1. When an assembly is loaded, .NET caches it in memory for the entire process lifetime
2. If you try to load the same assembly again (same name), .NET uses the cached version
3. Even if you specify a different path, .NET won't reload it
4. The only way to change which assembly is used is to restart the process (ETABS)

That's why you MUST:
- Delete the AppData file
- Close ETABS completely
- Start ETABS again

The next time ETABS starts, it will load the converter from bin folder (since AppData is gone), and type identity will be preserved!
