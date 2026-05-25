using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Infrastructure.Services.CustomLogic.Strategies;

namespace Acme.SaaS.Infrastructure.Services.CustomLogic.Factories;

public class TaxStrategyFactory
{
    private readonly ITenantProvider _tenantProvider;
    private readonly StandardTaxStrategy _standard;
    private readonly VodafoneTaxStrategy _vodafone;

    public TaxStrategyFactory(ITenantProvider tenantProvider, StandardTaxStrategy standard, VodafoneTaxStrategy vodafone)
    {
        _tenantProvider = tenantProvider;
        _standard = standard;
        _vodafone = vodafone;
    }

    public ITaxCalculationStrategy GetStrategy()
    {
        var identifier = _tenantProvider.GetIdentifier();
        return identifier?.ToLowerInvariant() switch
        {
            "vodafone" => _vodafone,
            _ => _standard
        };
    }
}
