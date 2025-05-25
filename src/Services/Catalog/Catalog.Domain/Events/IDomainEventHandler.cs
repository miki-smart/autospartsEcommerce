namespace Catalog.Domain.Events
{
    // Interface for domain event handlers
    public interface IDomainEventHandler<in TEvent> where TEvent : DomainEvent
    {
        Task Handle(TEvent @event);
    }
}
