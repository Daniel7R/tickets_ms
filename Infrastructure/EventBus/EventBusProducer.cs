using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using TicketsMS.Application.Interfaces;

namespace TicketsMS.Infrastructure.EventBus
{
    public class EventBusProducer : EventBusBase, IEventBusProducer
    {
        public EventBusProducer(IOptions<RabbitMQSettings> option) : base(option)
        {
            InitializeAsync().GetAwaiter().GetResult();
        }
        public async Task PublishEventAsync<TEvent>(TEvent eventMessage, string queueName)
        {
            if (_connection == null || !_connection.IsOpen || _channel.IsClosed)
            {
                await base.InitializeAsync();
            }

            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventMessage));
            var props = new BasicProperties
            {
                //in case rabbitmq is restarted
                Persistent = true
            };

            await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, mandatory: false, basicProperties: props, body: messageBytes);

        }

        public async Task<TResponse> SendRequest<TResquest, TResponse>(TResquest request, string queueName)
        {
            if (_connection == null || !_connection.IsOpen || _channel.IsClosed)
            {
                await base.InitializeAsync();
            }

            await _channel.QueueDeclareAsync(queue: queueName,
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);
            var replyQueue = await _channel.QueueDeclareAsync();
            var replyQueueName = replyQueue.QueueName;
            try
            {

                var correlationId = Guid.NewGuid().ToString();

                var props = new BasicProperties
                {
                    CorrelationId = correlationId,
                    ReplyTo = replyQueueName
                };

                var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

                await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, mandatory: false, basicProperties: props, body: messageBytes);

                var tcs = new TaskCompletionSource<TResponse>();

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var response = JsonConvert.DeserializeObject<TResponse>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                        tcs.SetResult(response);
                    }
                };

                await _channel.BasicConsumeAsync(consumer: consumer, queue: replyQueueName, autoAck: false);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                cts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

                return await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                throw new Exception($"Error: timeout processing at queue. Check if the service is working or the queue is working correctly.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in queue messages: {ex.Message}");
            }
        }
    }
}
