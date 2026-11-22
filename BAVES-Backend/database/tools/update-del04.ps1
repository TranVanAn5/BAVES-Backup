$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot "..\doc-templates"
$skel = [System.IO.File]::ReadAllText((Join-Path $root "del-04-skeleton.html"), [System.Text.UTF8Encoding]::new($false))
$sample = [System.IO.File]::ReadAllText((Join-Path $root "del-04-sample.html"), [System.Text.UTF8Encoding]::new($false))

$checklist = @(
  @{ id = "del04-c0"; title = "0. Session Governance"; description = "Date, duration, facilitator, channel, document ID."; rubricHint = "Must include date/time and facilitator."; snippetHtml = "<h2>0. Session Governance</h2>" }
  @{ id = "del04-c1"; title = "1. Participants"; description = "Name, role, unit, attendance."; rubricHint = "At least 2 stakeholders besides BA."; snippetHtml = "<h2>1. Participants</h2>" }
  @{ id = "del04-c2"; title = "2. Session Objectives"; description = "Clear elicitation goals for the session."; rubricHint = "Objectives must be testable / specific."; snippetHtml = "<h2>2. Session Objectives</h2>" }
  @{ id = "del04-c3"; title = "3. Techniques Used"; description = "Technique, rationale, output."; rubricHint = "Technique must match the information need."; snippetHtml = "<h2>3. Elicitation Techniques Used</h2>" }
  @{ id = "del04-c4"; title = "4. Interview Log"; description = "Q&A dialogue with roles labeled."; rubricHint = "Include at least 2 stakeholder voices."; snippetHtml = "<h2>4. Interview Log (Q&amp;A)</h2>" }
  @{ id = "del04-c5"; title = "5. Confirmed Findings"; description = "Finding ID, fact, source, impacted deliverable."; rubricHint = "At least 3 findings."; snippetHtml = "<h2>5. Confirmed Findings</h2>" }
  @{ id = "del04-c6"; title = "6. Evidence"; description = "Evidence ID linked to findings."; rubricHint = "Every critical finding needs evidence."; snippetHtml = "<h2>6. Evidence Captured</h2>" }
  @{ id = "del04-c7"; title = "7. Open Questions & Assumptions"; description = "Unresolved items with owners and due dates."; rubricHint = "Distinguish open questions vs assumptions."; snippetHtml = "<h2>7. Open Questions &amp; Assumptions</h2>" }
  @{ id = "del04-c8"; title = "8. Action Items"; description = "Action, owner, due date, related finding."; rubricHint = "At least 2 concrete next steps."; snippetHtml = "<h2>8. Action Items</h2>" }
)
$checklistJson = ($checklist | ConvertTo-Json -Compress -Depth 5)

$conn = New-Object System.Data.SqlClient.SqlConnection 'Server=TRANVANAN\SQLEXPRESS01;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True;'
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
UPDATE artifact.ArtifactTemplates
SET Name=N'Elicitation Results',
    Description=N'Session objectives, techniques, interview log, findings, evidence, open questions, and action items.',
    DefaultTitle=N'ELC-2026: Elicitation Results',
    SkeletonHtmlContent=@skel,
    SampleHtmlContent=@sample,
    ChecklistJson=@check,
    UpdatedAt=SYSDATETIMEOFFSET()
WHERE Code=N'DEL-04';
"@
[void]$cmd.Parameters.AddWithValue("@skel", $skel)
[void]$cmd.Parameters.AddWithValue("@sample", $sample)
[void]$cmd.Parameters.AddWithValue("@check", $checklistJson)
$n = $cmd.ExecuteNonQuery()
Write-Host "Updated DEL-04 rows=$n skelLen=$($skel.Length) sampleLen=$($sample.Length)"

$getId = $conn.CreateCommand()
$getId.CommandText = "SELECT Id FROM artifact.ArtifactTemplates WHERE Code=N'DEL-04'"
$tid = [int]$getId.ExecuteScalar()

$delSec = $conn.CreateCommand()
$delSec.CommandText = "DELETE FROM artifact.ArtifactTemplateSections WHERE ArtifactTemplateId=@id"
[void]$delSec.Parameters.AddWithValue("@id", $tid)
[void]$delSec.ExecuteNonQuery()

$order = 1
foreach ($item in $checklist) {
  $ins = $conn.CreateCommand()
  $ins.CommandText = @"
INSERT INTO artifact.ArtifactTemplateSections
(ArtifactTemplateId, SectionKey, Title, Instructions, DisplayOrder, IsRequired)
VALUES (@id, @key, @title, @inst, @ord, 1)
"@
  [void]$ins.Parameters.AddWithValue("@id", $tid)
  [void]$ins.Parameters.AddWithValue("@key", ($item.id.ToUpper() -replace '-', '_'))
  [void]$ins.Parameters.AddWithValue("@title", $item.title)
  [void]$ins.Parameters.AddWithValue("@inst", $item.description)
  [void]$ins.Parameters.AddWithValue("@ord", $order)
  [void]$ins.ExecuteNonQuery()
  $order++
}

$verify = $conn.CreateCommand()
$verify.CommandText = "SELECT Code, Name, LEN(SkeletonHtmlContent) Skel, LEN(SampleHtmlContent) Sample FROM artifact.ArtifactTemplates WHERE Code=N'DEL-04'"
$a = New-Object System.Data.SqlClient.SqlDataAdapter $verify
$t = New-Object System.Data.DataTable
[void]$a.Fill($t)
$conn.Close()
$t | Format-Table -AutoSize
Write-Host "Sections inserted: $($checklist.Count)"
