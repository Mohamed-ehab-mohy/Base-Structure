namespace Acme.SaaS.API.Models;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    public string[]? Errors { get; set; }

    public static ApiResponse Ok(object? data = null, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse Fail(string message, string[]? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
