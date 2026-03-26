Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$bashCommand = Get-Command -Name "bash" -ErrorAction SilentlyContinue
$defaultGitBashPath = Join-Path -Path $env:ProgramFiles -ChildPath "Git\bin\bash.exe"
if ($null -eq $bashCommand -and -not (Test-Path -Path $defaultGitBashPath))
{
    Write-Warning "Git Bash was not detected. Claude Code on Windows requires Git for Windows or WSL."
}

Write-Host "Installing or updating Claude Code to the latest native version..."
$installerScript = Invoke-RestMethod -Uri "https://claude.ai/install.ps1"
& ([scriptblock]::Create($installerScript)) latest
if ($LASTEXITCODE -ne 0)
{
    throw "The Claude Code installer failed."
}

$npmCommand = Get-Command -Name "npm" -ErrorAction SilentlyContinue
if ($null -ne $npmCommand)
{
    $globalNodeModulesPath = & $npmCommand.Source root --global
    if ($LASTEXITCODE -eq 0)
    {
        $legacyPackagePath = Join-Path -Path $globalNodeModulesPath.Trim() -ChildPath "@anthropic-ai\claude-code"
        if (Test-Path -Path $legacyPackagePath)
        {
            Write-Host "Removing deprecated npm-based Claude Code installation..."
            & $npmCommand.Source uninstall --global @anthropic-ai/claude-code
            if ($LASTEXITCODE -ne 0)
            {
                throw "Claude Code was installed natively, but removing the deprecated npm package failed."
            }
        }
    }
}

$claudeCommand = Get-Command -Name "claude" -ErrorAction SilentlyContinue
$claudeExecutablePath = if ($null -ne $claudeCommand)
{
    $claudeCommand.Source
}
else
{
    Join-Path -Path $env:USERPROFILE -ChildPath ".local\bin\claude.exe"
}

if (-not (Test-Path -Path $claudeExecutablePath))
{
    throw "Claude Code was installed, but the 'claude' executable could not be found."
}

Write-Host "Claude Code version:"
& $claudeExecutablePath --version
if ($LASTEXITCODE -ne 0)
{
    throw "Claude Code installed, but version verification failed."
}
