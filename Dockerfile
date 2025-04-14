# Dockerfile
# Use .NET SDK for build
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["src/InventoryConsumer/InventoryConsumer.csproj", "InventoryConsumer/"]
RUN dotnet restore "InventoryConsumer/InventoryConsumer.csproj"

COPY ["src/InventoryProducer/InventoryProducer.csproj", "InventoryProducer/"]
RUN dotnet restore "InventoryProducer/InventoryProducer.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR "/src/src/InventoryConsumer"
RUN dotnet build "InventoryConsumer.csproj" -c Release -o /app/consumer

WORKDIR "/src/src/InventoryProducer"
RUN dotnet build "InventoryProducer.csproj" -c Release -o /app/producer

FROM build AS publish

# Publish the applications
RUN dotnet publish "InventoryConsumer.csproj" -c Release -o /app/consumer /p:UseAppHost=false
RUN dotnet publish "InventoryProducer.csproj" -c Release -o /app/producer /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/consumer ./consumer
COPY --from=build /app/producer ./producer

# NOTE: Entry point defined in docker-compose.yml
