FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["spor_proje_api/spor_proje_api.csproj", "spor_proje_api/"]
RUN dotnet restore "spor_proje_api/spor_proje_api.csproj"
COPY . .
WORKDIR "/src/spor_proje_api"
RUN dotnet publish "spor_proje_api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "spor_proje_api.dll"]
