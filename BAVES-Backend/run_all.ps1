# Simple PowerShell launcher for BAVES backend and AI Runtime
# -------------------------------------------------------
# Run from the BAVES-Backend root folder:
#   .\run_all.ps1

$backendDir = "${PSScriptRoot}\src\BAVES.Api"
$runtimeDir = "${PSScriptRoot}\agent-runtime"

# Launch .NET API (listening on http://localhost:5291)
Start-Process "dotnet" -ArgumentList "run --project $backendDir" -WorkingDirectory $backendDir -NoNewWindow -PassThru | Out-Null

# Launch Python AI Runtime (listening on http://localhost:8000)
Start-Process "${runtimeDir}\\.venv\\Scripts\\python.exe" -ArgumentList "-m uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload" -WorkingDirectory $runtimeDir -NoNewWindow -PassThru | Out-Null

Write-Host "Both services started. Open Swagger UI at http://localhost:5291/swagger"
