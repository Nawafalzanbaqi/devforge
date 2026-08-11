using System;
using DevForge.ControlPlane.Application.Common.Interfaces;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class QualityScoreEngine : IQualityScoreEngine
{
    public int CalculateScore(QualityMetrics sonarMetrics, PlaywrightResult playwrightResult, bool buildSuccess, out int securityScore)
    {
        // 1. Build Health (10%)
        double buildPoints = buildSuccess ? 10.0 : 0.0;

        // 2. Playwright E2E Tests (30%)
        double playwrightPoints = 0.0;
        if (playwrightResult.Details.Count > 0)
        {
            int passedTests = 0;
            foreach (var detail in playwrightResult.Details)
            {
                if (detail.Passed) passedTests++;
            }
            playwrightPoints = 30.0 * ((double)passedTests / playwrightResult.Details.Count);
        }

        // 3. Sonar Quality Code Gate (40%)
        // - 20 points for Code Coverage (proportionate to 100%)
        double coveragePoints = 20.0 * ((double)sonarMetrics.Coverage / 100.0);
        
        // - 10 points for Duplication Density under 10%
        double duplicationPoints = 10.0;
        if (sonarMetrics.DuplicationDensity >= 10.0m)
        {
            duplicationPoints = Math.Max(0.0, 10.0 - (double)(sonarMetrics.DuplicationDensity - 10.0m));
        }

        // - 10 points for Blocker Issues
        double blockerPoints = Math.Max(0.0, 10.0 - sonarMetrics.BlockerIssues);
        double sonarPoints = coveragePoints + duplicationPoints + blockerPoints;

        // 4. Security Score (20%)
        // - 10 points based on security rating (A: 10, B: 8, C: 6, D: 4, E: 2, other: 0)
        double ratingPoints = sonarMetrics.SecurityRating.ToUpper() switch
        {
            "A" => 10.0,
            "B" => 8.0,
            "C" => 6.0,
            "D" => 4.0,
            "E" => 2.0,
            _ => 0.0
        };

        // - 10 points for Critical Vulnerabilities (deduct 5 per vulnerability)
        double vulnerabilityPoints = Math.Max(0.0, 10.0 - (sonarMetrics.CriticalVulnerabilities * 5.0));
        
        double securityPointsTotal = ratingPoints + vulnerabilityPoints;
        securityScore = (int)Math.Round(securityPointsTotal * 5.0); // scale to 0-100 for output metadata

        // Calculate combined score (0 - 100)
        double totalScore = buildPoints + playwrightPoints + sonarPoints + securityPointsTotal;
        return (int)Math.Clamp(Math.Round(totalScore), 0, 100);
    }
}
