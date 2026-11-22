$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot "..\doc-templates"
$skel = [System.IO.File]::ReadAllText((Join-Path $root "del-05-skeleton.html"), [System.Text.UTF8Encoding]::new($false))
$sample = [System.IO.File]::ReadAllText((Join-Path $root "del-05-sample.html"), [System.Text.UTF8Encoding]::new($false))

$checklist = @(
  @{ id = "del05-c0"; title = "0. Document Governance"; description = "Owner, sponsor, version, status, document ID."; rubricHint = "Sponsor must be decision authority."; snippetHtml = "<h2>0. Document Governance</h2>" }
  @{ id = "del05-c1"; title = "1. Business Objectives"; description = "Measurable objectives with baseline/target/measure."; rubricHint = "At least 3 SMART objectives."; snippetHtml = "<h2>1. Business Objectives</h2>" }
  @{ id = "del05-c2"; title = "2. Scope"; description = "Clear in-scope and out-of-scope lists."; rubricHint = "Out-of-scope must prevent scope creep."; snippetHtml = "<h2>2. Scope</h2>" }
  @{ id = "del05-c3"; title = "3. As-Is vs To-Be"; description = "Side-by-side process contrast."; rubricHint = "To-Be must show the key business change."; snippetHtml = "<h2>3. As-Is vs To-Be</h2>" }
  @{ id = "del05-c4"; title = "4. Stakeholders & Needs"; description = "Needs linked to objectives and findings."; rubricHint = "Trace needs to elicitation findings."; snippetHtml = "<h2>4. Stakeholders &amp; Needs</h2>" }
  @{ id = "del05-c5"; title = "5. MoSCoW Requirements"; description = "Prioritized business requirements with acceptance signals."; rubricHint = "At least 3 MUST requirements."; snippetHtml = "<h2>5. Business Requirements (MoSCoW)</h2>" }
  @{ id = "del05-c6"; title = "6. Business Rules"; description = "IF/THEN policies linked to requirements."; rubricHint = "Every critical MUST has a rule where needed."; snippetHtml = "<h2>6. Business Rules</h2>" }
  @{ id = "del05-c7"; title = "7. Business Constraints"; description = "Performance, audit, continuity constraints."; rubricHint = "Include measurable constraints."; snippetHtml = "<h2>7. Non-Functional Business Constraints</h2>" }
  @{ id = "del05-c8"; title = "8. Assumptions / Dependencies / Risks"; description = "ADR items with impact and response."; rubricHint = "At least 1 of each type."; snippetHtml = "<h2>8. Assumptions, Dependencies &amp; Risks</h2>" }
)
$checklistJson = ($checklist | ConvertTo-Json -Compress -Depth 5)

$conn = New-Object System.Data.SqlClient.SqlConnection 'Server=TRANVANAN\SQLEXPRESS01;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True;'
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
UPDATE artifact.ArtifactTemplates
SET Name=N'Business Requirements',
    Description=N'Objectives, scope, as-is/to-be, MoSCoW requirements, business rules, constraints, and risks.',
    DefaultTitle=N'BRD-2026: Business Requirements',
    SkeletonHtmlContent=@skel,
    SampleHtmlContent=@sample,
    ChecklistJson=@check,
    UpdatedAt=SYSDATETIMEOFFSET()
WHERE Code=N'DEL-05';
"@
[void]$cmd.Parameters.AddWithValue("@skel", $skel)
[void]$cmd.Parameters.AddWithValue("@sample", $sample)
[void]$cmd.Parameters.AddWithValue("@check", $checklistJson)
Write-Host ("Updated DEL-05 rows={0} skel={1} sample={2}" -f $cmd.ExecuteNonQuery(), $skel.Length, $sample.Length)

$tid = [int](New-Object System.Data.SqlClient.SqlCommand("SELECT Id FROM artifact.ArtifactTemplates WHERE Code=N'DEL-05'", $conn)).ExecuteScalar()
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
  [void]$ins.ExecuteNonQuery()
  $order++
}
$conn.Close()
Write-Host "DEL-05 sections=$($checklist.Count)"
