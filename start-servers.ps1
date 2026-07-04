# Start Backend API in background
Write-Host "Starting Backend API (port 5000)..." -ForegroundColor Green
$backendJob = Start-Job -ScriptBlock {
    cd "C:\Projects\Pijeen\api"
    dotnet run
}

# Wait for backend to start
Start-Sleep -Seconds 5

# Start Frontend in background
Write-Host "Starting Frontend React (port 3000)..." -ForegroundColor Green
$frontendJob = Start-Job -ScriptBlock {
    cd "C:\Projects\Pijeen\client"
    npm start
}

Write-Host "Both servers are starting..." -ForegroundColor Cyan
Write-Host "Backend: http://localhost:5000" -ForegroundColor Yellow
Write-Host "Frontend: http://localhost:3000" -ForegroundColor Yellow
Write-Host "Swagger API Docs: http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "To stop servers, run: Stop-Job -Id $($backendJob.Id); Stop-Job -Id $($frontendJob.Id)" -ForegroundColor Gray

# Wait for jobs
Get-Job | Wait-Job
