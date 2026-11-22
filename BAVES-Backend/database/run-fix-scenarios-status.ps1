$ErrorActionPreference = 'Stop'
$cs = 'Server=localhost;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True'
$path = Join-Path $PSScriptRoot 'fix-scenarios-status-check.sql'
$sql = Get-Content -Path $path -Raw -Encoding UTF8
$c = New-Object System.Data.SqlClient.SqlConnection $cs
$c.Open()
$batches = [regex]::Split($sql, '(?m)^\s*GO\s*$')
foreach ($b in $batches) {
  $t = $b.Trim()
  if (-not $t) { continue }
  $cmd = $c.CreateCommand()
  $cmd.CommandText = $t
  [void]$cmd.ExecuteNonQuery()
}
$c.Close()
Write-Output '>> CK_Scenarios_Status patched'
