FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/InventoryConsumer

# Copy everything first
COPY . .

# Restore and build
RUN dotnet restore InventoryConsumer.csproj
RUN dotnet publish -c Release -o /app/consumer

FROM base AS final
WORKDIR /app
COPY --from=build /app/consumer ./
ENTRYPOINT ["dotnet", "InventoryConsumer.dll"]
