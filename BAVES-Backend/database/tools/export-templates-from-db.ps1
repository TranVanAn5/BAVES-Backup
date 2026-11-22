# Rebuild del-01-08.json from current DB (UTF-8 safe).
$ErrorActionPreference = "Stop"
$outPath = Join-Path $PSScriptRoot "..\doc-templates\del-01-08.json"

$connStr = "Server=TRANVANAN\SQLEXPRESS01;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT [Code],[DeliverableCode],[Name],[ArtifactType],[Description],[Category],[DefaultTitle],
       [IssueKeyPrefix],[DisplayOrder],[SkeletonHtmlContent],[SampleHtmlContent],[ChecklistJson]
FROM [artifact].[ArtifactTemplates]
ORDER BY [DisplayOrder], [Code];
"@
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$table = New-Object System.Data.DataTable
[void]$adapter.Fill($table)
$conn.Close()

$rows = @()
foreach ($r in $table.Rows) {
  $checklist = @()
  if ($r.ChecklistJson -and $r.ChecklistJson -ne [DBNull]::Value) {
    try { $checklist = $r.ChecklistJson | ConvertFrom-Json } catch { $checklist = @() }
  }
  $rows += [ordered]@{
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

$json = $rows | ConvertTo-Json -Depth 10
[System.IO.File]::WriteAllText((Resolve-Path (Split-Path $outPath -Parent)).Path + "\del-01-08.json", $json, [System.Text.UTF8Encoding]::new($false))
Write-Host "Exported $($rows.Count) templates to del-01-08.json"
