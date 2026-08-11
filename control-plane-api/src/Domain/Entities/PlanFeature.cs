using System;

namespace DevForge.ControlPlane.Domain.Entities;

public class PlanFeature : BaseEntity
{
    public string PlanName { get; set; } = string.Empty; // Free, Pro, Agency, Enterprise
    public string FeatureKey { get; set; } = string.Empty; // CanRepair, CanDeploy, CanCreateProject, MultiClient
    public bool IsEnabled { get; set; }
}
