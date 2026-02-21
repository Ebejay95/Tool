# Build + runtime image for CMC.Web (.NET 8)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + projects first for better layer caching
COPY src/CMC.sln ./src/CMC.sln
COPY src/CMC.Web/CMC.Web.csproj ./src/CMC.Web/CMC.Web.csproj
COPY src/CMC.Persistence/CMC.Persistence.csproj ./src/CMC.Persistence/CMC.Persistence.csproj
COPY src/CMC.Notifications.Abstractions/CMC.Notifications.Abstractions.csproj ./src/CMC.Notifications.Abstractions/CMC.Notifications.Abstractions.csproj
COPY src/CMC.Notifications.Socket/CMC.Notifications.Socket.csproj ./src/CMC.Notifications.Socket/CMC.Notifications.Socket.csproj
COPY src/Modules/Todos/CMC.Todos.Domain/CMC.Todos.Domain.csproj ./src/Modules/Todos/CMC.Todos.Domain/CMC.Todos.Domain.csproj
COPY src/Modules/Todos/CMC.Todos.Application/CMC.Todos.Application.csproj ./src/Modules/Todos/CMC.Todos.Application/CMC.Todos.Application.csproj
COPY src/Modules/Todos/CMC.Todos.Infrastructure/CMC.Todos.Infrastructure.csproj ./src/Modules/Todos/CMC.Todos.Infrastructure/CMC.Todos.Infrastructure.csproj

RUN dotnet restore ./src/CMC.sln

# Copy the rest of the source
COPY src ./src

RUN dotnet publish ./src/CMC.Web/CMC.Web.csproj -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /out ./

ENTRYPOINT ["dotnet", "CMC.Web.dll"]
