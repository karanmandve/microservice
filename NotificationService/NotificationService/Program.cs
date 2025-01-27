using App.Core.Interface;
using NotificationService;
using NotificationService.Model;
using NotificationService.Service;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMQSettings>(context.Configuration.GetSection("RabbitMQ"));
        services.Configure<EmailSettings>(context.Configuration.GetSection("Smtp"));
        //services.Configure<EmailSettings>(context.Configuration.GetSection("Smtp"));
        services.AddSingleton<IEmailService, EmailService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
