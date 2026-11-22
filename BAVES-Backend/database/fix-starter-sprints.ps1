$ErrorActionPreference = "Stop"

function Parse-Legacy([string]$text) {
  $re = [regex]::new('\bSprint\s*(\d+)\s*(?:\|\s*(?:Ends:[^:\n]+|Duration:\s*\d+\s*[dDwW]|days?|weeks?)?)?\s*:\s*', 'IgnoreCase')
  $matches = $re.Matches($text)
  $list = @()
  for ($i = 0; $i -lt $matches.Count; $i++) {
    $m = $matches[$i]
    $n = [int]$m.Groups[1].Value
    if ($n -lt 1) { continue }
    $start = $m.Index + $m.Length
    $end = if ($i + 1 -lt $matches.Count) { $matches[$i + 1].Index } else { $text.Length }
    $body = $text.Substring($start, $end - $start).Trim()
    $list += [pscustomobject]@{
      sprintNo = $n
      focus = $body
      durationValue = 1
      durationUnit = "weeks"
      templateCodes = @()
    }
  }
  return @($list | Sort-Object sprintNo)
}

function To-JsonSprints([string]$raw) {
  $s = @(Parse-Legacy $raw)
  $norm = @()
  for ($i = 0; $i -lt $s.Count; $i++) {
    $norm += [pscustomobject]@{
      sprintNo = $i + 1
      focus = [string]$s[$i].focus
      durationValue = 1
      durationUnit = "weeks"
      templateCodes = @()
    }
  }
  return ([pscustomobject]@{ schema = "baves.sprints.v1"; sprints = $norm } | ConvertTo-Json -Compress -Depth 6)
}

$map = [ordered]@{
  "BEG-OMS-01"   = "Sprint 1: BAP ngắn + stakeholder map (Ops, CSKH, Store) + system context. Sprint 2: Phỏng vấn; notes về status & nguồn cập nhật. Sprint 3: State catalog as-is/to-be + RACI cập nhật. Sprint 4: 3 user stories + AC (UI hoặc API status)."
  "BEG-WMS-01"   = "Sprint 1: BAP + map (Supervisor, QA, Night Lead) + system context IoT channel. Sprint 2: Phỏng vấn ngưỡng & phản ứng. Sprint 3: Severity matrix + acknowledge flow. Sprint 4: 3 US + AC incident log."
  "BEG-LOS-01"   = "Sprint 1: BAP + map (Ops, Onboarding, Compliance). Sprint 2: Phỏng vấn SLA & lý do hold. Sprint 3: eKYC as-is + taxonomy. Sprint 4: 3 US + AC."
  "BEG-PROMO-01" = "Sprint 1: BAP + map (Marketing Ops, POS, CX). Sprint 2: Phỏng vấn lệch rule. Sprint 3: Rule table. Sprint 4: 3 US + AC."
  "BEG-CAFE-01"  = "Sprint 1: Đọc context; lập BAP ngắn và stakeholder map 3 bên. Sprint 2: Phỏng vấn stakeholder. Sprint 3: Chuẩn hóa quy trình/as-is to-be. Sprint 4: User stories + AC."
}

$conn = New-Object System.Data.SqlClient.SqlConnection "Server=localhost;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True"
$conn.Open()
foreach ($code in $map.Keys) {
  $json = To-JsonSprints $map[$code]
  $cmd = $conn.CreateCommand()
  $cmd.CommandText = @"
UPDATE v SET Assignment = @a
FROM scenario.ScenarioVersions v
JOIN scenario.Scenarios s ON s.Id = v.ScenarioId
WHERE s.Code = @c
"@
  [void]$cmd.Parameters.AddWithValue("@a", $json)
  [void]$cmd.Parameters.AddWithValue("@c", $code)
  $n = $cmd.ExecuteNonQuery()
  $count = ($json | ConvertFrom-Json).sprints.Count
  Write-Host "$code -> $count sprints (rows=$n)"
}
$conn.Close()
