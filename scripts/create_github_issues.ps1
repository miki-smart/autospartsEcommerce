# Create GitHub Issues from Product Backlog
# This script parses the product backlog and creates properly formatted GitHub issues

$repo = "miki-smart/autospartsEcommerce"
$backlogPath = ".\BA_and_Backlog\Identity_server\product_backlog.md"

# Read the entire backlog file
$content = Get-Content $backlogPath -Raw

# Split into lines for processing
$lines = $content -split "`r?`n"

$userStories = @()
$currentStory = $null

for ($i = 0; $i -lt $lines.Length; $i++) {
    $line = $lines[$i].Trim()
    
    # Check if this is a user story line
    if ($line -match "^- \*\*As a.+?\*\*") {
        # Save previous story if exists
        if ($currentStory) {
            $userStories += $currentStory
        }
        
        # Extract the story text without markdown formatting
        $storyText = $line -replace "^- \*\*", "" -replace "\*\*$", ""
        
        # Extract a meaningful title (everything after "so I can" or "so that" or use first part)
        $title = $storyText
        if ($storyText -match "so (I can|that) (.+)") {
            $purpose = $matches[2]
            $title = $purpose.Substring(0, [Math]::Min(50, $purpose.Length))
            if ($purpose.Length -gt 50) {
                $title += "..."
            }
        } elseif ($storyText -match ", I want to (.+?),") {
            $want = $matches[1]
            $title = $want.Substring(0, [Math]::Min(50, $want.Length))
            if ($want.Length -gt 50) {
                $title += "..."
            }
        }
        
        $currentStory = @{
            Title = $title
            Body = "**User Story:**`n$storyText`n"
            AcceptanceCriteria = @()
        }
    }
    # Check if this is acceptance criteria header
    elseif ($line -match "^\s*- Acceptance Criteria:" -and $currentStory) {
        $currentStory.Body += "`n**Acceptance Criteria:**"
    }
    # Check if this is a criteria item
    elseif ($line -match "^\s*- .+" -and $currentStory -and $currentStory.Body -match "Acceptance Criteria:") {
        $criteria = $line.Trim()
        $currentStory.Body += "`n$criteria"
    }
}

# Add the last story
if ($currentStory) {
    $userStories += $currentStory
}

Write-Host "Found $($userStories.Count) user stories. Creating GitHub issues..."

$successCount = 0
$errorCount = 0

foreach ($story in $userStories) {
    try {
        Write-Host "Creating: $($story.Title)"
        $result = gh issue create --repo $repo --title "$($story.Title)" --body "$($story.Body)" --label "user-story" 2>&1
        if ($LASTEXITCODE -eq 0) {
            $successCount++
            Write-Host "Created successfully" -ForegroundColor Green
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
Write-Host "Successfully created: $successCount issues" -ForegroundColor Green
Write-Host "Errors: $errorCount issues" -ForegroundColor Red
