param(
  [string]$Editor = "D:\Unity\UnityEditors\6000.6.0f1\Editor\Unity.exe",
  [switch]$PreflightOnly,
  [switch]$KeepProject,
  [int]$TimeoutMinutes = 45,
  [string]$PackageRoot = ""
)
$ErrorActionPreference = "Stop"
$repo = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
if (-not (Test-Path $Editor -PathType Leaf)) { throw "Unity editor not found: $Editor" }
if (-not $PackageRoot) { $PackageRoot = $repo }
$PackageRoot = (Resolve-Path $PackageRoot).Path
if (-not (Test-Path (Join-Path $PackageRoot "package.json"))) { throw "Missing WiRR package.json: $PackageRoot" }
if (-not (Get-Command py -ErrorAction SilentlyContinue)) { throw "Python launcher py is required" }
if ($TimeoutMinutes -lt 1) { throw "TimeoutMinutes must be positive" }
$matchVersion = [regex]::Match($Editor,'6000\.\d+\.\d+f\d+')
if (-not $matchVersion.Success) { throw "Editor version not found in path" }
$version = $matchVersion.Value
Write-Host "Unity Windows: $Editor; version: $version; package: $PackageRoot"
if ($PreflightOnly) { Write-Host "Preflight PASS; no tests executed."; exit 0 }
$project = Join-Path "D:\Unity\cli" ("wirr-native-" + [guid]::NewGuid().ToString("N"))
$results = Join-Path $repo "artifacts\unity-native"
New-Item -ItemType Directory -Force -Path $results,$project | Out-Null
try {
  & py -3 (Join-Path $repo "tools/unity-ci/prepare_project.py") --package $PackageRoot --project $project --unity $version
  if ($LASTEXITCODE -ne 0) { throw "Unity project preparation failed" }
  $summary = @()
  $commit = (& git -C $PackageRoot rev-parse HEAD 2>$null | Out-String).Trim()
  $passedAll = $false
  Write-Host "Project for diagnostics: $project"
  foreach ($mode in @("EditMode","PlayMode")) {
    $xml = Join-Path $results "$mode.xml"
    $log = Join-Path $results "$mode.log"
    Remove-Item -LiteralPath $xml,$log -ErrorAction SilentlyContinue
    Write-Host "Unity native $mode..."
    # Quote path-valued arguments explicitly: Start-Process joins ArgumentList into a command line.
    $quotedArgs = @("-batchmode","-nographics","-projectPath",('"' + $project + '"'),
      "-runTests","-testPlatform",$mode,"-testResults",('"' + $xml + '"'),
      "-logFile",('"' + $log + '"'))
    $process = Start-Process -FilePath $Editor -ArgumentList $quotedArgs -PassThru -NoNewWindow -ErrorAction Stop
    $finished = $process.WaitForExit($TimeoutMinutes * 60000)
    if (-not $finished) {
      Write-Warning "Unity $mode timed out after $TimeoutMinutes minutes"
      Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
      $exitCode = -1
    } else {
      $process.WaitForExit()
      $process.Refresh()
      $exitCode = $process.ExitCode
      if ($null -eq $exitCode) { $exitCode = -2; Write-Warning "Unity process exit code unavailable for $mode" }
    }
    $total = 0
    $failed = -1
    $result = "Missing"
    if (Test-Path $xml) {
      try {
        [xml]$doc = Get-Content -LiteralPath $xml -Raw
        $total = [int]$doc.DocumentElement.GetAttribute("total")
        $failed = [int]$doc.DocumentElement.GetAttribute("failed")
        $result = $doc.DocumentElement.GetAttribute("result")
      } catch { Write-Warning "Invalid NUnit XML for $mode" }
    }
    if (-not (Test-Path $xml)) {
      Write-Warning "Missing NUnit results for $mode. Unity log: $log"
      if (Test-Path $log) { Get-Content -LiteralPath $log -Tail 60 }
    }
    $passed = ($exitCode -eq 0 -and $total -gt 0 -and $failed -eq 0 -and $result -notin @("Failed","Inconclusive","Missing"))
    $summary += [pscustomobject]@{ Mode=$mode; ExitCode=$exitCode; Total=$total; Failed=$failed; Result=$result; Pass=$passed }
    if (-not $passed) { Write-Warning "$mode failed; skipping remaining suites to preserve primary error"; break }
  }
  $summary | Format-Table -AutoSize
  [pscustomobject]@{ Unity=$version; Commit=$commit; Project=$project; Suites=$summary } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $results "summary.json") -Encoding UTF8
  if ($summary | Where-Object { -not $_.Pass }) { throw "Unity tests failed. Inspect $results" }
  $passedAll = $true
  Write-Host "PASS: Unity EditMode + PlayMode"
} finally {
  if ($passedAll -and -not $KeepProject) {
    Remove-Item -LiteralPath $project -Recurse -Force -ErrorAction SilentlyContinue
  } else {
    Write-Host "Project retained for diagnostics: $project"
  }
}
