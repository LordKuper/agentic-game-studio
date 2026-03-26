Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$skippedExitCode = 10
$npmCommand = Get-Command -Name "npm" -ErrorAction SilentlyContinue
if ($null -eq $npmCommand)
{
    throw "npm was not found. Install Node.js and ensure npm is available on PATH."
}

$runningCodexProcesses = @(Get-Process -Name "codex" -ErrorAction SilentlyContinue)
if ($runningCodexProcesses.Count -gt 0)
{
    Write-Host "Codex CLI update is skipped because Codex is currently running."
    Write-Host "Windows locks codex.exe while the CLI is active, so npm cannot replace it."
    foreach ($runningCodexProcess in $runningCodexProcesses)
    {
        $processPath = if ([string]::IsNullOrWhiteSpace($runningCodexProcess.Path))
        {
            "<unknown>"
        }
        else
        {
            $runningCodexProcess.Path
        }

        Write-Host "Close Codex process $($runningCodexProcess.Id): $processPath"
    }

    exit $skippedExitCode
}

Write-Host "Installing or updating Codex CLI to the latest version..."
& $npmCommand.Source install --global @openai/codex@latest
if ($LASTEXITCODE -ne 0)
{
    throw "npm failed to install or update @openai/codex."
}

$codexCommand = Get-Command -Name "codex" -ErrorAction SilentlyContinue
if ($null -eq $codexCommand)
{
    throw "Codex CLI was installed, but the 'codex' command was not found on PATH."
}

Write-Host "Codex CLI version:"
& $codexCommand.Source --version
if ($LASTEXITCODE -ne 0)
{
    throw "Codex CLI installed, but version verification failed."
}
