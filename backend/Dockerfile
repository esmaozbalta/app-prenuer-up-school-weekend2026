# Archi API — production image for Render / any Docker host
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Archi.Api/Archi.Api.csproj Archi.Api/
RUN dotnet restore Archi.Api/Archi.Api.csproj

COPY Archi.Api/ Archi.Api/
RUN dotnet publish Archi.Api/Archi.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# SkiaSharp share-card rendering on Linux
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Render injects PORT; ASP.NET Core reads it when ASPNETCORE_URLS is unset
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "Archi.Api.dll"]
