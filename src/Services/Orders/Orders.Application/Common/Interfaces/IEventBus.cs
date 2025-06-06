namespace Orders.Application.Common.Interfaces
{
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
    }
}
