[CmdletBinding()]
param(
    [string]$UnityExe = $env:UNITY_EDITOR_PATH,
    [string]$ProjectPath = 'Project~',
    [string]$OutputRoot = 'logs/unity-cli'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $UnityExe -or -not (Test-Path -LiteralPath $UnityExe -PathType Leaf)) {
    throw 'UNITY_EDITOR_PATH must point to an installed Unity 6000.6.5f1 Editor executable.'
}
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = (Resolve-Path (Join-Path $root $ProjectPath)).Path
$out = Join-Path $root $OutputRoot
New-Item -ItemType Directory -Path $out -Force | Out-Null
$sha = (& git -C $root rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Could not determine commit SHA' }
$results = @()
foreach ($platform in @('EditMode','PlayMode')) {
    $log = Join-Path $out "$platform-editor.log"
    $xml = Join-Path $out "$platform-results.xml"
    & $UnityExe -batchmode -nographics -quit -projectPath $project -runTests -testPlatform $platform -testResults $xml -logFile $log
    $exit = $LASTEXITCODE
    $results += [ordered]@{ platform = $platform; exit_code = $exit; xml_exists = (Test-Path $xml); log_exists = (Test-Path $log) }
}
$report = [ordered]@{ schema_version = 1; sha = $sha; date_utc = (Get-Date).ToUniversalTime().ToString('o'); unity_editor = $UnityExe; results = $results }
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $out 'summary.json') -Encoding UTF8
if (@($results | Where-Object { $_.exit_code -ne 0 -or -not $_.xml_exists }).Count -gt 0) {
    throw 'Unity CLI tests failed; see logs/unity-cli/'
}
Write-Host 'PASS: Unity EditMode and PlayMode (results in logs/unity-cli/)'
