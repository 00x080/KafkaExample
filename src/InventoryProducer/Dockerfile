FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/InventoryProducer

# Copy everything first
COPY . .

# Restore and build
RUN dotnet restore InventoryProducer.csproj
RUN dotnet publish -c Release -o /app/producer

FROM base AS final
WORKDIR /app
COPY --from=build /app/producer ./
ENTRYPOINT ["dotnet", "InventoryProducer.dll"]