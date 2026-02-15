# ======================
# Runtime
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV TZ=UTC

EXPOSE 8080
EXPOSE 8081

# ======================
# Build
# ======================
FROM mcr.microsoft.com/dotnet/sdk:9.0.200 AS build
WORKDIR /src

COPY MussicApp.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet build -c Release -o /app/build

# ======================
# Publish
# ======================
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ======================
# Final
# ======================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MussicApp.dll"]
