namespace TicketsMS.Infrastructure.EventBus
{
    public class RabbitMQSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CertPassphrase { get; set; }
        public string CertFile { get; set; }
        public string ServerName { get; set; }
    }
}
