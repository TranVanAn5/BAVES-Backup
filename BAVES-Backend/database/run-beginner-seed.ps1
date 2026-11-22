# Runs seed-beginner-scenarios-x4.sql with UTF-8 via SqlClient (avoids sqlcmd mojibake)
$ErrorActionPreference = 'Stop'
$path = Join-Path $PSScriptRoot 'seed-beginner-scenarios-x4.sql'
$raw = Get-Content -Path $path -Raw -Encoding UTF8
$cs = 'Server=localhost;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True'

# Drop previous beginner rows so re-insert gets correct Unicode
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cleanup = @"
DELETE ska FROM scenario.StakeholderKnowledgeAccess ska
INNER JOIN scenario.StakeholderPersonas p ON p.Id = ska.StakeholderPersonaId
INNER JOIN scenario.ScenarioVersions v ON v.Id = p.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId
WHERE s.Code LIKE N'BEG-%';

DELETE k FROM scenario.KnowledgeItems k
INNER JOIN scenario.ScenarioVersions v ON v.Id = k.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId
WHERE s.Code LIKE N'BEG-%';

DELETE p FROM scenario.StakeholderPersonas p
INNER JOIN scenario.ScenarioVersions v ON v.Id = p.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId
WHERE s.Code LIKE N'BEG-%';

DELETE v FROM scenario.ScenarioVersions v
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId
WHERE s.Code LIKE N'BEG-%';

DELETE FROM scenario.Scenarios WHERE Code LIKE N'BEG-%';
"@
$cmd = $conn.CreateCommand()
$cmd.CommandText = $cleanup
[void]$cmd.ExecuteNonQuery()
Write-Output '>> Cleaned old BEG-* rows'

# Split batches on GO
$batches = [regex]::Split($raw, '(?m)^\s*GO\s*$')
foreach ($batch in $batches) {
  $sql = $batch.Trim()
  if (-not $sql) { continue }
  $cmd.CommandText = $sql
  try {
    [void]$cmd.ExecuteNonQuery()
  } catch {
    Write-Warning $_.Exception.Message
    throw
  }
}

$cmd.CommandText = @"
SELECT s.Code, s.Title, a.Code AS App, v.Difficulty, v.IsFreeAccess
FROM scenario.Scenarios s
JOIN scenario.ScenarioVersions v ON v.ScenarioId = s.Id AND v.VersionNo = 1
JOIN content.Applications a ON a.Id = s.ApplicationId
WHERE s.Code LIKE N'BEG-%'
ORDER BY s.Code
"@
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
  Write-Output ("{0} | {1} | {2} | {3} | free={4}" -f $reader['Code'], $reader['Title'], $reader['App'], $reader['Difficulty'], $reader['IsFreeAccess'])
}
$reader.Close()
$conn.Close()
Write-Output '>> Beginner seed complete'
