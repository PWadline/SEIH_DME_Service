using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity.SEIH;

public class UsersRoleEntity: AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

