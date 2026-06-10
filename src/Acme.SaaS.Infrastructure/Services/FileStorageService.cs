using Microsoft.Extensions.Logging;

namespace Acme.SaaS.Infrastructure.Services;

public class FileStorageService
{
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
    }

    public Task<string> UploadAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        _logger.LogInformation("Uploading file: {FileName}", fileName);
        return Task.FromResult($"https://storage.example.com/{fileName}");
    }

    public Task<string> GetUrlAsync(string fileName, CancellationToken ct = default)
    {
        return Task.FromResult($"https://storage.example.com/{fileName}");
    }

    public Task DeleteAsync(string fileName, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting file: {FileName}", fileName);
        return Task.CompletedTask;
    }
}
