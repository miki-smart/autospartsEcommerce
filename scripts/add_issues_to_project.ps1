# Add all GitHub issues to the project board
$repo = "miki-smart/autospartsEcommerce"
$projectNumber = 2
$owner = "miki-smart"

# Get all issue numbers
$issues = gh issue list --repo $repo --limit 30 --json number | ConvertFrom-Json

Write-Host "Found $($issues.Count) issues to add to project board..."

$successCount = 0
$errorCount = 0

foreach ($issue in $issues) {
    try {
        Write-Host "Adding issue #$($issue.number) to project..."
        $result = gh project item-add $projectNumber --owner $owner --url "https://github.com/$repo/issues/$($issue.number)" 2>&1
        if ($LASTEXITCODE -eq 0) {
            $successCount++
            Write-Host "Added successfully" -ForegroundColor Green
        } else {
            $errorCount++
            Write-Host "Error: $result" -ForegroundColor Red
        }
        Start-Sleep -Seconds 1  # Rate limiting
    }
    catch {
        $errorCount++
        Write-Host "Error: $_" -ForegroundColor Red
    }
}

Write-Host "`nSummary:"
Write-Host "Successfully added: $successCount issues to project board" -ForegroundColor Green
Write-Host "Errors: $errorCount issues" -ForegroundColor Red
