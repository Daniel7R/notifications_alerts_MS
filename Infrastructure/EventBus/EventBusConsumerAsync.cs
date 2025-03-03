using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NotificationsAndAlerts.Application.Interfaces;
using NotificationsAndAlerts.Application.Messages;
using NotificationsAndAlerts.Application.Queues;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationsAndAlerts.Infrastructure.EventBus
{
    public class EventBusConsumerAsync : BackgroundService, IEventBusConsumerAsync, IAsyncDisposable
    {
        private IConnection _connection;
        private IChannel _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMQSettings _rabbitmqSettings;
        private readonly ILogger<EventBusConsumerAsync> _logger;
        private readonly Dictionary<string, Func<string, Task>> _eventHandlers;

        public EventBusConsumerAsync(IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> options, ILogger<EventBusConsumerAsync> logger)
        {
            _rabbitmqSettings = options.Value;
            _scopeFactory = serviceScopeFactory;
            _eventHandlers = new();
            _logger = logger;
        }

        public static async Task<EventBusConsumerAsync> CreateAsync(IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> rabbitMQSettings, ILogger<EventBusConsumerAsync> logger)
        {
            var instance = new EventBusConsumerAsync(serviceScopeFactory, rabbitMQSettings, logger);
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
                    _logger.LogError($" Error with initialize async {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            RegisterHandlers();
        }
        private void RegisterHandlers()
        {
            _ = Task.Run(async () =>
            {
                await RegisterEventHandlerAsync<EmailNotificationRequest>(Queues.SEND_EMAIL_NOTIFICATION, async (payload) =>
                {
                    _logger.LogInformation($"Received request for sending email {payload}");

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
                    _logger.LogError($"Error executing async {ex.Message}");
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
