# ETABS22 Build Script (No Permission Issues)
# This script builds the ETABS22 connector and converter with SkipHusky flag
# to avoid permission issues with dotnet tool restore and husky install

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ETABS22 Clean & Rebuild Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to solution root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

Write-Host "Step 1: Cleaning solution..." -ForegroundColor Yellow
dotnet clean All.sln

Write-Host ""
Write-Host "Step 2: Removing all bin and obj folders..." -ForegroundColor Yellow
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Step 3: Cleaning ETABS 22 Plug-Ins folder..." -ForegroundColor Yellow
$pluginsFolder = "C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins"
if (Test-Path $pluginsFolder) {
    Remove-Item "$pluginsFolder\*.dll" -Force -ErrorAction SilentlyContinue
    Write-Host "  Cleaned: $pluginsFolder" -ForegroundColor Green
} else {
    Write-Host "  Warning: Plug-Ins folder not found: $pluginsFolder" -ForegroundColor Red
}

Write-Host ""
Write-Host "Step 4: Building projects in dependency order..." -ForegroundColor Yellow
Write-Host ""

# Build with SkipHusky flag to avoid permission issues
$buildParams = @("/p:SkipHusky=true", "-c", "Debug", "--verbosity", "minimal")

Write-Host "  [1/5] Building Core..." -ForegroundColor Cyan
dotnet build Core\Core\Core.csproj @buildParams
if ($LASTEXITCODE -ne 0) {
    Write-Host "    ERROR: Core build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "  [2/5] Building Objects..." -ForegroundColor Cyan
dotnet build Objects\Objects\Objects.csproj @buildParams
if ($LASTEXITCODE -ne 0) {
    Write-Host "    ERROR: Objects build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "  [3/5] Building DesktopUI2..." -ForegroundColor Cyan
dotnet build DesktopUI2\DesktopUI2\DesktopUI2.csproj @buildParams
if ($LASTEXITCODE -ne 0) {
    Write-Host "    ERROR: DesktopUI2 build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "  [4/5] Building ConverterETABS22..." -ForegroundColor Cyan
dotnet build Objects\Converters\ConverterCSI\ConverterETABS22\ConverterETABS22.csproj @buildParams
if ($LASTEXITCODE -ne 0) {
    Write-Host "    ERROR: ConverterETABS22 build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "  [5/5] Building ConnectorETABS22..." -ForegroundColor Cyan
dotnet build ConnectorCSI\ConnectorETABS22\ConnectorETABS22.csproj @buildParams
if ($LASTEXITCODE -ne 0) {
    Write-Host "    ERROR: ConnectorETABS22 build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 5: Verifying DLLs in ETABS Plug-Ins folder..." -ForegroundColor Yellow
if (Test-Path $pluginsFolder) {
    $dlls = Get-ChildItem $pluginsFolder -Filter *.dll | Select-Object Name, Length, LastWriteTime
    if ($dlls) {
        Write-Host ""
        Write-Host "  DLLs in ETABS Plug-Ins folder:" -ForegroundColor Green
        $dlls | Format-Table -AutoSize | Out-String | Write-Host

        # Check for critical DLLs
        $criticalDlls = @(
            "SpeckleConnectorCSI.dll",
            "Objects.dll",
            "Speckle.Core.dll",
            "Objects.Converter.ETABS22.dll"
        )

        $missing = @()
        foreach ($dll in $criticalDlls) {
            if (-not (Test-Path (Join-Path $pluginsFolder $dll))) {
                $missing += $dll
            }
        }

        if ($missing.Count -gt 0) {
            Write-Host "  WARNING: Missing critical DLLs:" -ForegroundColor Red
            $missing | ForEach-Object { Write-Host "    - $_" -ForegroundColor Red }
        } else {
            Write-Host "  All critical DLLs present!" -ForegroundColor Green
        }
    } else {
        Write-Host "  WARNING: No DLLs found in Plug-Ins folder!" -ForegroundColor Red
    }
} else {
    Write-Host "  ERROR: Plug-Ins folder not found!" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Close ETABS 22 if it's running" -ForegroundColor White
Write-Host "  2. Launch ETABS 22" -ForegroundColor White
Write-Host "  3. Open your model" -ForegroundColor White
Write-Host "  4. Try sending objects to Speckle" -ForegroundColor White
Write-Host "  5. Check logs for diagnostic info (look for 📍 and 🔍 emojis)" -ForegroundColor White
Write-Host ""
