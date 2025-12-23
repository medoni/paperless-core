# Getting Started - Developer Guide

Quick guide to set up PaperlessCore development environment.

## Prerequisites

### Required

- **gcloud CLI** - [Install Guide](https://cloud.google.com/sdk/docs/install)
- **OpenTofu** - [Install Guide](https://opentofu.org/docs/intro/install/)
- **Node.js LTS** (v20+) - [Download](https://nodejs.org/)
- **.NET SDK 8** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** - Version control

### Optional

- **Docker** - For containerized development
- **VS Code** - Recommended IDE with extensions:
  - C# Dev Kit
  - Svelte for VS Code
  - HashiCorp Terraform (works with OpenTofu)

## Step 1: Clone Repository

```bash
git clone https://github.com/medoni/paperless-core.git
cd paperless-core
```

## Step 2: Run Bootstrap Script

```bash
cd src/infrastructure/gcp/envs/dev
./bootstrap.sh
```

This script will:
- Check prerequisites
- Configure GCP project
- Enable required APIs
- Create Terraform state bucket
- Set up local environment files
- Install dependencies

## Step 3: GCP Service Account Setup

Follow the [GCP Setup Guide](./gcp-setup-guide.md) to:

1. Create service account for OpenTofu
2. Download service account key
3. Configure Firestore database
4. Set environment variables

## Step 4: Local Development

### Frontend (Svelte)

```bash
cd src/frontend
npm run dev
```

Open: http://localhost:5173

### Backend (C# API)

```bash
cd src/backend/PLC.Api
dotnet run
```

Open:
- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger

### Test Integration

1. Start backend (port 8080)
2. Start frontend (port 5173)
3. Frontend will automatically connect to API
4. Check "Hello Universe" page shows API status as "connected"

## Step 5: Deploy Infrastructure (Optional)

### Initialize OpenTofu

```bash
cd infrastructure/gcp/envs/dev
source .env
tofu init
```

### Plan Deployment

```bash
tofu plan
```

Review the planned changes.

### Deploy

```bash
tofu apply
```

**Note:** First deployment will fail for Cloud Run/Functions because container images don't exist yet. This is expected. Deploy these later after building images.

## Step 6: Build Container Images (Optional)

### Backend API

```bash
cd src/backend
gcloud builds submit --tag gcr.io/$PROJECT_ID/plc-api
```

### Frontend

```bash
cd src/frontend
gcloud builds submit --tag gcr.io/$PROJECT_ID/plc-web
```

## Project Structure

```
paperless-core/
├── docs/                       # Documentation
│   ├── arc42/                  # Architecture documentation
│   ├── adr/                    # Architecture decision records
│   ├── gcp-setup-guide.md      # Detailed GCP setup
│   └── getting-started.md      # This file
└── src/
    ├── backend/                # C# .NET backend
    │   └── PLC.Api/            # Web API
    ├── frontend/               # Svelte frontend
    ├── infrastructure/         # IaC configurations
    │   └── gcp/
    │       ├── envs/dev/       # Dev environment
    │       └── modules/        # Reusable modules
    ├── functions/              # Serverless functions (Node.js)
    └── config/                 # Configuration files
        └── document-structure.yaml
```

## Development Workflow

1. Work locally (frontend + backend)
2. Build Docker images
3. Push to Google Container Registry
4. Deploy to Cloud Run/Functions with OpenTofu
5. Test in cloud environment

## Common Tasks

### Install Frontend Dependencies

```bash
cd src/frontend
npm install
```

### Install Backend Dependencies

```bash
cd src/backend/PLC.Api
dotnet restore
```

### Build Backend

```bash
cd src/backend/PLC.Api
dotnet build
```

### Build Frontend

```bash
cd src/frontend
npm run build
```

### Run Tests

```bash
# Backend tests (when available)
cd src/backend
dotnet test

# Frontend tests (when available)
cd src/frontend
npm test
```

### View Logs (Cloud)

```bash
# Cloud Run logs
gcloud run services logs read plc-api --region=europe-west3

# Cloud Functions logs
gcloud functions logs read plc-document-classifier --region=europe-west3
```

## Troubleshooting

### "gcloud command not found"

Install gcloud CLI: https://cloud.google.com/sdk/docs/install

### "tofu command not found"

Install OpenTofu: https://opentofu.org/docs/intro/install/

### "Backend configuration changed"

```bash
cd infrastructure/gcp/envs/dev
tofu init -reconfigure
```

### "Permission denied" errors

Check service account has correct roles:
```bash
gcloud projects get-iam-policy $PROJECT_ID
```

### Frontend can't connect to API

1. Check backend is running on port 8080
2. Check `src/frontend/.env` has correct `VITE_API_URL`
3. Check CORS is enabled in development

### Docker build fails

Ensure you're in the correct directory:
```bash
# For backend
docker build -f src/backend/PLC.Api/Dockerfile src/backend

# For frontend
docker build -f src/frontend/Dockerfile src/frontend
```

## Environment Variables

### Frontend (.env)

```bash
VITE_API_URL=http://localhost:8080
```

### Backend (appsettings.Development.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Infrastructure (.env)

```bash
export PROJECT_ID="paperless-core-dev"
export REGION="europe-west3"
export GOOGLE_APPLICATION_CREDENTIALS=~/gcp-keys/paperless-core-dev-opentofu-admin.json
```

## References

- [GCP Setup Guide](./gcp-setup-guide.md)
- [Infrastructure README](../src/infrastructure/gcp/README.md)
- [Frontend README](../src/frontend/README.md)
- [Backend README](../src/backend/PLC.Api/README.md)
- [Arc42 Documentation](./arc42/)
- [ADRs](./adr/)
