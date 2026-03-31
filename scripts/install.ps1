#Requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$GitHubRepo = "LordKuper/agentic-game-studio"
$DefaultInstallDir = Join-Path -Path $env:USERPROFILE -ChildPath "ags"

# --- Install directory ---

$installDir = $DefaultInstallDir
if ($env:AGS_INSTALL_DIR)
{
    $installDir = $env:AGS_INSTALL_DIR
}

if (-not $PSBoundParameters.ContainsKey("InstallDir"))
{
    $userInput = Read-Host "Install directory [$installDir]"
    if ($userInput.Trim() -ne "")
    {
        $installDir = $userInput.Trim()
    }
}

Write-Host "Installing AGS to: $installDir"

if (-not (Test-Path -Path $installDir))
{
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

# --- Resolve latest release ---

Write-Host "Fetching latest release from GitHub..."
$releasesUri = "https://api.github.com/repos/$GitHubRepo/releases/latest"
$headers = @{ "User-Agent" = "ags-installer" }

try
{
    $release = Invoke-RestMethod -Uri $releasesUri -Headers $headers -TimeoutSec 30
}
catch
{
    throw "Failed to fetch release information from GitHub: $_"
}

$asset = $release.assets | Where-Object { $_.name -like "*.zip" } | Select-Object -First 1
if ($null -eq $asset)
{
    throw "No .zip asset found in the latest release ($($release.tag_name))."
}

Write-Host "Latest release: $($release.tag_name)"
Write-Host "Downloading $($asset.name)..."

$zipPath = Join-Path -Path $env:TEMP -ChildPath ("ags-" + [guid]::NewGuid().ToString() + ".zip")
try
{
    Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $zipPath -Headers $headers -TimeoutSec 120 -UseBasicParsing
}
catch
{
    throw "Failed to download release archive: $_"
}

# --- Extract archive ---

Write-Host "Extracting archive to $installDir..."
$extractTemp = Join-Path -Path $env:TEMP -ChildPath ("ags-extract-" + [guid]::NewGuid().ToString())
try
{
    Expand-Archive -Path $zipPath -DestinationPath $extractTemp -Force

    # Copy all extracted files into the install directory, overwriting existing ones
    $extractedItems = Get-ChildItem -Path $extractTemp -Recurse -File
    foreach ($item in $extractedItems)
    {
        $relativePath = $item.FullName.Substring($extractTemp.Length).TrimStart('\', '/')
        $destPath = Join-Path -Path $installDir -ChildPath $relativePath
        $destDir = Split-Path -Path $destPath -Parent
        if (-not (Test-Path -Path $destDir))
        {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        }
        Copy-Item -Path $item.FullName -Destination $destPath -Force
    }
}
finally
{
    if (Test-Path -Path $zipPath) { Remove-Item -Path $zipPath -Force }
    if (Test-Path -Path $extractTemp) { Remove-Item -Path $extractTemp -Recurse -Force }
}

# --- Update user PATH (idempotent) ---

$currentPath = [System.Environment]::GetEnvironmentVariable("PATH", "User")
$pathEntries = $currentPath -split ";" | Where-Object { $_ -ne "" }

if ($pathEntries -notcontains $installDir)
{
    Write-Host "Adding $installDir to user PATH..."
    $newPath = ($pathEntries + $installDir) -join ";"
    [System.Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
    $env:PATH = $env:PATH.TrimEnd(";") + ";" + $installDir
    Write-Host "PATH updated. Restart your shell for it to take effect in new sessions."
}
else
{
    Write-Host "$installDir is already in PATH."
}

# --- Verify installation ---

$agsExe = Join-Path -Path $installDir -ChildPath "ags.exe"
if (-not (Test-Path -Path $agsExe))
{
    throw "Installation completed, but ags.exe was not found at $agsExe."
}

Write-Host ""
Write-Host "Verifying installation..."
& $agsExe -version
if ($LASTEXITCODE -ne 0)
{
    throw "ags.exe was found, but version verification failed (exit code $LASTEXITCODE)."
}

# --- Success ---

Write-Host ""
Write-Host "AGS $($release.tag_name) installed successfully to $installDir"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Open a new terminal so that 'ags' is available on your PATH."
Write-Host "  2. Navigate to your game project directory."
Write-Host "  3. Run 'ags' to start Agentic Game Studio."
