# REBUILD REQUIRED - New Code Is Not Running Yet

## The Problem

Your logs show you're still running the OLD code with Assembly.LoadFrom().
The new fix has been committed but NOT yet built.

## Evidence from Your Logs

Old code (what you're running now):
```
🚧 Manually loading ObjectsKit and ETABS22 converter.
📍 Loading converter from: ...\bin\Debug\net48\Objects.Converter.ETABS22.dll
🔬 rawInstance loaded from: ...\AppData\Roaming\...\Objects.Converter.ETABS22.dll
```

New code (what you should see after rebuild):
```
✅ Using direct converter reference for ETABS22
✅ Created ConverterCSI instance
```

## How to Rebuild

### Step 1: Pull the Latest Changes
```cmd
git pull origin claude/etabs22-support-011CURhHYMJPk3Fq9Bb69KVU
```

### Step 2: Clean Old Build Artifacts
```cmd
cd C:\Users\jjhaddad\source\repos\speckle-sharp-main

rmdir /s /q ConnectorCSI\ConnectorETABS22\bin
rmdir /s /q ConnectorCSI\ConnectorETABS22\obj
rmdir /s /q Objects\Converters\ConverterCSI\ConverterETABS22\bin
rmdir /s /q Objects\Converters\ConverterCSI\ConverterETABS22\obj
```

### Step 3: Rebuild
Use the batch file:
```cmd
build-etabs22.bat
```

Or manually:
```cmd
dotnet clean ConnectorCSI\ConnectorETABS22\ConnectorETABS22.csproj -c Debug
dotnet build ConnectorCSI\ConnectorETABS22\ConnectorETABS22.csproj -c Debug /p:SkipHusky=true
```

### Step 4: Close and Restart ETABS22
**IMPORTANT**: Close ETABS completely and restart it. The old DLL is loaded in memory.

### Step 5: Test Again

When you send objects, you should now see:
```
✅ Using direct converter reference for ETABS22
✅ Created ConverterCSI instance
🔍 Converter type: Objects.Converter.CSI.ConverterCSI
🔍 Model is Base? True ✅
🔍 Is Model assignable to Base? True ✅
```

And the serialization should succeed!

## Why This Will Work

The new code:
1. ✅ No Assembly.LoadFrom() - no separate load contexts
2. ✅ Direct ProjectReference - same Objects.dll instance for both connector and converter
3. ✅ Direct instantiation - `new Objects.Converter.CSI.ConverterCSI()`
4. ✅ Type identity preserved - Model IS Base from same assembly

## If It Still Fails

If after rebuild you still see the old log messages, check:

1. **Are you pulling from the right branch?**
   ```cmd
   git branch --show-current
   ```
   Should show: `claude/etabs22-support-011CURhHYMJPk3Fq9Bb69KVU`

2. **Is the new code actually in the file?**
   Open: `ConnectorCSI\ConnectorCSIShared\UI\ConnectorBindingsCSI.Send.cs`

   Line 35-38 should be:
   ```csharp
   #if ETABS22
     SpeckleLog.Logger.Information("✅ Using direct converter reference for ETABS22");
     var converter = new Objects.Converter.CSI.ConverterCSI();
   ```

3. **Check the csproj file:**
   Open: `ConnectorCSI\ConnectorETABS22\ConnectorETABS22.csproj`

   Should contain:
   ```xml
   <ProjectReference Include="..\..\Objects\Converters\ConverterCSI\ConverterETABS22\ConverterETABS22.csproj" />
   ```
