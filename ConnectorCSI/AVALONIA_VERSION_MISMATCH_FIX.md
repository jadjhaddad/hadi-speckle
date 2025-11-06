# Avalonia Version Mismatch Issue - CSIBridge 25 Connector

## Problem Summary

When loading the Speckle connector plugin in CSIBridge 25, the following error occurs:

```
System.IO.FileLoadException: Could not load file or assembly 'Avalonia.Base, Version=0.10.18.0,
Culture=neutral, PublicKeyToken=c8d484a7012f9a8b' or one of its dependencies.
A strongly-named assembly is required. (Exception from HRESULT: 0x80131044)
```

**Error Location:** `DesktopUI2.App.Initialize()` during XAML initialization

## Root Cause Analysis

### 1. Version Mismatch
- **DesktopUI2.dll** was compiled with **Avalonia 0.10.18.0** references embedded in the compiled XAML
- The **Plug-Ins folder** contains **Avalonia 0.10.21.0** DLLs
- The compiled XAML code has hardcoded type references to version 0.10.18.0

### 2. Assembly Binding Context
- The plugin runs inside **CSiBridge.exe's AppDomain**
- Only **CSiBridge.exe.config** controls assembly binding redirects for the entire process
- The plugin's `app.config` file is **ignored** for assembly binding
- The `OnAssemblyResolve` handler fires **too late** - XAML initialization resolves assemblies internally before the handler is called

