using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace TicketsMS.Infrastructure.EventBus
{
    public class EventBusBase : BackgroundService, IAsyncDisposable
    {
        protected IConnection _connection;
        protected IChannel _channel;
        private readonly RabbitMQSettings _rabbitmqSettings;

        protected EventBusBase(IOptions<RabbitMQSettings> options)
        {
            _rabbitmqSettings = options.Value;
        }

        protected async Task InitializeAsync()
        {
            // var basePath = AppContext.BaseDirectory;
            // var pfxCertPath = Path.Combine(basePath, "Infrastructure", "Security", _rabbitmqSettings.CertFile);

            // if (!File.Exists(pfxCertPath))
            // {
            //     throw new FileNotFoundException("PFX certificate not found");
            // }

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
                 VirtualHost= "ngimsipu"
                // Ssl = new SslOption
                // {
                //     Enabled = true,
                //     ServerName = _rabbitmqSettings.ServerName,
                //     CertPath = pfxCertPath,
                //     CertPassphrase = _rabbitmqSettings.CertPassphrase,
                //     Version = System.Security.Authentication.SslProtocols.Tls12
                // }
            };

            while (_connection == null || !_connection.IsOpen || _channel == null || _channel.IsClosed)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.DisposeAsync();
            }
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
                {
                    await InitializeAsync();
                }

                await Task.Delay(500, stoppingToken);
            }
        }
    }
}
