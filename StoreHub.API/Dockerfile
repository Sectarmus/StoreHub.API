# Aşama 1: Runtime Base
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Aşama 2: SDK Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# NuGet paketlerini geri yükle (Katman önbellekleme optimizasyonu)
COPY ["StoreHub.API.csproj", "./"]
RUN dotnet restore "./StoreHub.API.csproj"

# Proje dosyalarını kopyala ve derle
COPY . .
WORKDIR "/src/."
RUN dotnet build "StoreHub.API.csproj" -c Release -o /app/build

# Aşama 3: Publish
FROM build AS publish
RUN dotnet publish "StoreHub.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Aşama 4: Final Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StoreHub.API.dll"]
