namespace Acme.SaaS.Infrastructure.MultiTenancy;

public class TenancyOptions
{
    public TenancyMode Mode { get; set; } = TenancyMode.SeparateSchema;
}

public enum TenancyMode
{
    SeparateSchema = 0,
    SharedSchema = 1
}
