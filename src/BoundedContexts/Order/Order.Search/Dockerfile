FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/BoundedContexts/Order/Order.Search/Order.Search.csproj", "src/BoundedContexts/Order/Order.Search/"]
COPY ["src/BoundedContexts/Order/Order.Contracts/Order.Contracts.csproj", "src/BoundedContexts/Order/Order.Contracts/"]
COPY ["src/BoundedContexts/Order/Order.Management/Order.Domain/Order.Domain.csproj", "src/BoundedContexts/Order/Order.Management/Order.Domain/"]
COPY ["src/BoundedContexts/Payment/Payment.Contracts/Payment.Contracts.csproj", "src/BoundedContexts/Payment/Payment.Contracts/"]
COPY ["src/BuildingBlocks/DDD.BuildingBlocks.Domain/DDD.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/DDD.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/DDD.BuildingBlocks.Integration/DDD.BuildingBlocks.Integration.csproj", "src/BuildingBlocks/DDD.BuildingBlocks.Integration/"]

# Restore dependencies
RUN dotnet restore "src/BoundedContexts/Order/Order.Search/Order.Search.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/BoundedContexts/Order/Order.Search"
RUN dotnet build "Order.Search.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Order.Search.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.Search.dll"]
