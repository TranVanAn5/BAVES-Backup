# Fix mojibake in artifact.ArtifactTemplates by reloading UTF-8 JSON seed content.
$ErrorActionPreference = "Stop"
$jsonPath = Join-Path $PSScriptRoot "..\doc-templates\del-01-08.json"
$rows = Get-Content -Path $jsonPath -Encoding UTF8 -Raw | ConvertFrom-Json

function Sanitize([string]$html) {
  if ($null -eq $html) { return $html }
  $h = $html
  # Common UTF-8 misread as Windows-1252
  $h = $h -replace [string]([char]0x2022), '&bull;'   # •
  $h = $h -replace 'â€¢', '&bull;'
  $h = $h -replace 'â€“', '&ndash;'
  $h = $h -replace 'â€”', '&mdash;'
  $h = $h -replace 'â€™', '&#39;'
  $h = $h -replace 'â€œ', '&quot;'
  $h = $h -replace 'â€', '&quot;'
  $h = $h -replace 'â€˜', '&#39;'
  $h = $h -replace 'Â ', '&nbsp;'
  $h = $h -replace 'Â', ''
  # Trailing " - " left after stripping BABOK from titles
  $h = $h -replace '\s+-\s*</h1>', '</h1>'
  $h = $h -replace '\s+-\s*$', ''
  # Prefer HTML entities for bullets going forward
  $h = $h.Replace([string][char]0x2022, '&bull;')
  return $h
}

$connStr = "Server=TRANVANAN\SQLEXPRESS01;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()

$updated = 0
foreach ($row in $rows) {
  $skel = Sanitize ([string]$row.skeletonHtmlContent)
  $sample = Sanitize ([string]$row.sampleHtmlContent)
  $checklist = Sanitize ([string]($row.checklist | ConvertTo-Json -Compress -Depth 8))
  $name = Sanitize ([string]$row.name)
  $desc = Sanitize ([string]$row.description)
  $title = Sanitize ([string]$row.defaultTitle)

  $cmd = $conn.CreateCommand()
  $cmd.CommandText = @"
UPDATE [artifact].[ArtifactTemplates]
SET [Name] = @name,
    [Description] = @desc,
    [DefaultTitle] = @title,
    [SkeletonHtmlContent] = @skel,
    [SampleHtmlContent] = @sample,
    [ChecklistJson] = @checklist,
    [UpdatedAt] = SYSDATETIMEOFFSET()
WHERE [Code] = @code;
"@
  [void]$cmd.Parameters.AddWithValue("@code", [string]$row.code)
  [void]$cmd.Parameters.AddWithValue("@name", $name)
  [void]$cmd.Parameters.AddWithValue("@desc", $(if ($desc) { $desc } else { [DBNull]::Value }))
  [void]$cmd.Parameters.AddWithValue("@title", $(if ($title) { $title } else { [DBNull]::Value }))
  [void]$cmd.Parameters.AddWithValue("@skel", $skel)
  [void]$cmd.Parameters.AddWithValue("@sample", $sample)
  [void]$cmd.Parameters.AddWithValue("@checklist", $checklist)
  $n = $cmd.ExecuteNonQuery()
  $updated += $n
  Write-Host ("Updated {0} rows={1} skelLen={2}" -f $row.code, $n, $skel.Length)
}

# Sweep any residual mojibake if JSON still had issues
$sweep = $conn.CreateCommand()
$sweep.CommandText = @"
UPDATE [artifact].[ArtifactTemplates]
SET
  [SkeletonHtmlContent] = REPLACE(REPLACE(REPLACE(REPLACE([SkeletonHtmlContent], N'â€¢', N'&bull;'), N'â€“', N'&ndash;'), N'â€”', N'&mdash;'), N'â€™', N'&#39;'),
  [SampleHtmlContent] = REPLACE(REPLACE(REPLACE(REPLACE([SampleHtmlContent], N'â€¢', N'&bull;'), N'â€“', N'&ndash;'), N'â€”', N'&mdash;'), N'â€™', N'&#39;'),
  [ChecklistJson] = REPLACE(REPLACE([ChecklistJson], N'â€¢', N'&bull;'), N'â€™', N'&#39;');
UPDATE [artifact].[ArtifactTemplates]
SET
  [SkeletonHtmlContent] = REPLACE([SkeletonHtmlContent], N' - </h1>', N'</h1>'),
  [SampleHtmlContent] = REPLACE([SampleHtmlContent], N' - </h1>', N'</h1>');
"@
[void]$sweep.ExecuteNonQuery()

$check = $conn.CreateCommand()
$check.CommandText = @"
SELECT [Code],
  CASE WHEN [SkeletonHtmlContent] LIKE N'%â€¢%' OR [SampleHtmlContent] LIKE N'%â€¢%' THEN 1 ELSE 0 END AS Mojibake,
  CASE WHEN [SkeletonHtmlContent] LIKE N'%&bull;%' THEN 1 ELSE 0 END AS HasBullEntity
FROM [artifact].[ArtifactTemplates]
ORDER BY [DisplayOrder];
"@
$reader = $check.ExecuteReader()
while ($reader.Read()) {
  Write-Host ("{0} mojibake={1} bull={2}" -f $reader["Code"], $reader["Mojibake"], $reader["HasBullEntity"])
}
$reader.Close()
$conn.Close()
Write-Host "Done. Updated=$updated"
