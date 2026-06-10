using Microsoft.Extensions.Logging;

namespace Acme.SaaS.Infrastructure.Services;

public class PaymentGatewayService
{
    private readonly ILogger<PaymentGatewayService> _logger;

    public PaymentGatewayService(ILogger<PaymentGatewayService> logger)
    {
        _logger = logger;
    }

    public Task<string> ProcessPaymentAsync(decimal amount, string currency, string paymentMethod, CancellationToken ct = default)
    {
        _logger.LogInformation("Processing payment: {Amount} {Currency} via {Method}", amount, currency, paymentMethod);
        return Task.FromResult($"txn_{Guid.NewGuid():N}");
    }

    public Task RefundAsync(string transactionId, CancellationToken ct = default)
    {
        _logger.LogInformation("Refunding transaction: {TransactionId}", transactionId);
        return Task.CompletedTask;
    }
}
