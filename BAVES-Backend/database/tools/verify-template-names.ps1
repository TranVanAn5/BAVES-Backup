$body = '{"email":"learner@gmail.com","password":"antran596"}'
$login = Invoke-RestMethod -Uri "http://localhost:5291/api/auth/login" -Method POST -ContentType "application/json" -Body $body
$h = @{ Authorization = ("Bearer " + $login.accessToken) }
$t = Invoke-RestMethod -Uri "http://localhost:5291/api/artifact-templates" -Headers $h
foreach ($x in $t) {
  Write-Host ("{0} | {1} | skel={2}" -f $x.code, $x.name, $x.skeletonHtmlContent.Length)
}
$stk = $t | Where-Object { $_.code -eq "DEL-03" }
$hasRaci = $stk.skeletonHtmlContent -match "RACI Matrix"
$hasGrid = $stk.skeletonHtmlContent -match "Power / Interest"
$hasEngage = $stk.skeletonHtmlContent -match "Engagement"
Write-Host ("DEL-03 sections: RACI={0} Grid={1} Engagement={2}" -f $hasRaci, $hasGrid, $hasEngage)
