<#
.SYNOPSIS
    Runs the YubiOn FSS .NET SDK Sample Web Application.
.DESCRIPTION
    Launches the ASP.NET Core Web API + SPA application on http://localhost:5000 (or your configured RP ID).
#>
[CmdletBinding()]
param(
    [string]$Url = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = (Get-Location).Path
}

$projectDir = Join-Path $scriptDir "YubionFssSdkSample"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Starting YubiOn FSS .NET SDK Sample Web" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$appsettingsPath = Join-Path $projectDir "appsettings.json"
if (Test-Path $appsettingsPath) {
    $configContent = Get-Content $appsettingsPath -Raw
    if ($configContent -match "your_api_key_id") {
        Write-Host "`n[Notice] It seems appsettings.json still contains placeholder values ('your_api_key_id')." -ForegroundColor Yellow
        Write-Host "Please make sure to set your actual FSS RP ID and API credentials before running FIDO2 operations." -ForegroundColor Yellow
    }
}

Write-Host "`nRunning application with: dotnet run --project `"$projectDir`"" -ForegroundColor Green
Write-Host "Access the application at: $Url" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop the server.`n" -ForegroundColor Gray

dotnet run --project "$projectDir" --urls "$Url"
