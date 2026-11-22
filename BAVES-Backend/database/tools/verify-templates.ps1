$ErrorActionPreference = "Stop"

function Try-Login($url, $body) {
  try {
    return Invoke-RestMethod -Uri $url -Method POST -ContentType "application/json" -Body $body
  } catch {
    return $null
  }
}

$body = '{"email":"learner@gmail.com","password":"antran596"}'
$login = Try-Login "http://localhost:5291/api/auth/login" $body
if (-not $login) {
  $login = Try-Login "http://localhost:5291/api/identity/auth/login" $body
}

if (-not $login) { throw "Login failed" }

$token = $login.accessToken
if (-not $token) { $token = $login.AccessToken }
if (-not $token) { $token = $login.token }
Write-Host "Login ok, token length:" $token.Length

$headers = @{ Authorization = "Bearer $token" }
$templates = Invoke-RestMethod -Uri "http://localhost:5291/api/artifact-templates" -Headers $headers
Write-Host "Template count:" $templates.Count
foreach ($t in $templates) {
  $skel = 0
  $sample = 0
  if ($t.skeletonHtmlContent) { $skel = $t.skeletonHtmlContent.Length }
  if ($t.sampleHtmlContent) { $sample = $t.sampleHtmlContent.Length }
  Write-Host ("{0} | {1} | skel={2} sample={3} cat={4}" -f $t.code, $t.name, $skel, $sample, $t.category)
}
