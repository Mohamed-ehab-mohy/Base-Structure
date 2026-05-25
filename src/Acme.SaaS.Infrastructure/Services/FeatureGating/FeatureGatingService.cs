using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Enums;

namespace Acme.SaaS.Infrastructure.Services.FeatureGating;

public interface IFeatureGatingService
{
    bool IsFeatureAllowed(string featureName);
}

public class FeatureGatingService : IFeatureGatingService
{
    private readonly ITenantProvider _tenantProvider;

    private static readonly Dictionary<string, SubscriptionPlan> FeaturePlanMap = new()
    {
        ["export"] = SubscriptionPlan.Pro,
        ["api-access"] = SubscriptionPlan.Pro,
        ["audit-logs"] = SubscriptionPlan.Enterprise,
        ["custom-branding"] = SubscriptionPlan.Enterprise,
        ["basic-crud"] = SubscriptionPlan.Free
    };

    public FeatureGatingService(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public bool IsFeatureAllowed(string featureName)
    {
        var feature = featureName.ToLowerInvariant();
        if (!FeaturePlanMap.ContainsKey(feature))
            return false;

        var requiredPlan = FeaturePlanMap[feature];
        return _tenantProvider.GetPlan() >= requiredPlan;
    }
}
