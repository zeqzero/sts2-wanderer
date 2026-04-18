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
    $logPath    = Join-Path $batchDir "$runLabel.log"
    $stdoutPath = Join-Path $batchDir "$runLabel.stdout.log"
    $stderrPath = Join-Path $batchDir "$runLabel.stderr.log"
    Write-Host "[$index/$($Seeds.Count)] seed=$seed"

    $start = Get-Date
    # Capture raw stdout/stderr from the game exe alongside AutoSlay's own log
    # — stack traces from unhandled exceptions (Harmony prefix throws, etc.)
    # surface via stderr, not the AutoSlay log.
    $proc = Start-Process -FilePath $exe -PassThru -WorkingDirectory $Sts2Path `
        -ArgumentList @('--autoslay', "--seed=$seed", "--log-file=$logPath") `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath

    # A2: Background job that kills the process if stderr grows past 100 MB.
    $stderrLimit = 100MB
    $stderrGuard = Start-Job -ScriptBlock {
        param($pid, $path, $limit)
        while ($true) {
            Start-Sleep -Seconds 5
            try {
                $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if (-not $proc) { break }
                if ((Test-Path $path) -and (Get-Item $path).Length -gt $limit) {
                    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                    break
                }
            } catch { break }
        }
    } -ArgumentList $proc.Id, $stderrPath, $stderrLimit

    $finished = $proc.WaitForExit($TimeoutMinutes * 60 * 1000)
    $duration = (Get-Date) - $start

    # A1: Flush I/O buffers and populate ExitCode (required after redirected streams).
    $proc.WaitForExit()
    $exitCode = $proc.ExitCode

    # Clean up stderr guard job regardless of outcome.
    Stop-Job  -Job $stderrGuard -ErrorAction SilentlyContinue
    Remove-Job -Job $stderrGuard -Force -ErrorAction SilentlyContinue

    if (-not $finished) {
        Write-Warning "  timed out after $TimeoutMinutes min, killing process"
        $exitCode = -1
        $status = 'TIMEOUT'
    } elseif ((Test-Path $stderrPath) -and (Get-Item $stderrPath).Length -gt $stderrLimit) {
        $status = 'STDERR_OVERFLOW'
    } else {
        # A1 + A3: Determine real outcome by parsing the AutoSlay log.
        # Godot may exit 0 even on failure, so the log content is authoritative.
        $logContent = if (Test-Path $logPath) { Get-Content $logPath -Raw } else { '' }
        if ($logContent -match 'Run completed successfully') {
            $status = 'PASS'
        } elseif ($logContent -match 'deck exhausted') {
            # A3: Deck exhaustion is a valid game-over; count it as a pass.
            $status = 'EXHAUSTED'
        } elseif ($logContent -match 'turn cap reached') {
            $status = 'STALEMATE'
        } else {
            $status = if ($exitCode -eq 0) { 'PASS' } else { 'FAIL' }
        }
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

$pass      = ($summary | Where-Object { $_.Status -in 'PASS','EXHAUSTED' }).Count
$fail      = ($summary | Where-Object Status -eq 'FAIL').Count
$timeout   = ($summary | Where-Object Status -eq 'TIMEOUT').Count
$stalemate = ($summary | Where-Object Status -eq 'STALEMATE').Count
$overflow  = ($summary | Where-Object Status -eq 'STDERR_OVERFLOW').Count
Write-Host "Done: $pass passed (incl. exhausted), $fail failed, $timeout timed out, $stalemate stalemates, $overflow stderr-overflows"
Write-Host "Results: $batchDir"
