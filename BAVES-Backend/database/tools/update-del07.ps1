$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot "..\doc-templates"
$skel = [System.IO.File]::ReadAllText((Join-Path $root "del-07-skeleton.html"), [System.Text.UTF8Encoding]::new($false))
$sample = [System.IO.File]::ReadAllText((Join-Path $root "del-07-sample.html"), [System.Text.UTF8Encoding]::new($false))

$checklist = @(
  @{ id = "del07-c0"; title = "0. Document Governance"; description = "Author, technical approver, status, document ID."; rubricHint = "Technical approver must be named."; snippetHtml = "<h2>0. Document Governance</h2>" }
  @{ id = "del07-c1"; title = "1. Scope & Trace Context"; description = "In/out of scope and source BRD/process IDs."; rubricHint = "Must reference BRD requirement IDs."; snippetHtml = "<h2>1. Scope &amp; Trace Context</h2>" }
  @{ id = "del07-c2"; title = "2. Actors & Systems"; description = "Human and system actors."; rubricHint = "Include both users and systems."; snippetHtml = "<h2>2. Actors &amp; Systems</h2>" }
  @{ id = "del07-c3"; title = "3. User Stories Backlog"; description = "Stories with priority, BRD link, estimate."; rubricHint = "At least 3 stories including MUST items."; snippetHtml = "<h2>3. Epics &amp; User Stories Backlog</h2>" }
  @{ id = "del07-c4"; title = "4. Acceptance Criteria"; description = "Given/When/Then for happy and exception paths."; rubricHint = "Each MUST story needs measurable AC."; snippetHtml = "<h2>4. Acceptance Criteria (Given / When / Then)</h2>" }
  @{ id = "del07-c5"; title = "5. Functional Requirements"; description = "Detailed FR statements linked to stories."; rubricHint = "FRs must be testable."; snippetHtml = "<h2>5. Functional Requirements (Detailed)</h2>" }
  @{ id = "del07-c6"; title = "6. API Contracts"; description = "Method, endpoint, payload, story link."; rubricHint = "At least 2 endpoints for core capability."; snippetHtml = "<h2>6. API / Interface Contracts</h2>" }
  @{ id = "del07-c7"; title = "7. Non-Functional Requirements"; description = "Performance, availability, security, reliability."; rubricHint = "Include measurable targets."; snippetHtml = "<h2>7. Non-Functional Requirements</h2>" }
  @{ id = "del07-c8"; title = "8. Data Validation"; description = "Field rules and error behavior."; rubricHint = "Critical fields need validation rules."; snippetHtml = "<h2>8. Data Rules &amp; Validation</h2>" }
)
$checklistJson = ($checklist | ConvertTo-Json -Compress -Depth 5)

$conn = New-Object System.Data.SqlClient.SqlConnection 'Server=TRANVANAN\SQLEXPRESS01;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True;'
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
UPDATE artifact.ArtifactTemplates
SET Name=N'Requirements Specification',
    Description=N'Scope, actors, user stories, acceptance criteria, FRs, API contracts, NFRs, and validation rules.',
    DefaultTitle=N'SRS-2026: Requirements Specification',
    SkeletonHtmlContent=@skel, SampleHtmlContent=@sample, ChecklistJson=@check,
    UpdatedAt=SYSDATETIMEOFFSET()
WHERE Code=N'DEL-07';
"@
[void]$cmd.Parameters.AddWithValue("@skel", $skel)
[void]$cmd.Parameters.AddWithValue("@sample", $sample)
[void]$cmd.Parameters.AddWithValue("@check", $checklistJson)
Write-Host ("Updated DEL-07 rows={0} skel={1} sample={2}" -f $cmd.ExecuteNonQuery(), $skel.Length, $sample.Length)

$tid = [int](New-Object System.Data.SqlClient.SqlCommand("SELECT Id FROM artifact.ArtifactTemplates WHERE Code=N'DEL-07'", $conn)).ExecuteScalar()
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
Write-Host "DEL-07 sections=$($checklist.Count)"
