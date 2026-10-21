<#
.SYNOPSIS
    Builds, tests, and packages the YubiOn FSS .NET SDK.
.DESCRIPTION
    Builds the solution in Release mode for all target frameworks (net8.0, net6.0),
    runs unit tests, and outputs NuGet packages (.nupkg) and DLLs to the artifacts/ directory.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = $PSScriptRoot
if (-not $scriptRoot) {
    $scriptRoot = (Get-Location).Path
}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Building YubiOn FSS .NET SDK" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$artifactsDir = Join-Path $scriptRoot "artifacts"
$dllDir = Join-Path $artifactsDir "dll"
$net80DllDir = Join-Path $dllDir "net8.0"
$net60DllDir = Join-Path $dllDir "net6.0"

if (Test-Path $artifactsDir) {
    Write-Host "Cleaning artifacts directory..." -ForegroundColor Yellow
    Remove-Item -Path $artifactsDir -Recurse -Force
}

New-Item -ItemType Directory -Path $net80DllDir -Force | Out-Null
New-Item -ItemType Directory -Path $net60DllDir -Force | Out-Null

Write-Host "`n[1/4] Restoring dependencies..." -ForegroundColor Green
dotnet restore "$scriptRoot\YubionFssSdk.sln"

Write-Host "`n[2/4] Building solution in Release mode..." -ForegroundColor Green
dotnet build "$scriptRoot\YubionFssSdk.sln" -c Release --no-restore

Write-Host "`n[3/4] Running tests..." -ForegroundColor Green
dotnet test "$scriptRoot\YubionFssSdk.sln" -c Release --no-build

Write-Host "`n[4/4] Creating NuGet package and collecting DLLs..." -ForegroundColor Green
dotnet pack "$scriptRoot\src\YubiOn.Fss.Sdk\YubiOn.Fss.Sdk.csproj" -c Release --no-build -o "$artifactsDir"

# Copy compiled DLLs and XML documentation
Copy-Item "$scriptRoot\src\YubiOn.Fss.Sdk\bin\Release\net8.0\YubiOn.Fss.Sdk.*" "$net80DllDir" -Force
Copy-Item "$scriptRoot\src\YubiOn.Fss.Sdk\bin\Release\net6.0\YubiOn.Fss.Sdk.*" "$net60DllDir" -Force

Write-Host "`n=========================================" -ForegroundColor Green
Write-Host " Build succeeded!" -ForegroundColor Green
Write-Host " Artifacts created in: $artifactsDir" -ForegroundColor Green
Write-Host "  - NuGet Packages:  $artifactsDir\*.nupkg" -ForegroundColor Green
Write-Host "  - .NET 8.0 DLL:    $net80DllDir" -ForegroundColor Green
Write-Host "  - .NET 6.0 DLL:    $net60DllDir" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
