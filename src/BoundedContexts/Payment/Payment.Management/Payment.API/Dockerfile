FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/BoundedContexts/Payment/Payment.API/Payment.API.csproj", "src/BoundedContexts/Payment/Payment.API/"]
COPY ["src/BoundedContexts/Payment/Payment.Application/Payment.Application.csproj", "src/BoundedContexts/Payment/Payment.Application/"]
COPY ["src/BoundedContexts/Payment/Payment.Domain/Payment.Domain.csproj", "src/BoundedContexts/Payment/Payment.Domain/"]
COPY ["src/BoundedContexts/Payment/Payment.Infrastructure/Payment.Infrastructure.csproj", "src/BoundedContexts/Payment/Payment.Infrastructure/"]
COPY ["src/BoundedContexts/Payment/Payment.IntegrationEvents/Payment.IntegrationEvents.csproj", "src/BoundedContexts/Payment/Payment.IntegrationEvents/"]
COPY ["src/BoundedContexts/Order/Order.IntegrationEvents/Order.IntegrationEvents.csproj", "src/BoundedContexts/Order/Order.IntegrationEvents/"]
COPY ["src/BuildingBlocks/DDD.BuildingBlocks.Domain/DDD.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/DDD.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/DDD.BuildingBlocks.Integration/DDD.BuildingBlocks.Integration.csproj", "src/BuildingBlocks/DDD.BuildingBlocks.Integration/"]

# Restore dependencies
RUN dotnet restore "src/BoundedContexts/Payment/Payment.API/Payment.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/BoundedContexts/Payment/Payment.API"
RUN dotnet build "Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payment.API.dll"]
