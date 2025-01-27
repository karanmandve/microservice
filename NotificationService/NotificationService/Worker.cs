using App.Core.Interface;
using Microsoft.Extensions.Options;
using NotificationService.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly IEmailService _emailService;
        private IConnection _connection;
        private IModel _channel;

        public Worker(ILogger<Worker> logger, IOptions<RabbitMQSettings> rabbitMQSettings, IEmailService emailService)
        {
            _logger = logger;
            _rabbitMQSettings = rabbitMQSettings.Value;
            _emailService = emailService;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _rabbitMQSettings.HostName,
                    UserName = _rabbitMQSettings.UserName,
                    Password = _rabbitMQSettings.Password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _rabbitMQSettings.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("RabbitMQ connection and channel initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing RabbitMQ.");
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => _logger.LogInformation("Stopping the service."));

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var email = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message: {Message}", email);

                try
                {

                    _logger.LogInformation("Email sent to {Email}", email);
                    _emailService.SendEmailAsync(email, "New Order Notification", $"Your Order Is Confirmed With Us");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the message.");
                }
            };

            _channel.BasicConsume(queue: _rabbitMQSettings.QueueName, autoAck: true, consumer: consumer);
            _logger.LogInformation("RabbitMQ consumer started.");

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            _logger.LogInformation("RabbitMQ connection and channel closed.");
            base.Dispose();
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Email { get; set; }

    }
}
