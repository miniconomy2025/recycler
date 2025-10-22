Write-Host "Running Recycler Integration Tests" -ForegroundColor Green

try {
    docker info | Out-Null
    Write-Host "Docker is running" -ForegroundColor Green
} catch {
    Write-Host "Docker is not running. Please start Docker and try again." -ForegroundColor Red
    exit 1
}

Set-Location Recycler.Tests

Write-Host "Building test project..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed" -ForegroundColor Red
    exit 1
}

dotnet test --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "All tests passed!" -ForegroundColor Green
} else {
    Write-Host "Some tests failed" -ForegroundColor Red
    exit 1
}
