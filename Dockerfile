# .NET 8 SDK image - build için
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# .csproj dosyasını kopyala ve restore et
COPY ["spor_proje_api/spor_proje_api.csproj", "spor_proje_api/"]
RUN dotnet restore "spor_proje_api/spor_proje_api.csproj"

# Tüm dosyaları kopyala ve build et
COPY . .
WORKDIR "/src/spor_proje_api"
RUN dotnet build "spor_proje_api.csproj" -c Release -o /app/build

# Publish işlemi
FROM build AS publish
RUN dotnet publish "spor_proje_api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image - çalışma zamanı için
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Publish edilmiş dosyaları kopyala
COPY --from=publish /app/publish .

# Entrypoint script'ini kopyala ve çalıştırılabilir yap
COPY spor_proje_api/docker-entrypoint.sh /app/
RUN chmod +x /app/docker-entrypoint.sh

# Port exposure (Render otomatik olarak port'u ayarlar)
EXPOSE 8080

# Entry point - Render'ın PORT değişkenini kullanmak için script kullanıyoruz
ENTRYPOINT ["/app/docker-entrypoint.sh"]
