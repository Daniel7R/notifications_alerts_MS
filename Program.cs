using NotificationsAndAlerts.Infrastructure.Data;
using System.Net.Mail;
using System.Net;
using System.Text;
using NotificationsAndAlerts.Application.Interfaces;
using NotificationsAndAlerts.Infrastructure.EventBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IEventBusConsumerAsync, EventBusConsumerAsync>();
builder.Services.AddHostedService<EventBusConsumerAsync>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//Run();
app.Run();


static async Task Run()
{
    var smtpClient = new SmtpClient("smtp.gmail.com")
    {
        Port = 587,
        Credentials = new NetworkCredential("dr734659@gmail.com", "kmsm jpap lurd yyqm"),
        EnableSsl = true
    };

    var mailMessage = new MailMessage
    {
        From = new MailAddress("dr734659@gmail.com"),
        Subject = "Correo de prueba con Gmail",
        Body = "Este es un correo de prueba usando Gmail SMTP.",
        IsBodyHtml = false
    };

    mailMessage.To.Add("carlosriverarangel7@gmail.com");
    smtpClient.Send(mailMessage);

    Console.WriteLine("Correo enviado con éxito.");
}