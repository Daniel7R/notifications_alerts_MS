# NotificationsAndAlerts
Este proyecto es el encargado de generar enviar y procesar notificaciones a los usuarios del sistema.

# Requisitos Previos
- .NET 8 SDK
- RabbitMQ Server
- Configuracion SMTP para envio de correos con Gmail


## Estructura

- **Application/Configs/**: configuraciones para el cliente SMTP
- **Application/Handlers/**: manejadores para el procesamiento de eventos
- **Application/Services/**: implementaciones de los procesamientos de los eveentos
- **Infrastructure/EventBus/**: contiene las configuraciones de los consumos de y procesamiento de eventos
- **Program.cs**: punto de entrada del proyecto

# Instrucciones de Ejecución
Para ejecutar el proyecto UsersAuthorization, sigue estos pasos:

- Configura las variables de entorno necesarias o modifica los archivos appsettings.json, según sea necesario.
- Navega al directorio del proyecto UsersAuthorization.
- Ejecuta el siguiente comando para aplicar las migraciones de la base de datos: `dotnet ef database update`
- Ejecuta el siguiente comando para iniciar el proyecto: `dotnet run`

Esto iniciará el proyecto y estará listo para poder ser usado.


## RabbitMQ/LavinMQ
En el proyecto se hace uso de RabbitMQ como Message Broker para procesamiento de eventos sincronos con el patron de integracion Request/Reply. 

### Colas Procesamiento Asincronico (Async Queues)

- `ticket.sale.notification`: para procesar el envio de la informacion de la venta del ticket por medio del correo al usuario que compro un ticket

- `payment.notification`: para procesar el envio de una notificacion cuando un usuario realizo o no el pago de manera oportuna

- `donation.email`: para procesar informacion con respecto a donaciones, ya sean exitosas o no

- `tournament.created`: para enviar correo masivo a todos los usuarios dentro del sistema sobre la creacion de un nuevo correo(informativo)

- `tournament.update`: para enviar correo masivo a todos los usuarios dentro del sistema sobre actualizaciones en lo que respecta a los torneos

- `matches.tournament.reminder`: para enviar correos masivos de recordatorio cada dia, en caso de que hayan partidos el dia actual, la tarea esta configurada para ejecutarse cada dia a las 00:01 en un background job, que consulta dichos partidos del dia, y envia reminder

- `matches.tournament.reminder`: para enviar correo cuando en un partido se establece un ganador de dicho partido


### Colas de procesamiento Sincron(Reques/Reply)

- `users.emails`: para consultar todos los correos del usuario en el sistema y proceder a realizar el envio masivo de correos

- `user.by_id`: para consultar la info(correo y id) de un usuario para enviar correos individuales