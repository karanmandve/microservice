using System.Text;
using Microsoft.Extensions.Options;
using OrderService.Model;
using RabbitMQ.Client;

public class RabbitMQPublisher
{
    private readonly IModel _channel;

    public RabbitMQPublisher(IOptions<RabbitMQSettings> options)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.Value.HostName,
            UserName = options.Value.UserName,
            Password = options.Value.Password
        };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        _channel.QueueDeclare(queue: "orderQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Publish(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: "orderQueue", basicProperties: null, body: body);
    }
}
