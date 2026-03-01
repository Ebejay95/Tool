# Build + runtime image for Api (.NET 8)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency resolution
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Client/Client.csproj", "src/Client/"]
COPY ["src/Shared/SharedKernel/SharedKernel.csproj", "src/Shared/SharedKernel/"]
COPY ["src/Shared/Notifications.Abstractions/Notifications.Abstractions.csproj", "src/Shared/Notifications.Abstractions/"]
COPY ["src/Modules/Notifications/Notifications.Domain/Notifications.Domain.csproj", "src/Modules/Notifications/Notifications.Domain/"]
COPY ["src/Modules/Notifications/Notifications.Application/Notifications.Application.csproj", "src/Modules/Notifications/Notifications.Application/"]
COPY ["src/Modules/Notifications/Notifications.Infrastructure/Notifications.Infrastructure.csproj", "src/Modules/Notifications/Notifications.Infrastructure/"]
COPY ["src/Modules/Notifications/Notifications.Api/Notifications.Api.csproj", "src/Modules/Notifications/Notifications.Api/"]
COPY ["src/Modules/Identity/Identity.Domain/Identity.Domain.csproj", "src/Modules/Identity/Identity.Domain/"]
COPY ["src/Modules/Identity/Identity.Application/Identity.Application.csproj", "src/Modules/Identity/Identity.Application/"]
COPY ["src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/Modules/Identity/Identity.Infrastructure/"]
COPY ["src/Modules/Identity/Identity.Api/Identity.Api.csproj", "src/Modules/Identity/Identity.Api/"]
COPY ["src/Modules/Todos/Todos.Domain/Todos.Domain.csproj", "src/Modules/Todos/Todos.Domain/"]
COPY ["src/Modules/Todos/Todos.Application/Todos.Application.csproj", "src/Modules/Todos/Todos.Application/"]
COPY ["src/Modules/Todos/Todos.Infrastructure/Todos.Infrastructure.csproj", "src/Modules/Todos/Todos.Infrastructure/"]
COPY ["src/Modules/Todos/Todos.Api/Todos.Api.csproj", "src/Modules/Todos/Todos.Api/"]

# Restore dependencies
RUN dotnet restore "src/Api/Api.csproj"

# Copy the rest of the source
COPY src ./src

# Build and publish the application
RUN dotnet publish "./src/Api/Api.csproj" -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /out ./

ENTRYPOINT ["dotnet", "Api.dll"]
