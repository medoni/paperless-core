# PLC.Api - Backend API

Minimal ASP.NET Core Web API for PaperlessCore development environment.

## Quick Start

### Run Locally

```bash
cd src/backend/PLC.Api
dotnet run
```

API runs on:
- HTTP: http://localhost:8080
- HTTPS: https://localhost:8443
- Swagger UI: http://localhost:8080/swagger

### Restore Dependencies

```bash
dotnet restore
```

### Build

```bash
dotnet build
```

### Publish

```bash
dotnet publish -c Release -o ./publish
```

## Endpoints

### Health Checks

- `GET /health` - Basic health check
- `GET /health/ready` - Readiness probe (for K8s)
- `GET /health/live` - Liveness probe (for K8s)

### Version

- `GET /api/version` - API version information

### Swagger

- `GET /swagger` - API documentation (dev only)

## Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Environment (Development, Production)
- `ASPNETCORE_URLS` - Listening URLs

## Docker

### Build Image

```bash
docker build -t plc-api:dev -f src/backend/PLC.Api/Dockerfile src/backend
```

### Run Container

```bash
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development plc-api:dev
```

## Deployment

### Cloud Run

```bash
# Build and push
gcloud builds submit --tag gcr.io/PROJECT_ID/plc-api src/backend

# Deploy
gcloud run deploy plc-api \
  --image gcr.io/PROJECT_ID/plc-api \
  --platform managed \
  --region europe-west3 \
  --allow-unauthenticated
```

## Project Structure

```
PLC.Api/
├── Controllers/
│   ├── HealthController.cs
│   └── VersionController.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── PLC.Api.csproj
└── Dockerfile
```
