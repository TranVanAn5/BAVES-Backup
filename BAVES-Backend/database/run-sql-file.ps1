param([Parameter(Mandatory=$true)][string]$SqlFile)
$ErrorActionPreference = 'Stop'
$cs = 'Server=localhost;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True'
$raw = Get-Content -Path $SqlFile -Raw -Encoding UTF8
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandTimeout = 120
foreach ($batch in [regex]::Split($raw, '(?im)^\s*GO\s*$')) {
  $sql = $batch.Trim()
  if (-not $sql) { continue }
  $cmd.CommandText = $sql
  $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
  $table = New-Object System.Data.DataTable
  try {
    [void]$adapter.Fill($table)
    if ($table.Rows.Count -gt 0) {
      $table | Format-Table -AutoSize | Out-String | Write-Output
    }
  } catch {
    [void]$cmd.ExecuteNonQuery()
  }
}
$conn.Close()
Write-Output (">> Applied " + $SqlFile)
