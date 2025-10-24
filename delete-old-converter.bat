@echo off
echo ================================================
echo Finding and Deleting Old Converter DLL
echo ================================================
echo.

REM Check if the file exists
set CONVERTER_PATH=%APPDATA%\Speckle\Kits\Objects\Objects.Converter.ETABS22.dll
echo Checking for old converter at:
echo %CONVERTER_PATH%
echo.

if exist "%CONVERTER_PATH%" (
    echo File FOUND! Deleting...
    del /f /q "%CONVERTER_PATH%"
    if %errorlevel% equ 0 (
        echo SUCCESS: File deleted!
    ) else (
        echo ERROR: Could not delete file. Try running as Administrator.
        echo You can manually delete it using Windows Explorer:
        echo %CONVERTER_PATH%
    )
) else (
    echo File NOT FOUND at that location.
    echo.
    echo Searching for the file in AppData...
    echo.

    REM Search for the file
    dir /s /b "%APPDATA%\Speckle\*.dll" 2>nul | findstr /i "ETABS22"

    echo.
    echo If you see the file above, you can delete it manually.
)

echo.
echo Now let's also check and clean the entire Kits\Objects folder...
echo.

set KITS_FOLDER=%APPDATA%\Speckle\Kits\Objects
if exist "%KITS_FOLDER%" (
    echo Found Kits folder: %KITS_FOLDER%
    echo.
    echo Files in this folder:
    dir /b "%KITS_FOLDER%\*.dll"
    echo.

    REM Ask user if they want to delete all converter DLLs
    echo Do you want to delete ALL converter DLLs from this folder? (Y/N)
    set /p CONFIRM=

    if /i "%CONFIRM%"=="Y" (
        del /f /q "%KITS_FOLDER%\Objects.Converter.*.dll"
        echo Deleted all converter DLLs!
    ) else (
        echo Skipped deletion.
    )
) else (
    echo Kits folder does not exist: %KITS_FOLDER%
    echo This is OK - it means no old converters to clean up.
)

echo.
echo ================================================
echo Done!
echo ================================================
echo.
echo Next step: Rebuild the connector
echo   dotnet build ConnectorCSI\ConnectorETABS22\ConnectorETABS22.csproj -c Debug /p:SkipHusky=true
echo.
pause
