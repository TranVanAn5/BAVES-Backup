$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot "..\doc-templates"
$skel = [System.IO.File]::ReadAllText((Join-Path $root "del-08-skeleton.html"), [System.Text.UTF8Encoding]::new($false))
$sample = [System.IO.File]::ReadAllText((Join-Path $root "del-08-sample.html"), [System.Text.UTF8Encoding]::new($false))

$checklist = @(
  @{ id = "del08-c0"; title = "0. Document Governance"; description = "Owner, approver, baseline, status."; rubricHint = "Approver must be named."; snippetHtml = "<h2>0. Document Governance</h2>" }
  @{ id = "del08-c1"; title = "1. Traceability Scope"; description = "Covered deliverables and baseline versions."; rubricHint = "Baseline versions must be explicit."; snippetHtml = "<h2>1. Traceability Scope</h2>" }
  @{ id = "del08-c2"; title = "2. Bidirectional Matrix"; description = "Full chain Objective to Test with status."; rubricHint = "At least 3 complete rows."; snippetHtml = "<h2>2. Bidirectional Traceability Matrix</h2>" }
  @{ id = "del08-c3"; title = "3. Forward Trace"; description = "Need to test coverage summary."; rubricHint = "Every MUST BRD assessed."; snippetHtml = "<h2>3. Forward Trace Summary (Need &rarr; Test)</h2>" }
  @{ id = "del08-c4"; title = "4. Backward Trace"; description = "Test/story back to objective/evidence."; rubricHint = "No unjustified tests in release scope."; snippetHtml = "<h2>4. Backward Trace Summary (Test &rarr; Need)</h2>" }
  @{ id = "del08-c5"; title = "5. Coverage Metrics"; description = "Counts and percentages vs targets."; rubricHint = "MUST test coverage target 100%."; snippetHtml = "<h2>5. Coverage Metrics</h2>" }
  @{ id = "del08-c6"; title = "6. Gaps & Orphans"; description = "Gap register with owner and due date."; rubricHint = "Open gaps must have owners."; snippetHtml = "<h2>6. Gaps &amp; Orphan Register</h2>" }
  @{ id = "del08-c7"; title = "7. Change Impact"; description = "Record requirement changes and impacted links."; rubricHint = "Material changes logged."; snippetHtml = "<h2>7. Change Impact Log</h2>" }
  @{ id = "del08-c8"; title = "8. Sign-off Checklist"; description = "Release readiness checks."; rubricHint = "Sign-off only when orphans are closed."; snippetHtml = "<h2>8. Sign-off Checklist</h2>" }
)
$checklistJson = ($checklist | ConvertTo-Json -Compress -Depth 5)

$conn = New-Object System.Data.SqlClient.SqlConnection 'Server=TRANVANAN\SQLEXPRESS01;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True;'
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
UPDATE artifact.ArtifactTemplates
SET Name=N'Traceability Matrix',
    Description=N'Bidirectional RTM with coverage metrics, orphan gaps, change impact, and sign-off checks.',
    DefaultTitle=N'RTM-2026: Traceability Matrix',
    SkeletonHtmlContent=@skel, SampleHtmlContent=@sample, ChecklistJson=@check,
    UpdatedAt=SYSDATETIMEOFFSET()
WHERE Code=N'DEL-08';
"@
[void]$cmd.Parameters.AddWithValue("@skel", $skel)
[void]$cmd.Parameters.AddWithValue("@sample", $sample)
[void]$cmd.Parameters.AddWithValue("@check", $checklistJson)
Write-Host ("Updated DEL-08 rows={0} skel={1} sample={2}" -f $cmd.ExecuteNonQuery(), $skel.Length, $sample.Length)

$tid = [int](New-Object System.Data.SqlClient.SqlCommand("SELECT Id FROM artifact.ArtifactTemplates WHERE Code=N'DEL-08'", $conn)).ExecuteScalar()
$del = $conn.CreateCommand(); $del.CommandText = "DELETE FROM artifact.ArtifactTemplateSections WHERE ArtifactTemplateId=@id"; [void]$del.Parameters.AddWithValue("@id", $tid); [void]$del.ExecuteNonQuery()
$order = 1
foreach ($item in $checklist) {
  $ins = $conn.CreateCommand()
  $ins.CommandText = "INSERT INTO artifact.ArtifactTemplateSections (ArtifactTemplateId, SectionKey, Title, Instructions, DisplayOrder, IsRequired) VALUES (@id,@key,@title,@inst,@ord,1)"
  [void]$ins.Parameters.AddWithValue("@id", $tid)
  [void]$ins.Parameters.AddWithValue("@key", ($item.id.ToUpper() -replace '-', '_'))
  [void]$ins.Parameters.AddWithValue("@title", $item.title)
  [void]$ins.Parameters.AddWithValue("@inst", $item.description)
  [void]$ins.Parameters.AddWithValue("@ord", $order)
  [void]$ins.ExecuteNonQuery(); $order++
}
$conn.Close()
Write-Host "DEL-08 sections=$($checklist.Count)"
