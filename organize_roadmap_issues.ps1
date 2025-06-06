# Organize GitHub Issues by Implementation Roadmap
# This script adds labels and milestones to existing issues based on the roadmap phases

$repo = "miki-smart/autospartsEcommerce"
$owner = "miki-smart"

# Define roadmap phases and their corresponding labels
$roadmapPhases = @{
    "Phase 1" = @{
        "Label" = "phase-1-foundation"
        "Color" = "d73a4a"
        "Description" = "Phase 1: Core Authentication (Critical Priority)"
        "Milestone" = "Sprint 1-2: Foundation"
        "Keywords" = @("register", "login", "password", "profile", "authentication", "jwt")
    }
    "Phase 2" = @{
        "Label" = "phase-2-authorization" 
        "Color" = "0075ca"
        "Description" = "Phase 2: Authorization System (High Priority)"
        "Milestone" = "Sprint 3-4: Authorization"
        "Keywords" = @("role", "permission", "assign", "admin", "manage", "access control")
    }
    "Phase 3" = @{
        "Label" = "phase-3-security"
        "Color" = "ffc107"
        "Description" = "Phase 3: Security & Compliance (High Priority)"
        "Milestone" = "Sprint 5-6: Security"
        "Keywords" = @("2fa", "audit", "security", "compliance", "encrypt", "policy")
    }
    "Phase 4" = @{
        "Label" = "phase-4-integration"
        "Color" = "28a745"
        "Description" = "Phase 4: Social Auth & Integration (Medium Priority)"
        "Milestone" = "Sprint 7-8: Integration"
        "Keywords" = @("social", "oauth", "google", "facebook", "webhook", "api", "documentation")
    }
    "Phase 5" = @{
        "Label" = "phase-5-enterprise"
        "Color" = "6f42c1"
        "Description" = "Phase 5: Enterprise Features (Low Priority)"
        "Milestone" = "Sprint 9-10: Enterprise"
        "Keywords" = @("saml", "sso", "enterprise", "health", "backup", "monitor")
    }
}

Write-Host "Creating roadmap labels and milestones..." -ForegroundColor Yellow

# Create labels for each phase
foreach ($phase in $roadmapPhases.Keys) {
    $phaseData = $roadmapPhases[$phase]
    try {
        Write-Host "Creating label: $($phaseData.Label)"
        gh label create $phaseData.Label --description "$($phaseData.Description)" --color $phaseData.Color --repo $repo 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Label created: $($phaseData.Label)" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Label already exists: $($phaseData.Label)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "⚠️  Error creating label $($phaseData.Label): $_" -ForegroundColor Yellow
    }
}

# Create milestones
foreach ($phase in $roadmapPhases.Keys) {
    $phaseData = $roadmapPhases[$phase]
    try {
        Write-Host "Creating milestone: $($phaseData.Milestone)"
        gh milestone create "$($phaseData.Milestone)" --description "$($phaseData.Description)" --repo $repo 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Milestone created: $($phaseData.Milestone)" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Milestone already exists: $($phaseData.Milestone)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "⚠️  Error creating milestone $($phaseData.Milestone): $_" -ForegroundColor Yellow
    }
}

# Get all existing issues
Write-Host "`nFetching existing issues..." -ForegroundColor Yellow
$issues = gh issue list --repo $repo --limit 100 --json number,title,body,labels --state all | ConvertFrom-Json

Write-Host "Found $($issues.Count) issues to organize..." -ForegroundColor Cyan

$organizedCount = 0
$skippedCount = 0

foreach ($issue in $issues) {
    $issueText = "$($issue.title) $($issue.body)".ToLower()
    $matchedPhase = $null
    
    # Find which phase this issue belongs to based on keywords
    foreach ($phase in $roadmapPhases.Keys) {
        $phaseData = $roadmapPhases[$phase]
        $keywordMatches = 0
        
        foreach ($keyword in $phaseData.Keywords) {
            if ($issueText -match $keyword) {
                $keywordMatches++
            }
        }
        
        # If we found keyword matches, assign to this phase
        if ($keywordMatches -gt 0) {
            $matchedPhase = $phase
            break
        }
    }
    
    if ($matchedPhase) {
        $phaseData = $roadmapPhases[$matchedPhase]
        
        # Check if issue already has this label
        $hasLabel = $issue.labels | Where-Object { $_.name -eq $phaseData.Label }
        
        if (-not $hasLabel) {
            try {
                Write-Host "Adding label '$($phaseData.Label)' to issue #$($issue.number): $($issue.title)"
                gh issue edit $issue.number --add-label $phaseData.Label --repo $repo
                
                # Also set milestone
                gh issue edit $issue.number --milestone "$($phaseData.Milestone)" --repo $repo
                
                $organizedCount++
                Write-Host "✅ Organized issue #$($issue.number)" -ForegroundColor Green
                Start-Sleep -Milliseconds 500  # Rate limiting
            }
            catch {
                Write-Host "❌ Error organizing issue #$($issue.number): $_" -ForegroundColor Red
            }
        } else {
            Write-Host "⏭️  Issue #$($issue.number) already has label $($phaseData.Label)" -ForegroundColor Gray
            $skippedCount++
        }
    } else {
        Write-Host "❓ No phase match for issue #$($issue.number): $($issue.title)" -ForegroundColor Yellow
        $skippedCount++
    }
}

Write-Host "`n📊 SUMMARY:" -ForegroundColor Cyan
Write-Host "✅ Organized: $organizedCount issues" -ForegroundColor Green
Write-Host "⏭️  Skipped: $skippedCount issues" -ForegroundColor Yellow

Write-Host "`n🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Check your GitHub project board - issues should now be organized by phase"
Write-Host "2. Review any unmatched issues and manually assign them to appropriate phases"
Write-Host "3. Use the milestones to track sprint progress"
Write-Host "4. Filter by phase labels to focus on current sprint work"

Write-Host "`n🔗 View organized issues:" -ForegroundColor Cyan
Write-Host "Phase 1 (Foundation): https://github.com/$repo/issues?q=label%3Aphase-1-foundation"
Write-Host "Phase 2 (Authorization): https://github.com/$repo/issues?q=label%3Aphase-2-authorization"  
Write-Host "Phase 3 (Security): https://github.com/$repo/issues?q=label%3Aphase-3-security"
Write-Host "Phase 4 (Integration): https://github.com/$repo/issues?q=label%3Aphase-4-integration"
Write-Host "Phase 5 (Enterprise): https://github.com/$repo/issues?q=label%3Aphase-5-enterprise"
