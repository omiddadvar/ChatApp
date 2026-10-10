namespace ChatApp.API.Modules.Shared.Kernel.Abstractions;


public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}