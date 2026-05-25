namespace Acme.SaaS.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? GetUserId();
    string? GetUserEmail();
    string? GetRole();
    bool IsAuthenticated();
}
