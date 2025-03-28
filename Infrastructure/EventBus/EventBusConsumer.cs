﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.Handlers;
using TicketsMS.Application.Interfaces;
using TicketsMS.Application.Messages.Request;
using TicketsMS.Application.Messages.Response;
using TicketsMS.Application.Queues;
using TicketsMS.Application.Services;
using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;
using TicketsMS.Infrastructure.Data;
using TicketsMS.Infrastructure.Repository;

namespace TicketsMS.Infrastructure.EventBus
{
    public class EventBusConsumer : EventBusBase, IEventBusConsumer, IEventBusConsumerAsync
    {
    //    private IConnection _connection;
      //  private IChannel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RabbitMQSettings _rabbitmqSettings;
        private readonly Dictionary<string, Func<string, Task<string>>> _handlers;
        private readonly Dictionary<string, Func<string, Task>> _eventHandlers;
        private readonly ILogger<EventBusConsumer> _logger;
        private readonly IMapper _mapper;

        public EventBusConsumer(IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> options, ILogger<EventBusConsumer> logger, IMapper mapper) : base(options)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _handlers = new();
            _eventHandlers = new();
            _mapper = mapper;
            _logger = logger;
            InitializeAsync().GetAwaiter().GetResult();
        }

        public static async Task<EventBusConsumer> CreateAsync(IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> rabbitMQSettings, ILogger<EventBusConsumer> logger, IMapper mapper)
        {
            var instance = new EventBusConsumer(serviceScopeFactory, rabbitMQSettings, logger, mapper);
            await instance.InitializeAsync();
            return instance;
        }

        private async Task InitializeAsync()
        {
            await base.InitializeAsync();
            RegisterHandlers();
        }

        private async void RegisterHandlers()
        {
            _ = Task.Run(async () =>
            {
                //Queue to manage participant tickets creation 
                await RegisterEventHandlerAsync<GenerateParticipantsTicketRequest>(Queues.GENERATE_PARTICIPANTS_TICKETS_ASYNC, async (request) =>
                {
                    _logger.LogInformation($"Received request to generate participant tickets for tournament id {request.IdTournament}");
                    using var scope = _serviceScopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TicketsHandler>();
                    await handler.GenerateTicketsParticipantsAsync(request);
                });
                //Queue to manage ticket sale participant
                await RegisterEventHandlerAsync<GenerateTicketSale>(Queues.SELL_TICKET_PARTICIPANT, async (request) =>
                {
                    _logger.LogInformation($"Received request to generate ticket sale participant for user {request.IdUser}");
                    using var scope = _serviceScopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TicketsHandler>();

                    await handler.GenerateTicketSale(request);
                });

                await RegisterEventHandlerAsync<GenerateTicketSaleViewer>(Queues.SELL_TICKET_VIEWER, async (request) =>
                {
                    _logger.LogInformation($"Received request to generate ticket viewer and ticket sale {request.IdUser}");
                    using var scope = _serviceScopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TicketsHandler>();

                    await handler.GenerateTicketSaleViewer(request);
                });


                await RegisterEventHandlerAsync<int>(Queues.CHANGE_TICKETS_PARTICIPANT_USED, async (idTournament) =>
                {
                    _logger.LogInformation($"Received request to inactivate particpant tickets");
                    using var scope = _serviceScopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TicketsHandler>();

                    await handler.ChangeParticipantTicketsStatus(idTournament, TicketStatus.USED);
                });

                await RegisterEventHandlerAsync<int>(Queues.CHANGE_TICKETS_VIEWERS_USED, async (idmatch) =>
                {
                    _logger.LogInformation($"Received request to inactivate viewers tickets");
                    using var scope = _serviceScopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TicketsHandler>();

                    await handler.ChangeViewersTicketsStatus(idmatch, TicketStatus.USED);
                });
            });

            RegisterQueueHandler<int, GetTicketInfoResponse>(Queues.GET_TICKET_INFO, async (idInfo) =>
            {
                _logger.LogInformation($"Received request to get info ticket");
                using var scope = _serviceScopeFactory.CreateScope();

                var ticketService = scope.ServiceProvider.GetRequiredService<IRepository<Tickets>>();
                var ticketInfo = await ticketService.GetByIdAsync(idInfo);

                if(ticketInfo == null) return new GetTicketInfoResponse();

                return new GetTicketInfoResponse
                {
                    IdTicket = ticketInfo.Id,
                    Status = ticketInfo.Status,
                    Type = ticketInfo.Type,
                    IdTournament = ticketInfo?.IdTournament?? 0,
                };
            });

            RegisterQueueHandler<GetTicketUserTournament, bool>(Queues.VALIDATE_USER_HAS_TICKETS_TOURNAMENT, async (payload) =>
            {
                _logger.LogInformation($"Received request to validate user has tickets for tournament");
                using var scope = _serviceScopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
                var ticketSalesInfo = await (from ts in context.TicketSales
                                             join t in context.Tickets on ts.IdTicket equals t.Id
                                             where ts.IdUser == payload.IdUser && t.IdTournament == payload.IdTournament 
                                             select t).ToListAsync();
                //if user has already ticket sales confirmed for tournament
                if (ticketSalesInfo.Any()) return true;

                return false;

            });
        }

        public async Task PublishEventAsync<TEvent>(TEvent eventMessage, string queueName)
        {
            if (_connection == null || !_connection.IsOpen || _channel.IsClosed)
            {
                await InitializeAsync();
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
    }
}
