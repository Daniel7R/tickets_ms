namespace TicketsMS.Application.Interfaces
{
    public interface IEventBusProducer
    {
        Task PublishEventAsync<TEvent>(TEvent eventMessage, string queueName);
        Task<TResponse> SendRequest<TResquest, TResponse>(TResquest resquest, string queueName);
    }
}
