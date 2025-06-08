# Set your repo name
$repo = "miki-smart/autospartsEcommerce" # Correct repository name

# Path to your backlog file
$backlogPath = ".\BA_and_Backlog\Identity_server\product_backlog.md"

# Read the backlog file
$content = Get-Content $backlogPath -Raw

# Split content into lines and find user stories
$lines = $content -split "`n"

# Extract user stories (lines starting with "- **As a")
$userStories = @()
for ($i = 0; $i -lt $lines.Length; $i++) {
    $line = $lines[$i].Trim()
    if ($line -match "^- \*\*As a") {
        # Extract the full user story including acceptance criteria
        $story = $line -replace "^- \*\*", "" -replace "\*\*", ""
        
        # Look for acceptance criteria on the next lines
        $acceptanceCriteria = @()
        $j = $i + 1
        while ($j -lt $lines.Length -and $lines[$j].Trim() -match "^\s*- ") {
            $acceptanceCriteria += $lines[$j].Trim()
            $j++
        }
        
        # Create full story with acceptance criteria
        $fullStory = $story
        if ($acceptanceCriteria.Count -gt 0) {
            $fullStory += "`n`n**Acceptance Criteria:**`n"
            foreach ($criteria in $acceptanceCriteria) {
                $fullStory += "$criteria`n"
            }
        }
        
        $userStories += @{
            Title = $story.Split(',')[0].Trim()
            Body = $fullStory
        }
    }
}

Write-Host "Found $($userStories.Count) user stories"

foreach ($story in $userStories) {
    Write-Host "Creating issue: $($story.Title)"
    try {
        gh issue create --repo $repo --title "$($story.Title)" --body "$($story.Body)" --label "user-story"
        Start-Sleep -Seconds 1  # Rate limiting
    }
    catch {
        Write-Host "Error creating issue: $_"
    }
}