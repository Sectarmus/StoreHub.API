# AŞAMA 1: BASE (Sadece Çalıştırma Katmanı - "Garson")
# Bu sadece hafif bir Linux barındırır (.NET 10 ASPNET). Kod derleyemez, sadece paketli kodları çalıştırır. Canlı (Production) sunucuda sadece bu katman kullanılacak.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# AŞAMA 2: BUILD (Mutfak / Derleme Katmanı - "Aşçı")
# Koca .NET 10 SDK'sını kapsar (Ağırdır). Sadece projeyi derlemek için (geçici) ayağa kalkar.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Cache (Önbellek) tuzağı: Önce YALNIZCA .csproj'u çekeriz ki kütüphaneleri (NuGet) bir kere indirip ezberlesin.
COPY ["StoreHub.API.csproj", "./"]
RUN dotnet restore "./StoreHub.API.csproj"

# Sonra .csproj harici tüm kodu (.cs vb.) hıphızlı Mutfağa kopyalayıp Release modunda "Derle (Build)" diyoruz.
COPY . .
WORKDIR "/src/."
RUN dotnet build "StoreHub.API.csproj" -c Release -o /app/build

# AŞAMA 3: PUBLISH (Paketleme)
# Derlenen kodu, tüm fuzuli C# (.cs, obj, bin vb.) çöplerinden arındırıp paketler (Publish). Sadece temiz .dll dosyası kalır.
FROM build AS publish
RUN dotnet publish "StoreHub.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# AŞAMA 4: FINAL (Kargo Durumu)
# O temiz "Publish" edilmiş .dll dosyasını, ilk baştaki "Garson" (Base) Linux'una verir ve koca "Aşçı" (Build) Linux'unu siler çöpe atar!
# (Buna "Multi-Stage Build" denir. Docker image'in 1 GB yerine 100 MB olmasını sağlar, şirketlerde buna BAYILIRLAR!)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StoreHub.API.dll"]
