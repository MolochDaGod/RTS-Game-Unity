#!/usr/bin/env pwsh
# Unity WebGL Build Script

$ErrorActionPreference = "Stop"

# Configuration
$UnityPath = "C:\Program Files\Unity\Hub\Editor\2021.3.42f1\Editor\Unity.exe"
$ProjectPath = $PSScriptRoot
$BuildPath = Join-Path $ProjectPath "Build"
$LogFile = Join-Path $ProjectPath "Logs\build-webgl.log"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Unity WebGL Build Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if Unity exists
if (-not (Test-Path $UnityPath)) {
    Write-Host "ERROR: Unity not found at: $UnityPath" -ForegroundColor Red
    exit 1
}

Write-Host "Unity Path: $UnityPath" -ForegroundColor Green
Write-Host "Project Path: $ProjectPath" -ForegroundColor Green
Write-Host "Build Path: $BuildPath" -ForegroundColor Green
Write-Host ""

# Create Build directory if it doesn't exist
if (-not (Test-Path $BuildPath)) {
    New-Item -ItemType Directory -Path $BuildPath | Out-Null
    Write-Host "Created Build directory" -ForegroundColor Yellow
}

Write-Host "Starting Unity build..." -ForegroundColor Cyan
Write-Host "This may take 10-30 minutes depending on your system..." -ForegroundColor Yellow
Write-Host ""

# Build command
$BuildArgs = @(
    "-quit",
    "-batchmode",
    "-nographics",
    "-silent-crashes",
    "-projectPath", "`"$ProjectPath`"",
    "-buildTarget", "WebGL",
    "-executeMethod", "WebGLBuilder.BuildWebGLProject",
    "-logFile", "`"$LogFile`""
)

# Execute build
try {
    $process = Start-Process -FilePath $UnityPath -ArgumentList $BuildArgs -PassThru -Wait
    
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    
    if ($process.ExitCode -eq 0) {
        Write-Host "Build completed successfully!" -ForegroundColor Green
        Write-Host "Build location: $BuildPath" -ForegroundColor Green
        
        # Check if index.html exists
        if (Test-Path (Join-Path $BuildPath "index.html")) {
            Write-Host "index.html found - build is valid!" -ForegroundColor Green
            
            # Show build size
            $buildSize = (Get-ChildItem -Path $BuildPath -Recurse | Measure-Object -Property Length -Sum).Sum
            $buildSizeMB = [math]::Round($buildSize / 1MB, 2)
            Write-Host "Total build size: $buildSizeMB MB" -ForegroundColor Cyan
        } else {
            Write-Host "WARNING: index.html not found in build!" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Build failed with exit code: $($process.ExitCode)" -ForegroundColor Red
        Write-Host "Check log file: $LogFile" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. git add Build/" -ForegroundColor White
    Write-Host "2. git commit -m 'Add WebGL build'" -ForegroundColor White
    Write-Host "3. git push origin master" -ForegroundColor White
    Write-Host "4. Deploy to Vercel from https://vercel.com" -ForegroundColor White
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
