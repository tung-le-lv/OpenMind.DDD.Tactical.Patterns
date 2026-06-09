FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/BoundedContexts/Order/Order.API/Order.API.csproj", "src/BoundedContexts/Order/Order.API/"]
COPY ["src/BoundedContexts/Order/Order.Application/Order.Application.csproj", "src/BoundedContexts/Order/Order.Application/"]
COPY ["src/BoundedContexts/Order/Order.Domain/Order.Domain.csproj", "src/BoundedContexts/Order/Order.Domain/"]
COPY ["src/BoundedContexts/Order/Order.Infrastructure/Order.Infrastructure.csproj", "src/BoundedContexts/Order/Order.Infrastructure/"]
COPY ["src/BoundedContexts/Order/Order.IntegrationEvents/Order.IntegrationEvents.csproj", "src/BoundedContexts/Order/Order.IntegrationEvents/"]
COPY ["src/BoundedContexts/Payment/Payment.IntegrationEvents/Payment.IntegrationEvents.csproj", "src/BoundedContexts/Payment/Payment.IntegrationEvents/"]
COPY ["src/BuildingBlocks/DDD.BuildingBlocks.Domain/DDD.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/DDD.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/DDD.BuildingBlocks.Integration/DDD.BuildingBlocks.Integration.csproj", "src/BuildingBlocks/DDD.BuildingBlocks.Integration/"]

# Restore dependencies
RUN dotnet restore "src/BoundedContexts/Order/Order.API/Order.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/BoundedContexts/Order/Order.API"
RUN dotnet build "Order.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Order.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.API.dll"]
