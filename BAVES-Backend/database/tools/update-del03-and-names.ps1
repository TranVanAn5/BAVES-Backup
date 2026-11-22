$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot "..\doc-templates"
$skel = [System.IO.File]::ReadAllText((Join-Path $root "del-03-skeleton.html"), [System.Text.UTF8Encoding]::new($false))
$sample = [System.IO.File]::ReadAllText((Join-Path $root "del-03-sample.html"), [System.Text.UTF8Encoding]::new($false))

$names = @{
  "DEL-01" = @{
    Name = "Problem Statement"
    DefaultTitle = "PRB-2026: Problem Statement"
    Description = "Business need, current state, root cause, future state, gaps, risks, and evidence."
  }
  "DEL-02" = @{
    Name = "Business Analysis Plan"
    DefaultTitle = "BAP-2026: Business Analysis Plan"
    Description = "BA approach, stakeholder engagement, governance, information management, schedule, risks, and performance metrics."
  }
  "DEL-03" = @{
    Name = "Stakeholder Analysis"
    DefaultTitle = "STK-2026: Stakeholder Analysis"
    Description = "Stakeholder profiles, power/interest grid, engagement plan, RACI, and resistance risks."
  }
  "DEL-04" = @{
    Name = "Elicitation Results"
    DefaultTitle = "ELC-2026: Elicitation Results"
    Description = "Session objectives, interview log, evidence, and action items from elicitation."
  }
  "DEL-05" = @{
    Name = "Business Requirements"
    DefaultTitle = "BRD-2026: Business Requirements"
    Description = "Business objectives, as-is/to-be, MoSCoW requirements, and business rules."
  }
  "DEL-06" = @{
    Name = "Process Model"
    DefaultTitle = "PROC-2026: Process Model"
    Description = "Swimlanes, step-by-step flow, exception paths, and cycle-time notes."
  }
  "DEL-07" = @{
    Name = "Requirements Specification"
    DefaultTitle = "SRS-2026: Requirements Specification"
    Description = "User stories, acceptance criteria, API contracts, and non-functional requirements."
  }
  "DEL-08" = @{
    Name = "Traceability Matrix"
    DefaultTitle = "RTM-2026: Traceability Matrix"
    Description = "Bidirectional trace from goals and requirements to stories, tasks, and tests."
  }
}

$checklist = @(
  @{ id = "del03-c0"; title = "0. Document Governance"; description = "Owner, sponsor, status, document ID, deliverable code."; rubricHint = "Sponsor must be a decision authority."; snippetHtml = "<h2>0. Document Governance</h2><p>Lead BA, Sponsor, Status, Document ID, DEL-03.</p>" }
  @{ id = "del03-c1"; title = "1. Stakeholder Profiles"; description = "Role, representative, interest, influence, attitude, pain, outcome."; rubricHint = "At least 4 stakeholders with influence and attitude filled."; snippetHtml = "<h2>1. Stakeholder Identification &amp; Profiles</h2>" }
  @{ id = "del03-c2"; title = "2. Power / Interest Grid"; description = "Place stakeholders into Keep Satisfied / Manage Closely / Monitor / Keep Informed."; rubricHint = "No stakeholder left unclassified."; snippetHtml = "<h2>2. Power / Interest Grid</h2>" }
  @{ id = "del03-c3"; title = "3. Engagement Plan"; description = "Goal, technique, cadence, owner per group."; rubricHint = "High-power stakeholders must have explicit cadence."; snippetHtml = "<h2>3. Engagement &amp; Communication Plan</h2>" }
  @{ id = "del03-c4"; title = "4. RACI Matrix"; description = "R/A/C/I for BA and delivery activities."; rubricHint = "Each activity must have exactly one A."; snippetHtml = "<h2>4. RACI Matrix</h2>" }
  @{ id = "del03-c5"; title = "5. Risks & Resistance"; description = "Stakeholder risks with mitigation."; rubricHint = "At least 2 risks with mitigation."; snippetHtml = "<h2>5. Stakeholder Risks &amp; Resistance</h2>" }
)
$checklistJson = ($checklist | ConvertTo-Json -Compress -Depth 5)

$conn = New-Object System.Data.SqlClient.SqlConnection 'Server=TRANVANAN\SQLEXPRESS01;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True;'
$conn.Open()

foreach ($code in $names.Keys) {
  $meta = $names[$code]
  $cmd = $conn.CreateCommand()
  if ($code -eq "DEL-03") {
    $cmd.CommandText = @"
UPDATE artifact.ArtifactTemplates
SET Name=@name, Description=@desc, DefaultTitle=@title,
    SkeletonHtmlContent=@skel, SampleHtmlContent=@sample, ChecklistJson=@check,
    UpdatedAt=SYSDATETIMEOFFSET()
WHERE Code=@code;
"@
    [void]$cmd.Parameters.AddWithValue("@skel", $skel)
    [void]$cmd.Parameters.AddWithValue("@sample", $sample)
    [void]$cmd.Parameters.AddWithValue("@check", $checklistJson)
  } else {
    $cmd.CommandText = @"
UPDATE artifact.ArtifactTemplates
SET Name=@name, Description=@desc, DefaultTitle=@title, UpdatedAt=SYSDATETIMEOFFSET()
WHERE Code=@code;
"@
  }
  [void]$cmd.Parameters.AddWithValue("@code", $code)
  [void]$cmd.Parameters.AddWithValue("@name", $meta.Name)
  [void]$cmd.Parameters.AddWithValue("@desc", $meta.Description)
  [void]$cmd.Parameters.AddWithValue("@title", $meta.DefaultTitle)
  $n = $cmd.ExecuteNonQuery()
  Write-Host ("{0} -> {1} (rows={2})" -f $code, $meta.Name, $n)
}

