FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# config listen port
ENV ASPNETCORE_URLS=http://+:8081 

ARG APISettings__JwtOptions__Secret
ARG APISettings__JwtOptions__Issuer
ARG APISettings__JwtOptions__Audience
ARG RabbitMQ__Host
ARG RabbitMQ__Port
ARG RabbitMQ__Username
ARG RabbitMQ__Password
ARG mail__PORT
ARG mail__SMTP_CLIENT
ARG mail__USER_EMAIL
ARG mail__PASSWORD_APP

ENV APISettings__JwtOptions__Secret=${APISettings__JwtOptions__Secret}
ENV APISettings__JwtOptions__Issuer=${APISettings__JwtOptions__Issuer}
ENV APISettings__JwtOptions__Audience=${APISettings__JwtOptions__Audience}
ENV RabbitMQ__Host=${RabbitMQ__Host}
ENV RabbitMQ__Port=${RabbitMQ__Port}
ENV RabbitMQ__Username=${RabbitMQ__Username}
ENV RabbitMQ__Password=${RabbitMQ__Password}
ENV mail__PORT=${mail__PORT}
ENV mail__SMTP_CLIENT=${mail__SMTP_CLIENT}
ENV mail__USER_EMAIL=${mail__USER_EMAIL}
ENV mail__PASSWORD_APP=${mail__PASSWORD_APP}

RUN echo "Variables cargadas: $RabbitMQ__Host, $mail__PASSWORD_APP"

EXPOSE 8081
# EXPOSE 8080 
# EXPOSE 8081 
# EXPOSE 5089

CMD ["dotnet", "NotificationsAndAlerts.dll"]
