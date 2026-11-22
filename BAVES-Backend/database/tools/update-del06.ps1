$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot "..\doc-templates"
$skel = [System.IO.File]::ReadAllText((Join-Path $root "del-06-skeleton.html"), [System.Text.UTF8Encoding]::new($false))
$sample = [System.IO.File]::ReadAllText((Join-Path $root "del-06-sample.html"), [System.Text.UTF8Encoding]::new($false))

$checklist = @(
  @{ id = "del06-c0"; title = "0. Document Governance"; description = "Process name, owner, author, status."; rubricHint = "Process owner must be a business authority."; snippetHtml = "<h2>0. Document Governance</h2>" }
  @{ id = "del06-c1"; title = "1. Purpose & Scope"; description = "Purpose, trigger, outcome, in/out of scope."; rubricHint = "Trigger and primary outcome must be explicit."; snippetHtml = "<h2>1. Process Purpose &amp; Scope</h2>" }
  @{ id = "del06-c2"; title = "2. Swimlanes"; description = "Actors/systems and responsibilities."; rubricHint = "At least 3 lanes with clear responsibilities."; snippetHtml = "<h2>2. Swimlanes (Participants)</h2>" }
  @{ id = "del06-c3"; title = "3. Happy Path"; description = "Ordered steps with actor, output, SLA."; rubricHint = "At least 5 steps with timing where critical."; snippetHtml = "<h2>3. Happy Path (Step-by-Step)</h2>" }
  @{ id = "del06-c4"; title = "4. Decision Gateways"; description = "Decision questions and next paths."; rubricHint = "Each gateway needs Yes/No style outcomes."; snippetHtml = "<h2>4. Decision Gateways</h2>" }
  @{ id = "del06-c5"; title = "5. Exceptions"; description = "Triggers, handling, end states."; rubricHint = "At least 2 exception paths."; snippetHtml = "<h2>5. Exception &amp; Alternate Paths</h2>" }
  @{ id = "del06-c6"; title = "6. Data Touchpoints"; description = "Systems and data in/out per step."; rubricHint = "Critical steps must name systems/events."; snippetHtml = "<h2>6. Data &amp; System Touchpoints</h2>" }
  @{ id = "del06-c7"; title = "7. SLA Metrics"; description = "Cycle time / latency targets."; rubricHint = "Include measurable targets."; snippetHtml = "<h2>7. SLA &amp; Performance Metrics</h2>" }
  @{ id = "del06-c8"; title = "8. Traceability"; description = "Link steps to BRD requirements and rules."; rubricHint = "At least 2 trace links."; snippetHtml = "<h2>8. Traceability to Requirements</h2>" }
)
$checklistJson = ($checklist | ConvertTo-Json -Compress -Depth 5)

$conn = New-Object System.Data.SqlClient.SqlConnection 'Server=TRANVANAN\SQLEXPRESS01;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True;'
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
UPDATE artifact.ArtifactTemplates
SET Name=N'Process Model',
    Description=N'Swimlanes, happy path, gateways, exceptions, data touchpoints, SLAs, and requirement traceability.',
    DefaultTitle=N'PROC-2026: Process Model',
    SkeletonHtmlContent=@skel, SampleHtmlContent=@sample, ChecklistJson=@check,
    UpdatedAt=SYSDATETIMEOFFSET()
WHERE Code=N'DEL-06';
"@
[void]$cmd.Parameters.AddWithValue("@skel", $skel)
[void]$cmd.Parameters.AddWithValue("@sample", $sample)
[void]$cmd.Parameters.AddWithValue("@check", $checklistJson)
Write-Host ("Updated DEL-06 rows={0} skel={1} sample={2}" -f $cmd.ExecuteNonQuery(), $skel.Length, $sample.Length)

$tid = [int](New-Object System.Data.SqlClient.SqlCommand("SELECT Id FROM artifact.ArtifactTemplates WHERE Code=N'DEL-06'", $conn)).ExecuteScalar()
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
Write-Host "DEL-06 sections=$($checklist.Count)"
