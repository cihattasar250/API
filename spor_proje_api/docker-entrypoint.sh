#!/bin/sh

# Render'ın PORT değişkenini ASPNETCORE_URLS'e çevir
# Eğer PORT set edilmemişse varsayılan olarak 8080 kullan
PORT=${PORT:-8080}
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"

# .NET uygulamasını çalıştır
exec dotnet spor_proje_api.dll

