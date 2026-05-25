namespace Acme.SaaS.Infrastructure.Services.CustomLogic.Strategies;

public class VodafoneTaxStrategy : ITaxCalculationStrategy
{
    public decimal CalculateTax(decimal amount) => (amount * 0.14m) + 5;
}
