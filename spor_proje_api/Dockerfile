# 1) Runtime imajý (sadece yayýnlanmýþ dosyalarý çalýþtýracak)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2) Build imajý (kodu derleyip publish alacak)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Çözüm dosyasýný ve proje dosyalarýný kopyala
COPY ["spor_proje_api/spor_proje_api.csproj", "spor_proje_api/"]
RUN dotnet restore "spor_proje_api/spor_proje_api.csproj"

# Geri kalan her þeyi kopyala
COPY . .
WORKDIR "/src/spor_proje_api"

# Release publish
RUN dotnet publish "spor_proje_api.csproj" -c Release -o /app/publish

# 3) Final imaj
FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

# Render'ýn verdiði PORT'tan dinle
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT ["dotnet", "spor_proje_api.dll"]
