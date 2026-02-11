# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file
COPY ["LateralChallenge.slnx", "./"]

# Copy project files
COPY ["Client/Client.csproj", "Client/"]
COPY ["Manager/Manager.csproj", "Manager/"]
COPY ["Engine/Engine.csproj", "Engine/"]
COPY ["Resources/Resources.csproj", "Resources/"]
COPY ["Database/Database.csproj", "Database/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

# Restore dependencies
RUN dotnet restore "LateralChallenge.slnx"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/Client"
RUN dotnet build "Client.csproj" -c Release -o /app/build

# =========================
# Publish stage
# =========================
FROM build AS publish
RUN dotnet publish "Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser /app
USER appuser

# Copy published output
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Optional health check (adjust endpoint if needed)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Client.dll"]
