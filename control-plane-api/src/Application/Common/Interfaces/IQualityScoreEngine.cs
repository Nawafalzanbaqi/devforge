namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IQualityScoreEngine
{
    int CalculateScore(QualityMetrics sonarMetrics, PlaywrightResult playwrightResult, bool buildSuccess, out int securityScore);
}
