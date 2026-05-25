namespace Acme.SaaS.Domain.Entities;

public class User : Common.BaseAuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Role { get; set; } = "Member";
    public Guid TenantId { get; set; }
}
