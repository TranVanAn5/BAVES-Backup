# Seed BEG-CAFE-01 (BeanCounter Café) with UTF-8 via SqlClient
$ErrorActionPreference = 'Stop'
$path = Join-Path $PSScriptRoot 'seed-beancounter-cafe-beg-01.sql'
$raw = Get-Content -Path $path -Raw -Encoding UTF8
$cs = 'Server=localhost;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True'

$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandTimeout = 120
$cmd.CommandText = $raw
[void]$cmd.ExecuteNonQuery()

$cmd.CommandText = @"
SELECT s.Code, s.Title, a.Code AS App, v.Difficulty, v.IsFreeAccess,
       (SELECT COUNT(*) FROM scenario.StakeholderPersonas p WHERE p.ScenarioVersionId = v.Id) AS Personas,
       (SELECT COUNT(*) FROM scenario.KnowledgeItems k WHERE k.ScenarioVersionId = v.Id) AS Knowledge,
       (SELECT COUNT(*) FROM scenario.BusinessRules r WHERE r.ScenarioVersionId = v.Id) AS Rules,
       (SELECT COUNT(*) FROM scenario.ScenarioEvents e WHERE e.ScenarioVersionId = v.Id) AS Events
FROM scenario.Scenarios s
JOIN scenario.ScenarioVersions v ON v.ScenarioId = s.Id AND v.VersionNo = 1
JOIN content.Applications a ON a.Id = s.ApplicationId
WHERE s.Code = N'BEG-CAFE-01'
"@
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
  Write-Output ("{0} | {1} | {2} | {3} | free={4} | P={5} K={6} R={7} E={8}" -f `
    $reader['Code'], $reader['Title'], $reader['App'], $reader['Difficulty'], $reader['IsFreeAccess'], `
    $reader['Personas'], $reader['Knowledge'], $reader['Rules'], $reader['Events'])
}
$reader.Close()
$conn.Close()
Write-Output '>> BEG-CAFE-01 seed complete'
