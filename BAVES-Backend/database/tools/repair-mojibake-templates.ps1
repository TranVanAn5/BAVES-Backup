# Reverse UTF-8-as-Windows-1252 mojibake in artifact template HTML fields.
$ErrorActionPreference = "Stop"

function Repair-Mojibake([string]$text) {
  if ([string]::IsNullOrEmpty($text)) { return $text }
  # Only attempt if classic markers exist
  if ($text -notmatch 'Ã.|Â.|â€|ðŸ|á»|áº') { return $text }
  try {
    $bytes = [System.Text.Encoding]::GetEncoding(1252).GetBytes($text)
    $fixed = [System.Text.Encoding]::UTF8.GetString($bytes)
    # Sanity: fixed should contain Vietnamese or clean ASCII and fewer mojibake markers
    $badBefore = ([regex]::Matches($text, 'Ã.|â€|ðŸ')).Count
    $badAfter = ([regex]::Matches($fixed, 'Ã.|â€|ðŸ')).Count
    if ($badAfter -lt $badBefore) { return $fixed }
    return $text
  } catch {
    return $text
  }
}

function Sanitize([string]$html) {
  if ($null -eq $html) { return $html }
  $h = Repair-Mojibake $html
  $h = $h -replace 'â€¢', '&bull;'
  $h = $h -replace [string]([char]0x2022), '&bull;'
  $h = $h -replace 'â€“', '&ndash;'
  $h = $h -replace 'â€”', '&mdash;'
  $h = $h -replace 'â€™', '&#39;'
  $h = $h -replace 'â€œ', '&quot;'
  $h = $h -replace 'â€', '&quot;'
  $h = $h -replace '\s+-\s*</h1>', '</h1>'
  return $h
}

$conn = New-Object System.Data.SqlClient.SqlConnection 'Server=TRANVANAN\SQLEXPRESS01;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True;'
$conn.Open()

$select = $conn.CreateCommand()
$select.CommandText = "SELECT Id, Code, Name, Description, DefaultTitle, SkeletonHtmlContent, SampleHtmlContent, ChecklistJson FROM artifact.ArtifactTemplates"
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter $select
$table = New-Object System.Data.DataTable
[void]$adapter.Fill($table)

foreach ($row in $table.Rows) {
  $name = Sanitize ([string]$row.Name)
  $desc = if ($row.Description -eq [DBNull]::Value) { $null } else { Sanitize ([string]$row.Description) }
  $title = if ($row.DefaultTitle -eq [DBNull]::Value) { $null } else { Sanitize ([string]$row.DefaultTitle) }
  $skel = Sanitize ([string]$row.SkeletonHtmlContent)
  $sample = Sanitize ([string]$row.SampleHtmlContent)
  $check = if ($row.ChecklistJson -eq [DBNull]::Value) { $null } else { Sanitize ([string]$row.ChecklistJson) }

  $upd = $conn.CreateCommand()
  $upd.CommandText = @"
UPDATE artifact.ArtifactTemplates
SET Name=@name, Description=@desc, DefaultTitle=@title,
    SkeletonHtmlContent=@skel, SampleHtmlContent=@sample, ChecklistJson=@check,
    UpdatedAt=SYSDATETIMEOFFSET()
WHERE Id=@id
"@
  [void]$upd.Parameters.AddWithValue("@id", [int]$row.Id)
  [void]$upd.Parameters.AddWithValue("@name", $name)
  [void]$upd.Parameters.AddWithValue("@desc", $(if ($null -eq $desc) { [DBNull]::Value } else { $desc }))
  [void]$upd.Parameters.AddWithValue("@title", $(if ($null -eq $title) { [DBNull]::Value } else { $title }))
  [void]$upd.Parameters.AddWithValue("@skel", $skel)
  [void]$upd.Parameters.AddWithValue("@sample", $sample)
  [void]$upd.Parameters.AddWithValue("@check", $(if ($null -eq $check) { [DBNull]::Value } else { $check }))
  [void]$upd.ExecuteNonQuery()

  $preview = $skel.Substring(0, [Math]::Min(70, $skel.Length))
  Write-Host ("{0}: {1}" -f $row.Code, $preview)
}

# Export JSON backup
$outDir = Join-Path $PSScriptRoot "..\doc-templates"
$export = $conn.CreateCommand()
$export.CommandText = @"
SELECT Code, DeliverableCode, Name, ArtifactType, Description, Category, DefaultTitle,
       IssueKeyPrefix, DisplayOrder, SkeletonHtmlContent, SampleHtmlContent, ChecklistJson
FROM artifact.ArtifactTemplates ORDER BY DisplayOrder
"@
$ea = New-Object System.Data.SqlClient.SqlDataAdapter $export
$et = New-Object System.Data.DataTable
[void]$ea.Fill($et)
$conn.Close()

$list = @()
foreach ($r in $et.Rows) {
  $checklist = @()
  if ($r.ChecklistJson -ne [DBNull]::Value -and $r.ChecklistJson) {
    try { $checklist = $r.ChecklistJson | ConvertFrom-Json } catch { $checklist = @() }
  }
  $list += [ordered]@{
    code = [string]$r.Code
    deliverableCode = [string]$r.DeliverableCode
    name = [string]$r.Name
    artifactType = [string]$r.ArtifactType
    description = if ($r.Description -eq [DBNull]::Value) { $null } else { [string]$r.Description }
    category = if ($r.Category -eq [DBNull]::Value) { $null } else { [string]$r.Category }
    defaultTitle = if ($r.DefaultTitle -eq [DBNull]::Value) { $null } else { [string]$r.DefaultTitle }
    issueKeyPrefix = if ($r.IssueKeyPrefix -eq [DBNull]::Value) { $null } else { [string]$r.IssueKeyPrefix }
    displayOrder = [int]$r.DisplayOrder
    skeletonHtmlContent = [string]$r.SkeletonHtmlContent
    sampleHtmlContent = [string]$r.SampleHtmlContent
    checklist = $checklist
  }
}
$jsonPath = Join-Path $outDir "del-01-08.json"
[System.IO.File]::WriteAllText((Resolve-Path $outDir).Path + "\del-01-08.json", ($list | ConvertTo-Json -Depth 12), [System.Text.UTF8Encoding]::new($false))
Write-Host "Wrote $($list.Count) templates to $jsonPath"
