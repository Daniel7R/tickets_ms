using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.Interfaces;
using TicketsMS.Application.Messages.Request;
using TicketsMS.Application.Queues;
using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;
using TicketsMS.Infrastructure.Data;
using TicketsMS.Infrastructure.Repository;

namespace TicketsMS.Infrastructure.EventBus
{
    public class EventBusConsumer : BackgroundService, IEventBusConsumer, IEventBusConsumerAsync, IAsyncDisposable
    {
        private IConnection _connection;
        private IChannel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RabbitMQSettings _rabbitmqSettings;
        private readonly Dictionary<string, Func<string, Task<string>>> _handlers;
        private readonly Dictionary<string, Func<string, Task>> _eventHandlers;
        private readonly ILogger<EventBusConsumer> _logger;
        private readonly IMapper _mapper;

        public EventBusConsumer(IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> options, ILogger<EventBusConsumer> logger, IMapper mapper)
        {
            _rabbitmqSettings = options.Value;
            _serviceScopeFactory = serviceScopeFactory;
            _handlers = new();
            _eventHandlers = new();
            _mapper = mapper;
            _logger = logger;
        }

        public static async Task<EventBusConsumer> CreateAsync(IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> rabbitMQSettings, ILogger<EventBusConsumer> logger, IMapper mapper)
        {
            var instance = new EventBusConsumer(serviceScopeFactory, rabbitMQSettings, logger, mapper);
            await instance.InitializeAsync();
            return instance;
        }

        private async Task InitializeAsync()
        {
            var basePath = AppContext.BaseDirectory;
            var pfxCerPath = Path.Combine(basePath, "Infrastructure", "Security", _rabbitmqSettings.CertFile);
            if (!File.Exists(pfxCerPath)) throw new FileNotFoundException("PFX certificate not found");

            var factory = new ConnectionFactory
            {
                HostName = _rabbitmqSettings.Host,
                UserName = _rabbitmqSettings.Username,
                Password = _rabbitmqSettings.Password,
                Port = _rabbitmqSettings.Port,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                RequestedHeartbeat = TimeSpan.FromSeconds(30),
                ContinuationTimeout = TimeSpan.FromSeconds(30),
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = _rabbitmqSettings.ServerName,
                    CertPath = pfxCerPath,
                    CertPassphrase = _rabbitmqSettings.CertPassphrase,
                    Version = System.Security.Authentication.SslProtocols.Tls12
                }
            };

            while (_connection == null || !_connection.IsOpen || _channel == null || _channel.IsClosed)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();
                }
                catch (Exception ex)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            _ = Task.Run(async () =>
            {
                await RegisterEventHandlerAsync<GenerateParticipantsTicketRequest>(Queues.GENERATE_PARTICIPANTS_TICKETS_ASYNC, async (request) =>
                {
                    _logger.LogInformation($"Received request to generate participant tickets for tournament id {request.IdTournament}");
                    using var scope = _serviceScopeFactory.CreateScope();
                    var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TicketDbContext>();

                    using var transaction = await dbContext.Database.BeginTransactionAsync();


                    try
                    {
                        var quantity = request.QuantityTickets;
                        var priceTicket = request.IsFree ? 0 : TicketPrices.PARTICIPANT;
                        var tasks = Enumerable.Range(0, quantity)
                            .Select(async _ =>
                            {
                                var ticketResponse = await ticketService.GenerateTicketParticipant(request.IdTournament, request.IsFree, priceTicket);
                                dbContext.Tickets.Add(ticketResponse);
                            }).ToList();
                        await Task.WhenAll(tasks);

                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error generation tickets: {ex.Message}");
                        await transaction.RollbackAsync();
                    }
                    /*
                    var tasks = Enumerable.Range(0, quantity)
                        .Select(_ => Task.Run(async () =>
                        {
                            var ticketResponse = await ticketService.GenerateTicketParticipant(request.IdTournament, request.IsFree, priceTicket);
                            //var ticketConverted = _mapper.Map<TicketRequestDTO>(ticketResponse);
                            //await ticketService.CreateTicketAsync(ticketConverted);
                            await ticketService.CreateTicketAsync(ticketResponse);
                        })).ToList();
                    await Task.WhenAll(tasks);

                    await Parallel.ForEachAsync(Enumerable.Range(0, quantity), async (i, _) =>
                    {
                        var ticketResponse = await ticketService.GenerateTicketParticipant(request.IdTournament, request.IsFree, priceTicket);
                        //
                        var ticketConverted = _mapper.Map<TicketRequestDTO>(ticketResponse);
                        await ticketService.CreateTicketAsync(ticketConverted);
                    });*/
                });
            });
            //Queue to manage participant tickets creation 

        }
        /// <summary>
        ///     Register a Request/Reply queue manager
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="handler"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RegisterQueueHandler<TRequest, TResponse>(string queueName, Func<TRequest, Task<TResponse>> handler)
        {
            if (_channel == null) throw new InvalidOperationException("EventBusRabbitMQ is not initialized");
            if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
            {
                Task.Run(InitializeAsync).GetAwaiter().GetResult();
            }

            _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, null).Wait();
            _channel.BasicQosAsync(0, 1, false);

            _handlers[queueName] = async (message) =>
            {
                var request = JsonConvert.DeserializeObject<TRequest>(message);
                var response = await handler(request);
                return JsonConvert.SerializeObject(response);
            };

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                _logger.LogInformation($"Processing request at queue: ${queueName}");
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var replyProps = new BasicProperties
                {
                    CorrelationId = ea.BasicProperties.CorrelationId,
                };

                try
                {
                    if (_handlers.TryGetValue(ea.RoutingKey, out var reqHandler))
                    {
                        var responseMessage = await reqHandler(message);
                        var responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                        await _channel.BasicPublishAsync(exchange: "", routingKey: ea.BasicProperties.ReplyTo, mandatory: false, basicProperties: replyProps, body: responseBytes);
                    }
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error rocessing request at queue: ${queueName}");
                    if (!_connection.IsOpen) await InitializeAsync();
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer).Wait();
        }

        /// <summary>
        ///     Register an async Event queue manager
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="handler"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RegisterEventHandlerAsync<TEvent>(string queueName, Func<TEvent, Task> handler)
        {
            if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
            {
                await InitializeAsync();
            }

            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, null);
            await _channel.BasicQosAsync(0, 1, false);

            _eventHandlers[queueName] = async (message) =>
            {
                var @event = JsonConvert.DeserializeObject<TEvent>(message);
                await handler(@event);
            };

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    if (_eventHandlers.TryGetValue(ea.RoutingKey, out var handlerAsync))
                    {
                        await handlerAsync(message);
                    }
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);

                }
                catch (Exception ex)
                {
                    if (!_connection.IsOpen)
                    {
                        await InitializeAsync();
                    }
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };
            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        }

        private void EnsureConnection()
        {
            if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
            {
                Task.Run(InitializeAsync).GetAwaiter().GetResult();
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_connection == null || !_connection.IsOpen || _channel == null || _channel.IsClosed) await InitializeAsync();

                try
                {
                    await Task.Delay(500, stoppingToken);
                }
                catch (Exception ex)
                {
                    await InitializeAsync();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null) await _connection.DisposeAsync();
            if (_channel != null) await _channel.DisposeAsync();
        }
    }
}
