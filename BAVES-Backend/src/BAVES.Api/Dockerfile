# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["src/BAVES.Api/BAVES.Api.csproj", "src/BAVES.Api/"]
RUN dotnet restore "src/BAVES.Api/BAVES.Api.csproj"

# Copy full API source code
COPY ["src/BAVES.Api/", "src/BAVES.Api/"]
WORKDIR "/src/src/BAVES.Api"
RUN dotnet publish "BAVES.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Cài đặt curl để hỗ trợ docker healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .

HEALTHCHECK --interval=10s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "BAVES.Api.dll"]
