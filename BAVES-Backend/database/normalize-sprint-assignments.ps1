# Normalize ScenarioVersions.Assignment → JSON sprints (split legacy blobs, duration weeks).
$ErrorActionPreference = "Stop"
$connStr = "Server=localhost;Database=BAVES;User Id=sa;Password=123456;TrustServerCertificate=True"

function Parse-Legacy([string]$text) {
  $re = [regex]::new('\bSprint\s*(\d+)\s*(?:\|\s*(?:Ends:[^:\n]+|Duration:\s*\d+\s*[dDwW]|days?|weeks?)?)?\s*:\s*', 'IgnoreCase')
  $matches = $re.Matches($text)
  if ($matches.Count -eq 0) {
    return ,@([pscustomobject]@{ sprintNo = 1; focus = $text.Trim(); durationValue = 1; durationUnit = "weeks"; templateCodes = @() })
  }
  $list = @()
  for ($i = 0; $i -lt $matches.Count; $i++) {
    $m = $matches[$i]
    $n = [int]$m.Groups[1].Value
    if ($n -lt 1) { continue }
    $start = $m.Index + $m.Length
    $end = if ($i + 1 -lt $matches.Count) { $matches[$i + 1].Index } else { $text.Length }
    $body = $text.Substring($start, $end - $start).Trim().TrimEnd('-', '•', ' ', "`t", "`r", "`n")
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

function Split-IfBlob($sprints) {
  if ($sprints.Count -ne 1) { return $sprints }
  $focus = [string]$sprints[0].focus
  $re = [regex]::new('\bSprint\s*\d+\s*:', 'IgnoreCase')
  $hits = $re.Matches($focus)
  if ($hits.Count -eq 0) { return $sprints }
  $text = if ($focus -match '^\s*Sprint\s*\d+\s*:') { $focus } else { "Sprint 1: $focus" }
  $split = @(Parse-Legacy $text)
  if ($split.Count -le 1) { return $sprints }
  $i = 1
  return @($split | ForEach-Object {
      $_.sprintNo = $i; $i++
      $_
    })
}

function Normalize-Assignment([string]$assignment) {
  $text = if ($null -eq $assignment) { "" } else { $assignment.Trim() }
  $sprints = $null
  if ($text.StartsWith("{") -or $text.StartsWith("[")) {
    try {
      $parsed = $text | ConvertFrom-Json
      $arr = if ($parsed -is [System.Array]) { $parsed } else { $parsed.sprints }
      if ($arr -and $arr.Count -gt 0) {
        $sprints = @()
        $idx = 1
        foreach ($row in $arr) {
          $unit = [string]$row.durationUnit
          if (-not $unit) { $unit = "weeks" }
          $unit = if ($unit.ToLower().StartsWith("d")) { "days" } else { "weeks" }
          $val = 1
          if ($row.durationValue) { $val = [Math]::Max(1, [int]$row.durationValue) }
          elseif ($row.duration) { $val = [Math]::Max(1, [int]$row.duration) }
          $codes = @()
          if ($row.templateCodes) { $codes = @($row.templateCodes | ForEach-Object { "$_".ToUpper() }) }
          $sprints += [pscustomobject]@{
            sprintNo = $idx
            focus = [string]$row.focus
            durationValue = $val
            durationUnit = $unit
            templateCodes = $codes
          }
          $idx++
        }
      }
    } catch {
      $sprints = $null
    }
  }
  if (-not $sprints) { $sprints = @(Parse-Legacy $text) }
  $sprints = @(Split-IfBlob $sprints)
  $i = 1
  $normalized = @($sprints | ForEach-Object {
      [pscustomobject]@{
        sprintNo = $i
        focus = [string]$_.focus
        durationValue = [Math]::Max(1, [int]($_.durationValue))
        durationUnit = if ([string]$_.durationUnit -match '^d') { "days" } else { "weeks" }
        templateCodes = @($_.templateCodes)
      }
      $i++
    })
  $payload = [pscustomobject]@{
    schema = "baves.sprints.v1"
    sprints = $normalized
  }
  return ($payload | ConvertTo-Json -Compress -Depth 6)
}

$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, Assignment FROM scenario.ScenarioVersions"
$reader = $cmd.ExecuteReader()
$rows = @()
while ($reader.Read()) {
  $rows += [pscustomobject]@{
    Id = [int]$reader["Id"]
    Assignment = if ($reader["Assignment"] -is [DBNull]) { "" } else { [string]$reader["Assignment"] }
  }
}
$reader.Close()

$updated = 0
foreach ($row in $rows) {
  $next = Normalize-Assignment $row.Assignment
  if ($next -eq $row.Assignment) { continue }
  $upd = $conn.CreateCommand()
  $upd.CommandText = "UPDATE scenario.ScenarioVersions SET Assignment = @a WHERE Id = @id"
  [void]$upd.Parameters.AddWithValue("@a", $next)
  [void]$upd.Parameters.AddWithValue("@id", $row.Id)
  [void]$upd.ExecuteNonQuery()
  $count = ($next | ConvertFrom-Json).sprints.Count
  Write-Host "Updated version $($row.Id) → $count sprints"
  $updated++
}
$conn.Close()
Write-Host "Done. Updated $updated version(s)."
