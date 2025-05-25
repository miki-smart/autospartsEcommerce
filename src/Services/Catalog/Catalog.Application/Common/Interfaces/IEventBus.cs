namespace Catalog.Application.Common.Interfaces
{
    // This is the application layer abstraction for the event bus
    // The actual implementation will be in the infrastructure layer
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
    }
}
