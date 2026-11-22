$ErrorActionPreference = 'Stop'
$cs = 'Server=localhost;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True'
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandTimeout = 120

$cmd.CommandText = @"
-- Dependent rows for BEG-* versions (order matters for FKs)
DELETE ska FROM scenario.StakeholderKnowledgeAccess ska
INNER JOIN scenario.StakeholderPersonas p ON p.Id = ska.StakeholderPersonaId
INNER JOIN scenario.ScenarioVersions v ON v.Id = p.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId WHERE s.Code LIKE N'BEG-%';

DELETE k FROM scenario.KnowledgeItems k
INNER JOIN scenario.ScenarioVersions v ON v.Id = k.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId WHERE s.Code LIKE N'BEG-%';

DELETE p FROM scenario.StakeholderPersonas p
INNER JOIN scenario.ScenarioVersions v ON v.Id = p.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId WHERE s.Code LIKE N'BEG-%';

DELETE br FROM scenario.BusinessRules br
INNER JOIN scenario.ScenarioVersions v ON v.Id = br.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId WHERE s.Code LIKE N'BEG-%';

DELETE se FROM scenario.ScenarioEvents se
INNER JOIN scenario.ScenarioVersions v ON v.Id = se.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId WHERE s.Code LIKE N'BEG-%';

DELETE rc FROM assessment.RubricCriteria rc
INNER JOIN assessment.Rubrics r ON r.Id = rc.RubricId
INNER JOIN scenario.ScenarioVersions v ON v.Id = r.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId WHERE s.Code LIKE N'BEG-%';

DELETE r FROM assessment.Rubrics r
INNER JOIN scenario.ScenarioVersions v ON v.Id = r.ScenarioVersionId
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId WHERE s.Code LIKE N'BEG-%';

DELETE v FROM scenario.ScenarioVersions v
INNER JOIN scenario.Scenarios s ON s.Id = v.ScenarioId WHERE s.Code LIKE N'BEG-%';

DELETE FROM scenario.Scenarios WHERE Code LIKE N'BEG-%';
"@
[void]$cmd.ExecuteNonQuery()
Write-Output '>> Cleaned BEG-* (personas/knowledge/rules/events/rubrics + versions)'

$files = @(
  (Join-Path $PSScriptRoot 'seed-omnimart-starter-from-okf.sql'),
  (Join-Path $PSScriptRoot 'seed-aquacold-starter-from-okf.sql'),
  (Join-Path $PSScriptRoot 'seed-finnova-starter-from-okf.sql'),
  (Join-Path $PSScriptRoot 'seed-megafashion-starter-from-okf.sql')
)

foreach ($f in $files) {
  Write-Output (">> Running " + (Split-Path $f -Leaf))
  $raw = Get-Content -Path $f -Raw -Encoding UTF8
  $batches = [regex]::Split($raw, '(?im)^\s*GO\s*$')
  foreach ($batch in $batches) {
    $sql = $batch.Trim()
    if (-not $sql) { continue }
    $cmd.CommandText = $sql
    [void]$cmd.ExecuteNonQuery()
  }
}

$cmd.CommandText = @"
SELECT
  s.Code,
  s.Title,
  a.Code AS App,
  v.Difficulty,
  v.IsFreeAccess,
  (SELECT COUNT(*) FROM scenario.StakeholderPersonas p WHERE p.ScenarioVersionId = v.Id) AS Personas,
  (SELECT COUNT(*) FROM scenario.KnowledgeItems k WHERE k.ScenarioVersionId = v.Id) AS Knowledge,
  (SELECT COUNT(*) FROM scenario.BusinessRules br WHERE br.ScenarioVersionId = v.Id) AS Rules,
  (SELECT COUNT(*) FROM scenario.ScenarioEvents e WHERE e.ScenarioVersionId = v.Id) AS Events,
  (SELECT COUNT(*) FROM assessment.Rubrics r WHERE r.ScenarioVersionId = v.Id) AS Rubrics,
  (SELECT COUNT(*) FROM assessment.RubricCriteria rc
     INNER JOIN assessment.Rubrics r ON r.Id = rc.RubricId
     WHERE r.ScenarioVersionId = v.Id) AS Criteria
FROM scenario.Scenarios s
JOIN scenario.ScenarioVersions v ON v.ScenarioId = s.Id AND v.VersionNo = 1
JOIN content.Applications a ON a.Id = s.ApplicationId
WHERE s.Code LIKE N'BEG-%'
ORDER BY s.Code
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) {
  Write-Output ("{0} | {1} | {2} | {3} | free={4} | P={5} K={6} R={7} E={8} Rub={9} Crit={10}" -f `
    $r['Code'], $r['Title'], $r['App'], $r['Difficulty'], $r['IsFreeAccess'], `
    $r['Personas'], $r['Knowledge'], $r['Rules'], $r['Events'], $r['Rubrics'], $r['Criteria'])
}
$r.Close()
$conn.Close()
Write-Output '>> OKF import done'
