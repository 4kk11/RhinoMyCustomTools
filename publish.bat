@echo off
setlocal enabledelayedexpansion

:: Save the current directory
set "ORIGINAL_DIR=%CD%"
:: Change to the script's directory
cd /d "%~dp0"

:: Initialization
if exist ".\bin\Package" rmdir /s /q ".\bin\Package"

:: Build
dotnet build MyCustomTools.csproj -c Release -o .\bin\Package

:: Copy
xcopy /y /i ".\Yak\manifest.yml" ".\bin\Package"

:: Packaging
cd .\bin\Package
"D:\Program Files\Rhino 8\System\Yak.exe" build

:: Get the name of the generated .yak file
for %%F in (*.yak) do (
    set "YAK_FILE=%%F"
    goto :found_yak
)
:found_yak

if not defined YAK_FILE (
    echo No .yak file found.
    exit /b 1
)

:: Upload the package
"D:\Program Files\Rhino 8\System\Yak.exe" push "%YAK_FILE%"

:: Return to the original directory
cd /d "%ORIGINAL_DIR%"