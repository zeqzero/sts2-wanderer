# Queues up AutoSlay runs of the local STS2 install (with the Wanderer mod
# loaded) and stores per-run logs + a summary CSV under util/test-results/.
#
# Requires the AutoSlay_Patches.cs Harmony patches to be in the loaded mod
# DLL (built with `dotnet build`, default Debug config).
#
# Examples:
#   ./util/RunAutoSlay.ps1                          # 5 runs, random seeds
#   ./util/RunAutoSlay.ps1 -RunCount 20             # 20 runs, random seeds
#   ./util/RunAutoSlay.ps1 -Seeds ABC123,DEF456     # specific seeds
#   ./util/RunAutoSlay.ps1 -TimeoutMinutes 10       # tighter per-run timeout

param(
    [int]$RunCount = 5,
    [string[]]$Seeds,
    [string]$Sts2Path,
    [int]$TimeoutMinutes = 30,
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptRoot

# --- Ensure Debug DLL is deployed. A prior `dotnet build -c Release` silently
#     strips the #if DEBUG-gated AutoSlay Harmony patches and the game will
#     boot to main menu instead of starting autoslay.
if (-not $SkipBuild) {
    Write-Host "Building mod (Debug)..."
    & dotnet build "$projectRoot/Wanderer.csproj" --configuration Debug --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed (exit $LASTEXITCODE)" }
}

# --- Locate STS2 ---
if (-not $Sts2Path) {
    $steam = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction SilentlyContinue).SteamPath
    if ($steam) {
        $candidate = Join-Path $steam 'steamapps/common/Slay the Spire 2'
        if (Test-Path $candidate) { $Sts2Path = $candidate }
    }
    if (-not $Sts2Path) {
        $Sts2Path = 'C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2'
    }
}
$exe = Join-Path $Sts2Path 'SlaytheSpire2.exe'
if (-not (Test-Path $exe)) {
    throw "STS2 executable not found at: $exe (pass -Sts2Path to override)"
}

# --- Generate seeds in STS2's canonical format if none supplied ---
# Matches SeedHelper: 10 chars from 0-9 + A-Z minus I and O.
$seedAlphabet = '0123456789ABCDEFGHJKLMNPQRSTUVWXYZ'.ToCharArray()
function New-StsSeed {
    -join (1..10 | ForEach-Object { $seedAlphabet | Get-Random })
}

if (-not $Seeds -or $Seeds.Count -eq 0) {
    $Seeds = 1..$RunCount | ForEach-Object { New-StsSeed }
}

# --- Set up batch dir under util/test-results ---
$resultsRoot = Join-Path $scriptRoot 'test-results'
$null = New-Item -ItemType Directory -Path $resultsRoot -Force
$batchTimestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$batchDir = Join-Path $resultsRoot "batch-$batchTimestamp"
$null = New-Item -ItemType Directory -Path $batchDir
Write-Host "Batch dir: $batchDir"
Write-Host "STS2: $exe"
Write-Host "Runs: $($Seeds.Count) (timeout $TimeoutMinutes min each)"
Write-Host ""

# --- Run loop ---
$summary = @()
$index = 0
foreach ($seed in $Seeds) {
    $index++
    $runLabel = '{0:D3}-{1}' -f $index, $seed
    $logPath = Join-Path $batchDir "$runLabel.log"
    Write-Host "[$index/$($Seeds.Count)] seed=$seed"

    $start = Get-Date
    $proc = Start-Process -FilePath $exe -PassThru -WorkingDirectory $Sts2Path `
        -ArgumentList @('--autoslay', "--seed=$seed", "--log-file=$logPath")

    $finished = $proc.WaitForExit($TimeoutMinutes * 60 * 1000)
    $duration = (Get-Date) - $start

    if (-not $finished) {
        Write-Warning "  timed out after $TimeoutMinutes min, killing process"
        try { $proc.Kill() } catch { }
        $proc.WaitForExit()
        $exitCode = -1
        $status = 'TIMEOUT'
    } else {
        $exitCode = $proc.ExitCode
        $status = if ($exitCode -eq 0) { 'PASS' } else { 'FAIL' }
    }

    $durStr = '{0:mm\:ss}' -f $duration
    Write-Host "  -> $status (exit=$exitCode, duration=$durStr)"

    $summary += [PSCustomObject]@{
        Index    = $index
        Seed     = $seed
        Status   = $status
        ExitCode = $exitCode
        Duration = $durStr
        Log      = "$runLabel.log"
    }
}

# --- Write summary CSV + console table ---
$summaryPath = Join-Path $batchDir 'summary.csv'
$summary | Export-Csv -Path $summaryPath -NoTypeInformation
Write-Host ""
$summary | Format-Table -AutoSize

$pass    = ($summary | Where-Object Status -eq 'PASS').Count
$fail    = ($summary | Where-Object Status -eq 'FAIL').Count
$timeout = ($summary | Where-Object Status -eq 'TIMEOUT').Count
Write-Host "Done: $pass passed, $fail failed, $timeout timed out"
Write-Host "Results: $batchDir"