### 3. Security Restrictions
- CSiBridge loads plugins from `C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\`
- .NET Code Access Security (CAS) treats this as a semi-trusted location
- **SpeckleConnectorCSI.dll** is unsigned
- **Avalonia assemblies** are strongly-named (signed)
- Unsigned assemblies loading signed assemblies from semi-trusted locations trigger security errors

## Available Solutions

### Solution 1: Rebuild DesktopUI2 Against Avalonia 0.10.21.0 ✅ **RECOMMENDED**

This is the cleanest and most permanent fix.

**Steps:**
1. Check which Avalonia version DesktopUI2 currently references:
   ```bash
   # In DesktopUI2.csproj
   <PackageReference Include="Avalonia" Version="?" />
   ```

2. Update DesktopUI2/DesktopUI2.csproj to use Avalonia 0.10.21:
   ```xml
   <PackageReference Include="Avalonia" Version="0.10.21" />
   <PackageReference Include="Avalonia.Desktop" Version="0.10.21" />
   <PackageReference Include="Avalonia.Diagnostics" Version="0.10.21" />
   <PackageReference Include="Avalonia.ReactiveUI" Version="0.10.21" />
   ```

3. Rebuild DesktopUI2:
   ```bash
   dotnet clean DesktopUI2/DesktopUI2/DesktopUI2.csproj
   dotnet build DesktopUI2/DesktopUI2/DesktopUI2.csproj --configuration Debug
   ```

4. Rebuild ConnectorCSIBridge25:
   ```bash
   dotnet build ConnectorCSI/ConnectorCSIBridge25/ConnectorCSIBridge25.csproj --configuration Debug
   ```

**Pros:**
- Permanent fix
- No modifications to CSI software files
- Works for all CSI product versions
- Clean solution

**Cons:**
- Requires rebuilding projects
- May affect other connectors that use DesktopUI2

---

### Solution 2: Downgrade Avalonia to 0.10.18.0

Match the Avalonia version to what DesktopUI2 was compiled against.

**Steps:**
1. Update DesktopUI2/DesktopUI2.csproj:
   ```xml
   <PackageReference Include="Avalonia" Version="0.10.18" />
   <PackageReference Include="Avalonia.Desktop" Version="0.10.18" />
   <PackageReference Include="Avalonia.Diagnostics" Version="0.10.18" />
   <PackageReference Include="Avalonia.ReactiveUI" Version="0.10.18" />
   ```

2. Rebuild both projects as in Solution 1

**Pros:**
- Guaranteed compatibility
- No CSI file modifications

**Cons:**
- Uses older Avalonia version (may have bugs/missing features)
- Requires rebuilding projects

---

### Solution 3: Add Binding Redirects to CSiBridge.exe.config ⚠️ **REQUIRES ADMIN**

Tell CSiBridge.exe to redirect Avalonia 0.10.18.0 requests to 0.10.21.0.

**Warning:** This modifies a CSI software file and requires administrator privileges.

**Steps:**
1. **Backup the original file:**
   ```
   C:\Program Files\Computers and Structures\CSiBridge 25\CSiBridge.exe.config
   ```

2. **Open as Administrator:** Right-click Notepad → "Run as Administrator" → Open the file

3. **Add this section** before the closing `</runtime>` tag (around line 110):
   ```xml
   <!-- Avalonia binding redirects for Speckle plugin -->
   <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Base" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Controls" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Visuals" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Animation" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Input" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Interactivity" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Layout" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Styling" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Markup" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
     <dependentAssembly>
       <assemblyIdentity name="Avalonia.Markup.Xaml" publicKeyToken="c8d484a7012f9a8b" culture="neutral" />
       <bindingRedirect oldVersion="0.0.0.0-0.10.21.0" newVersion="0.10.21.0" />
     </dependentAssembly>
   </assemblyBinding>
   ```

4. Save the file (may need to disable read-only attribute)

**Pros:**
- No rebuilding required
- Immediate fix

**Cons:**
- Requires admin rights
- Modifies CSI software files (may be overwritten by updates)
- Must be repeated for each CSI product (CSiBridge 26, ETABS, SAP2000, etc.)
- May cause issues with CSI software if they use different Avalonia versions

---

## Code Changes Already Applied

The following changes have been made to `ConnectorCSIShared/cPlugin.cs` to help with assembly loading:

### 1. OnAssemblyResolve Handler (Lines 164-194)
Changed from `Assembly.LoadFile()` to `Assembly.Load(bytes)`:
- Loads assemblies from bytes to bypass security restrictions
- Properly parses assembly names to ignore version requirements
- Logs what's being loaded for debugging

### 2. Preloading Code (Lines 206-239)
Changed preloading to use `Assembly.Load(bytes)`:
- Preloads critical assemblies before XAML initialization
- Bypasses Code Access Security restrictions
- Loads into the default load context

**Note:** These changes help with security errors but **do not solve the version mismatch** issue. You still need to implement one of the solutions above.

---

## Diagnostic Information

### Current State
- **DesktopUI2.dll**: UNSIGNED, references Avalonia 0.10.18.0 and 0.10.21.0 (mixed)
- **SpeckleConnectorCSI.dll**: UNSIGNED
- **Avalonia DLLs**: Version 0.10.21.0, SIGNED (except Avalonia.ReactiveUI.dll which is UNSIGNED)
- **CSiBridge.exe.config**: No Avalonia binding redirects

### How to Check DesktopUI2's Avalonia References
```powershell
$bytes = [System.IO.File]::ReadAllBytes('C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\DesktopUI2.dll')
$asm = [System.Reflection.Assembly]::Load($bytes)
$refs = $asm.GetReferencedAssemblies()
$refs | Where-Object { $_.Name -like 'Avalonia*' } | ForEach-Object {
    Write-Output "$($_.Name) | Version: $($_.Version)"
}
```

### How to Check Avalonia DLL Versions in Plug-Ins Folder
```powershell
Get-ChildItem 'C:\ProgramData\Computers and Structures\CSiBridge 25\Plug-Ins\' -Filter 'Avalonia*.dll' |
ForEach-Object {
    $asm = [System.Reflection.AssemblyName]::GetAssemblyName($_.FullName)
    $token = if ($asm.GetPublicKeyToken()) {
        [BitConverter]::ToString($asm.GetPublicKeyToken()) -replace '-',''
    } else {
        'UNSIGNED'
    }
    Write-Output "$($_.Name) | Version: $($asm.Version) | Token: $token"
}
```

---

## Recommendation

**Use Solution 1** - Rebuild DesktopUI2 to reference Avalonia 0.10.21.0 consistently. This is the cleanest approach and won't require any modifications to CSI software files or admin privileges.

If you need the plugin working immediately and have admin rights, use Solution 3 as a temporary workaround while implementing Solution 1.
