# Simple script to add all issues to project board
Write-Host "Adding all issues to project board..." -ForegroundColor Yellow

# Get all issue numbers from 1 to 29
for ($i = 1; $i -le 29; $i++) {
    try {
        Write-Host "Adding issue #$i..." -NoNewline
        gh project item-add 2 --owner miki-smart --url "https://github.com/miki-smart/autospartsEcommerce/issues/$i" 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host " Success" -ForegroundColor Green
        } else {
            Write-Host " Skipped" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host " Error" -ForegroundColor Red
    }
    Start-Sleep -Milliseconds 500
}

Write-Host "Finished adding issues to project board!" -ForegroundColor Green
