# Stage 1: Build with .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Copy solution and project files
COPY SchoolAdmission.sln .
COPY SchoolAdmission.Api/*.csproj ./SchoolAdmission.Api/
COPY SchoolAdmission.Tests/*.csproj ./SchoolAdmission.Tests/

RUN dotnet restore SchoolAdmission.Api/SchoolAdmission.Api.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/SchoolAdmission.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health check
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SchoolAdmission.Api.dll"]
