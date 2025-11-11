#
# Setup Git hooks for the OrderTaking project (Windows PowerShell)
# Run this script after cloning the repository
#

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$HooksDir = Join-Path $ProjectRoot ".git\hooks"

Write-Host "🔧 Setting up Git hooks..." -ForegroundColor Cyan

# Check if .git directory exists
if (-not (Test-Path (Join-Path $ProjectRoot ".git"))) {
    Write-Host "❌ Error: .git directory not found" -ForegroundColor Red
    Write-Host "Please run this script from the project root or ensure the repository is initialized"
    exit 1
}

# Check if pre-commit hook exists
$PreCommitPath = Join-Path $HooksDir "pre-commit"
if (Test-Path $PreCommitPath) {
    Write-Host "⚠️  pre-commit hook already exists" -ForegroundColor Yellow
    $response = Read-Host "Do you want to replace it? (y/N)"
    if ($response -notmatch "^[Yy]$") {
        Write-Host "Skipping pre-commit hook setup"
        exit 0
    }
    Remove-Item $PreCommitPath -Force
}

# Copy pre-commit hook
$SourceHook = Join-Path $ScriptDir "hooks\pre-commit"
Copy-Item $SourceHook $PreCommitPath -Force

Write-Host "✅ Git hooks installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "The pre-commit hook will run the following checks:"
Write-Host "  1. Format check (Fantomas)"
Write-Host "  2. Build (0 warnings)"
Write-Host "  3. Tests (all passing)"
Write-Host ""
Write-Host "To skip hooks for a specific commit, use: git commit --no-verify"
