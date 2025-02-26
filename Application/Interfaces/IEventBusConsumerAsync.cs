namespace TicketsMS.Application.Interfaces
{
    public interface IEventBusConsumerAsync
    {
        Task RegisterEventHandlerAsync<TEvent>(string queueName, Func<TEvent, Task> handler);
    }
}
