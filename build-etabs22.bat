@echo off
REM ETABS22 Build Script (Batch File Version)
REM This script builds the ETABS22 connector and converter with SkipHusky flag
REM to avoid permission issues

echo ========================================
echo ETABS22 Clean ^& Rebuild Script
echo ========================================
echo.

echo Step 1: Cleaning solution...
dotnet clean All.sln
if %errorlevel% neq 0 (
    echo ERROR: Failed to clean solution
    pause
    exit /b 1
)

echo.
echo Step 2: Removing all bin and obj folders...
for /d /r . %%d in (bin obj) do @if exist "%%d" rd /s /q "%%d"

echo.
echo Step 3: Cleaning ETABS 22 Plug-Ins folder...
set PLUGINS_FOLDER=C:\ProgramData\Computers and Structures\ETABS 22\Plug-Ins
if exist "%PLUGINS_FOLDER%" (
    del /q "%PLUGINS_FOLDER%\*.dll" 2>nul
    echo   Cleaned: %PLUGINS_FOLDER%
) else (
    echo   Warning: Plug-Ins folder not found: %PLUGINS_FOLDER%
)

echo.
echo Step 4: Cleaning AppData Kits folder (to prevent old DLL loading)...
set KITS_FOLDER=%APPDATA%\Speckle\Kits\Objects
if exist "%KITS_FOLDER%" (
    del /q "%KITS_FOLDER%\Objects.Converter.ETABS22.dll" 2>nul
    echo   Cleaned old converter from: %KITS_FOLDER%
) else (
    echo   Kits folder not found (this is OK)
)

echo.
echo Step 5: Building projects in dependency order...
echo.

echo   [1/5] Building Core...
dotnet build Core\Core\Core.csproj -c Debug /p:SkipHusky=true --verbosity minimal
if %errorlevel% neq 0 (
    echo     ERROR: Core build failed!
    pause
    exit /b 1
)

echo   [2/5] Building Objects...
dotnet build Objects\Objects\Objects.csproj -c Debug /p:SkipHusky=true --verbosity minimal
if %errorlevel% neq 0 (
    echo     ERROR: Objects build failed!
    pause
    exit /b 1
)

echo   [3/5] Building DesktopUI2...
dotnet build DesktopUI2\DesktopUI2\DesktopUI2.csproj -c Debug /p:SkipHusky=true --verbosity minimal
if %errorlevel% neq 0 (
    echo     ERROR: DesktopUI2 build failed!
    pause
    exit /b 1
)

echo   [4/5] Building ConverterETABS22 (MUST be fresh for type identity)...
echo         Cleaning converter bin folder first...
if exist "Objects\Converters\ConverterCSI\ConverterETABS22\bin\Debug" (
    rd /s /q "Objects\Converters\ConverterCSI\ConverterETABS22\bin\Debug"
)
dotnet build Objects\Converters\ConverterCSI\ConverterETABS22\ConverterETABS22.csproj -c Debug /p:SkipHusky=true --verbosity minimal
if %errorlevel% neq 0 (
    echo     ERROR: ConverterETABS22 build failed!
    pause
    exit /b 1
)

echo   [5/5] Building ConnectorETABS22 (will auto-copy fresh converter)...
echo         Cleaning connector bin folder first...
if exist "ConnectorCSI\ConnectorETABS22\bin\Debug" (
    rd /s /q "ConnectorCSI\ConnectorETABS22\bin\Debug"
)
dotnet build ConnectorCSI\ConnectorETABS22\ConnectorETABS22.csproj -c Debug /p:SkipHusky=true --verbosity minimal
if %errorlevel% neq 0 (
    echo     ERROR: ConnectorETABS22 build failed!
    pause
    exit /b 1
)

echo.
echo Step 6: Verifying DLLs in ETABS Plug-Ins folder...
if exist "%PLUGINS_FOLDER%" (
    echo.
    echo   DLLs in ETABS Plug-Ins folder:
    dir "%PLUGINS_FOLDER%\*.dll" /b
    echo.

    REM Check for critical DLLs
    set MISSING=0
    if not exist "%PLUGINS_FOLDER%\SpeckleConnectorCSI.dll" (
        echo   WARNING: Missing SpeckleConnectorCSI.dll
        set MISSING=1
    )
    if not exist "%PLUGINS_FOLDER%\Objects.dll" (
        echo   WARNING: Missing Objects.dll
        set MISSING=1
    )
    if not exist "%PLUGINS_FOLDER%\Speckle.Core.dll" (
        echo   WARNING: Missing Speckle.Core.dll
        set MISSING=1
    )
    if not exist "%PLUGINS_FOLDER%\Objects.Converter.ETABS22.dll" (
        echo   WARNING: Missing Objects.Converter.ETABS22.dll
        set MISSING=1
    )

    if %MISSING%==0 (
        echo   All critical DLLs present!
    )
) else (
    echo   ERROR: Plug-Ins folder not found!
)

echo.
echo ========================================
echo Build Complete!
echo ========================================
echo.
echo Next steps:
echo   1. Close ETABS 22 if it's running
echo   2. Launch ETABS 22
echo   3. Open your model
echo   4. Try sending objects to Speckle
echo   5. Check logs for diagnostic info
echo.
pause
