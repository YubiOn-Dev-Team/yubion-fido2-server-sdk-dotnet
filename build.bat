@echo off
setlocal enabledelayedexpansion

echo =========================================
echo  Building YubiOn FSS .NET SDK
echo =========================================

set SCRIPT_DIR=%~dp0
set ARTIFACTS_DIR=%SCRIPT_DIR%artifacts
set NET80_DIR=%ARTIFACTS_DIR%\dll\net8.0
set NET60_DIR=%ARTIFACTS_DIR%\dll\net6.0

if exist "%ARTIFACTS_DIR%" (
    echo Cleaning artifacts directory...
    rmdir /s /q "%ARTIFACTS_DIR%"
)

mkdir "%NET80_DIR%" >nul 2>&1
mkdir "%NET60_DIR%" >nul 2>&1

echo.
echo [1/4] Restoring dependencies...
dotnet restore "%SCRIPT_DIR%YubionFssSdk.sln"
if errorlevel 1 goto error

echo.
echo [2/4] Building solution in Release mode...
dotnet build "%SCRIPT_DIR%YubionFssSdk.sln" -c Release --no-restore
if errorlevel 1 goto error

echo.
echo [3/4] Running tests...
dotnet test "%SCRIPT_DIR%YubionFssSdk.sln" -c Release --no-build
if errorlevel 1 goto error

echo.
echo [4/4] Creating NuGet package and collecting DLLs...
dotnet pack "%SCRIPT_DIR%src\YubiOn.Fss.Sdk\YubiOn.Fss.Sdk.csproj" -c Release --no-build -o "%ARTIFACTS_DIR%"
if errorlevel 1 goto error

copy "%SCRIPT_DIR%src\YubiOn.Fss.Sdk\bin\Release\net8.0\YubiOn.Fss.Sdk.*" "%NET80_DIR%\" >nul
copy "%SCRIPT_DIR%src\YubiOn.Fss.Sdk\bin\Release\net6.0\YubiOn.Fss.Sdk.*" "%NET60_DIR%\" >nul

echo.
echo =========================================
echo  Build succeeded!
echo  Artifacts created in: %ARTIFACTS_DIR%
echo   - NuGet Packages:  %ARTIFACTS_DIR%\*.nupkg
echo   - .NET 8.0 DLL:    %NET80_DIR%
echo   - .NET 6.0 DLL:    %NET60_DIR%
echo =========================================
exit /b 0

:error
echo.
echo [ERROR] Build failed!
exit /b 1
