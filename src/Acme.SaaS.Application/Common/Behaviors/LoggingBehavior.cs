using Microsoft.Extensions.Logging;

namespace Acme.SaaS.Application.Common.Behaviors;

public class LoggingBehavior
{
    private readonly ILogger<LoggingBehavior> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(string operationName, Func<Task<T>> action)
    {
        var start = DateTime.UtcNow;
        _logger.LogInformation("Starting operation: {OperationName}", operationName);

        try
        {
            var result = await action();
            var elapsed = DateTime.UtcNow - start;
            _logger.LogInformation("Completed operation: {OperationName} in {ElapsedMs}ms",
                operationName, elapsed.TotalMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed operation: {OperationName}", operationName);
            throw;
        }
    }
}
