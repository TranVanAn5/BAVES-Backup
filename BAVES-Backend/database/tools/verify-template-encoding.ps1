$body = '{"email":"learner@gmail.com","password":"antran596"}'
$login = Invoke-RestMethod -Uri "http://localhost:5291/api/auth/login" -Method POST -ContentType "application/json" -Body $body
$h = @{ Authorization = ("Bearer " + $login.accessToken) }
$t = Invoke-RestMethod -Uri "http://localhost:5291/api/artifact-templates" -Headers $h
foreach ($x in $t) {
  $html = [string]$x.skeletonHtmlContent
  $bad = ([regex]::Matches($html, "â€¢|PhÃ¢n|Ã¡c bÃªn")).Count
  $head = $html.Substring(0, [Math]::Min(60, $html.Length))
  Write-Host ("{0} bad={1} {2}" -f $x.code, $bad, $head)
}
