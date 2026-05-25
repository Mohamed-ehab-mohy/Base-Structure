namespace Acme.SaaS.Infrastructure.MultiTenancy.Services;

public class TenantConnectionService
{
    private readonly string _defaultConnectionString;

    public TenantConnectionService(string defaultConnectionString)
    {
        _defaultConnectionString = defaultConnectionString;
    }

    public string GetConnectionString(string? schemaName = null)
    {
        if (string.IsNullOrEmpty(schemaName))
            return _defaultConnectionString;

        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_defaultConnectionString);
        builder.InitialCatalog = $"{builder.InitialCatalog}_{schemaName}";

        return builder.ConnectionString;
    }
}