# Refresh sections for DEL-03
$getId = $conn.CreateCommand()
$getId.CommandText = "SELECT Id FROM artifact.ArtifactTemplates WHERE Code=N'DEL-03'"
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

# Also shorten H1 titles inside other templates where they are long Vietnamese canvas names
$fixHeads = $conn.CreateCommand()
$fixHeads.CommandText = @"
UPDATE artifact.ArtifactTemplates SET SkeletonHtmlContent = REPLACE(SkeletonHtmlContent, N'<h1>Problem Statement &amp; Opportunity Assessment</h1>', N'<h1>Problem Statement</h1>'), SampleHtmlContent = REPLACE(SampleHtmlContent, N'<h1>Problem Statement &amp; Opportunity Assessment</h1>', N'<h1>Problem Statement</h1>') WHERE Code=N'DEL-01';
UPDATE artifact.ArtifactTemplates SET SkeletonHtmlContent = REPLACE(SkeletonHtmlContent, N'<h1>Business Analysis Plan (BAP)</h1>', N'<h1>Business Analysis Plan</h1>'), SampleHtmlContent = REPLACE(SampleHtmlContent, N'<h1>Business Analysis Plan (BAP)</h1>', N'<h1>Business Analysis Plan</h1>') WHERE Code=N'DEL-02';
UPDATE artifact.ArtifactTemplates SET SkeletonHtmlContent = REPLACE(SkeletonHtmlContent, N'<h1>ELC: Biên bản Phỏng vấn Khơi gợi Yêu cầu (Elicitation Meeting Notes)</h1>', N'<h1>Elicitation Results</h1>'), SampleHtmlContent = REPLACE(SampleHtmlContent, N'<h1>ELC: Biên bản Phỏng vấn Khơi gợi Yêu cầu (Elicitation Meeting Notes)</h1>', N'<h1>Elicitation Results</h1>') WHERE Code=N'DEL-04';
UPDATE artifact.ArtifactTemplates SET SkeletonHtmlContent = REPLACE(SkeletonHtmlContent, N'<h1>BRD: Tài liệu Yêu cầu Nghiệp vụ Doanh nghiệp (Business Requirements Document)</h1>', N'<h1>Business Requirements</h1>'), SampleHtmlContent = REPLACE(SampleHtmlContent, N'<h1>BRD: Tài liệu Yêu cầu Nghiệp vụ Doanh nghiệp (Business Requirements Document)</h1>', N'<h1>Business Requirements</h1>') WHERE Code=N'DEL-05';
UPDATE artifact.ArtifactTemplates SET SkeletonHtmlContent = REPLACE(SkeletonHtmlContent, N'<h1>BPMN: Đặc tả Mô hình Quy trình Nghiệp vụ (Process Model Canvas)</h1>', N'<h1>Process Model</h1>'), SampleHtmlContent = REPLACE(SampleHtmlContent, N'<h1>PROC: Đặc tả Mô hình Quy trình Nghiệp vụ (BPMN 2.0 Specification)</h1>', N'<h1>Process Model</h1>') WHERE Code=N'DEL-06';
UPDATE artifact.ArtifactTemplates SET SkeletonHtmlContent = REPLACE(SkeletonHtmlContent, N'<h1>SRS: Đặc tả Yêu cầu Kỹ thuật Phần mềm (Software Requirements Spec)</h1>', N'<h1>Requirements Specification</h1>'), SampleHtmlContent = REPLACE(SampleHtmlContent, N'<h1>SRS: Đặc tả Kỹ thuật Phần mềm & Danh mục User Stories Agile</h1>', N'<h1>Requirements Specification</h1>') WHERE Code=N'DEL-07';
UPDATE artifact.ArtifactTemplates SET SkeletonHtmlContent = REPLACE(SkeletonHtmlContent, N'<h1>RTM: Ma trận Truy vết Yêu cầu Nghiệp vụ (Traceability Matrix)</h1>', N'<h1>Traceability Matrix</h1>'), SampleHtmlContent = REPLACE(SampleHtmlContent, N'<h1>RTM: Ma trận Truy vết Yêu cầu Hai chiều (Requirements Traceability Matrix)</h1>', N'<h1>Traceability Matrix</h1>') WHERE Code=N'DEL-08';
"@
[void]$fixHeads.ExecuteNonQuery()

$list = $conn.CreateCommand()
$list.CommandText = "SELECT Code, Name, DefaultTitle, LEN(SkeletonHtmlContent) SkelLen FROM artifact.ArtifactTemplates ORDER BY DisplayOrder"
$a = New-Object System.Data.SqlClient.SqlDataAdapter $list
$t = New-Object System.Data.DataTable
[void]$a.Fill($t)
$conn.Close()
$t | Format-Table -AutoSize
Write-Host "Done."
