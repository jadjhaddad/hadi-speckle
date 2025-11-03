# QUICK FIX - Converter Loading from Wrong Location

## The Problem (Found in Logs)

```
📍 Connector assembly location: ...\ConnectorETABS22\bin\Debug\net48\SpeckleConnectorCSI.dll
📍 Objects assembly location: ...\ConnectorETABS22\bin\Debug\net48\Objects.dll ✅ Correct
📍 Core assembly location: ...\ConnectorETABS22\bin\Debug\net48\SpeckleCore2.dll ✅ Correct
📍 Loading converter from: C:\Users\jjhaddad\AppData\Roaming\Speckle\Kits\Objects\Objects.Converter.ETABS22.dll ❌ WRONG!
```

**The converter is loading from AppData instead of the connector's bin folder!**

This causes type identity mismatch even though versions are the same, because the DLLs were built at different times.

## The Solution

1. **Delete the old converter from AppData:**
   ```cmd
   del /q "%APPDATA%\Speckle\Kits\Objects\Objects.Converter.ETABS22.dll"
   ```

2. **Rebuild just the connector** (it will now auto-copy the converter):
   ```cmd
   dotnet build ConnectorCSI\ConnectorETABS22\ConnectorETABS22.csproj -c Debug /p:SkipHusky=true
   ```

The connector build will now automatically copy the converter DLL from the converter's output folder to the connector's bin folder.

## What Was Changed

Added a post-build target to `ConnectorETABS22.csproj` that:
- Copies `Objects.Converter.ETABS22.dll` from the converter's bin folder to the connector's bin folder
- This ensures the converter is loaded from the same location as Objects.dll and Core.dll
- Prevents type identity mismatches

## Verify It Worked

After rebuilding, check the logs again. You should see:

```
📍 Loading converter from: ...\ConnectorETABS22\bin\Debug\net48\Objects.Converter.ETABS22.dll ✅ Correct!
```

**Both the connector AND converter should now load from the same bin folder!**

## If You're Still Getting Errors

If it still loads from AppData, manually delete all DLLs in the Kits folder:

```cmd
del /q "%APPDATA%\Speckle\Kits\Objects\*.dll"
```

Then rebuild the connector again.
