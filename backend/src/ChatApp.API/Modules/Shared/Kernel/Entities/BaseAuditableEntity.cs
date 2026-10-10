using ChatApp.API.Modules.Shared.Kernel.Abstractions;

namespace ChatApp.API.Modules.Shared.Kernel.Entities;

public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}