$path = Join-Path $PSScriptRoot 'fix-beginner-scenario-titles.sql'
$sql = Get-Content -Path $path -Raw -Encoding UTF8
$cs = 'Server=localhost;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True'
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = $sql.Replace("GO`r`n", "`n").Replace("GO`n", "`n")
[void]$cmd.ExecuteNonQuery()
$cmd.CommandText = "SELECT Code, Title FROM scenario.Scenarios WHERE Code LIKE 'BEG-%' ORDER BY Code"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
  Write-Output ($reader.GetString(0) + ' | ' + $reader.GetString(1))
}
$reader.Close()
$conn.Close()
