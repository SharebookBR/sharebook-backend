# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy source code
COPY ShareBook/ ./ShareBook/

# Restore and build in one step to avoid cache issues
RUN dotnet publish ShareBook/ShareBook.Api/ShareBook.Api.csproj -c Release -o /app/publish --verbosity normal

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Instala curl - necessário pro health check funcionar
RUN apt-get update && apt-get install -y curl

WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# força o ASP.NET Core a escutar na porta 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=30s \
  CMD curl -fsS http://127.0.0.1:8080/api/Operations/Ping || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "ShareBook.Api.dll"]