# build.ps1 — Build the combined nodi Docker image and push to Docker Hub
#
# Usage:
#   .\build.ps1 -Username yourname                    # build + push with tag "latest"
#   .\build.ps1 -Username yourname -Tag 1.0.0         # build + push with a version tag
#   .\build.ps1 -Username yourname -NoPush            # build only, skip push
#
# Requires: Docker Desktop running, "docker login" already done for push.

param(
    [Parameter(Mandatory)]
    [string] $Username,             # Your Docker Hub username

    [string] $Tag = "latest",       # Image tag, e.g. "1.0.0" or "latest"

    [switch] $NoPush                # Set to skip the docker push step
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Helpers ───────────────────────────────────────────────────────────────────

function Step([string]$msg) {
    Write-Host ""
    Write-Host "──────────────────────────────────────────" -ForegroundColor Cyan
    Write-Host "  $msg" -ForegroundColor Cyan
    Write-Host "──────────────────────────────────────────" -ForegroundColor Cyan
}

function Run([string]$cmd) {
    Write-Host "> $cmd" -ForegroundColor DarkGray
    Invoke-Expression $cmd
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAILED (exit $LASTEXITCODE)" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

# ── Config ────────────────────────────────────────────────────────────────────

$Image = "$Username/nodi:$Tag"

# Move to repo root so the build context covers all projects
$RepoRoot = $PSScriptRoot
Set-Location $RepoRoot

Write-Host ""
Write-Host "nodi build script" -ForegroundColor White
Write-Host "  Image : $Image"
Write-Host "  Push  : $(-not $NoPush)"

# ── Build ─────────────────────────────────────────────────────────────────────

Step "Building nodi (core + web combined)"
Run "docker build -t $Image ."

# ── Push ──────────────────────────────────────────────────────────────────────

if (-not $NoPush) {
    Step "Pushing to Docker Hub"
    Run "docker push $Image"

    Write-Host ""
    Write-Host "Done! Image pushed:" -ForegroundColor Green
    Write-Host "  $Image"
} else {
    Write-Host ""
    Write-Host "Build complete (push skipped)." -ForegroundColor Green
    Write-Host "  $Image"
}
