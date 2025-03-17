using System.Net.Mail;
using System.Net;
using System.Text;
using NotificationsAndAlerts.Application.Interfaces;
using NotificationsAndAlerts.Infrastructure.EventBus;
using DotNetEnv;
using NotificationsAndAlerts.Application.Handlers;
using NotificationsAndAlerts.Application.Services;
using NotificationsAndAlerts.Application.Configs;

Env.Load();
var builder = WebApplication.CreateBuilder(args);  
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection("mail"));
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddScoped<SendEmailNotificationHandler>();
builder.Services.AddSingleton<IEventBusConsumerAsync, EventBusConsumerAsync>();
builder.Services.AddSingleton<IEventBusProducer, EventBusProducer>();

builder.Services.AddHostedService<EventBusConsumerAsync>();
builder.Services.AddHostedService<EventBusProducer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }
app.MapGet("/", () => Results.Ok("Healthy"));

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();