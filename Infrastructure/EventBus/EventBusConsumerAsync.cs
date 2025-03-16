using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NotificationsAndAlerts.Application.Handlers;
using NotificationsAndAlerts.Application.Interfaces;
using NotificationsAndAlerts.Application.Messages;
using NotificationsAndAlerts.Application.Queues;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationsAndAlerts.Infrastructure.EventBus
{
    public class EventBusConsumerAsync : EventBusBase, IEventBusConsumerAsync
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMQSettings _rabbitmqSettings;
        private readonly ILogger<EventBusConsumerAsync> _logger;
        private readonly Dictionary<string, Func<string, Task>> _eventHandlers;

        public EventBusConsumerAsync(IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> options, ILogger<EventBusConsumerAsync> logger): base(options)
        {
            _rabbitmqSettings = options.Value;
            _scopeFactory = serviceScopeFactory;
            _eventHandlers = new();
            _logger = logger;
            InitializeAsync().GetAwaiter().GetResult();
        }

        public static async Task<EventBusConsumerAsync> CreateAsync(IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> rabbitMQSettings, ILogger<EventBusConsumerAsync> logger)
        {
            var instance = new EventBusConsumerAsync(serviceScopeFactory, rabbitMQSettings, logger);
            await instance.InitializeAsync();
            return instance;
        }

        private async Task InitializeAsync()
        {
            await base.InitializeAsync();

            RegisterHandlers();
        }
        private void RegisterHandlers()
        {
            _ = Task.Run(async () =>
            {
                //send email notification 
                await RegisterEventHandlerAsync<EmailNotificationRequest>(Queues.SEND_EMAIL_NOTIFICATION_SALE, async (payload) =>
                {
                    using var scope = _scopeFactory.CreateScope();

                    var handler = scope.ServiceProvider.GetRequiredService<SendEmailNotificationHandler>();

                    await handler.HandleAsync(payload);

                    //_logger.LogInformation($"Received request for sending email {payload}");
                });

                await RegisterEventHandlerAsync<EmailNotificationRequest>(Queues.SEND_EMAIL_NOTIFICATION_PAYMENT, async(payload) =>
                {
                    using var scope = _scopeFactory.CreateScope();

                    var handler = scope.ServiceProvider.GetRequiredService<SendEmailNotificationHandler>();

                    await handler.HandleAsync(payload);
                });

                await RegisterEventHandlerAsync<EmailNotificationRequest>(Queues.SEND_EMAIL_DONATION, async (payload) => {
                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<SendEmailNotificationHandler>();

                    await handler.HandleAsync(payload);
                });
                //email to send info when tournament is created
                await RegisterEventHandlerAsync<EmailBulkNotificationRequest>(Queues.SEND_EMAIL_CREATE_TOURNAMENT, async (payload) =>
                {
                    using var scope = _scopeFactory.CreateScope();

                    var handler = scope.ServiceProvider.GetRequiredService<SendEmailNotificationHandler>();
                    await handler.HandleBulk(payload);

                });

                await RegisterEventHandlerAsync<EmailBulkNotificationRequest>(Queues.REMINDER, async (payload) =>
                {
                    using var scope = _scopeFactory.CreateScope();

                    var handler = scope.ServiceProvider.GetRequiredService<SendEmailNotificationHandler>();
                    await handler.HandleBulk(payload);

                });

                await RegisterEventHandlerAsync<EmailBulkNotificationRequest>(Queues.SEND_EMAIL_UPDATE_TOURNAMENT, async (payload) =>
                {
                    using var scope = _scopeFactory.CreateScope();

                    var handler = scope.ServiceProvider.GetRequiredService<SendEmailNotificationHandler>();
                    await handler.HandleBulk(payload);

                });

                await RegisterEventHandlerAsync<EmailBulkNotificationRequest>(Queues.SEND_EMAIL_MATCH_WINNER, async (payload) =>
                {
                    using var scope = _scopeFactory.CreateScope();

                    var handler = scope.ServiceProvider.GetRequiredService<SendEmailNotificationHandler>();
                    await handler.HandleBulk(payload);

                });

            });
        }

        public async Task RegisterEventHandlerAsync<TEvent>(string queueName, Func<TEvent, Task> handler)
        {
            EnsureConnection();

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
                    if(_eventHandlers.TryGetValue(ea.RoutingKey, out var handlerAsync))
                    {
                        await handlerAsync(message);
                        _logger.LogInformation($"info {JsonConvert.SerializeObject(message)}");
                    }
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);

                } catch(Exception ex)
                {
                    _logger.LogError($"Error registering event: {ex.Message}");
                    if (!_connection.IsOpen) await InitializeAsync();

                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }

            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        }

        private async void EnsureConnection()
        {
            if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
                await InitializeAsync();
        }
    }
}
