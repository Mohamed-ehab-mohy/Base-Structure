namespace Acme.SaaS.Infrastructure.Services.CustomLogic.Strategies;

public interface ITaxCalculationStrategy
{
    decimal CalculateTax(decimal amount);
}

public class StandardTaxStrategy : ITaxCalculationStrategy
{
    public decimal CalculateTax(decimal amount) => amount * 0.10m;
}
