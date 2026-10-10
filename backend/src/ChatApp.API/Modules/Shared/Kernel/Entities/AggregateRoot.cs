using ChatApp.API.Modules.Shared.Kernel.Abstractions;

namespace ChatApp.API.Modules.Shared.Kernel.Entities;

public abstract class AggregateRoot : BaseAuditableEntity, IAggregateRoot
{
}
