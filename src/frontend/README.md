# PLC.Web - Frontend

Minimal Svelte frontend for PaperlessCore development environment.

## Quick Start

### Install Dependencies

```bash
npm install
```

### Development Server

```bash
npm run dev
```

Open http://localhost:5173

### Build for Production

```bash
npm run build
```

Output: `dist/`

### Preview Production Build

```bash
npm run preview
```

## Environment Configuration

Copy `.env.example` to `.env` and configure:

```bash
cp .env.example .env
```

Variables:
- `VITE_API_URL`: Backend API endpoint (default: http://localhost:8080)

## Features

- Hello Universe landing page
- API health check
- System status display
- Auto-refresh capability

## Tech Stack

- Svelte 4
- TypeScript
- Vite 5

## Docker Build

```bash
# Build image
docker build -t plc-web:dev .

# Run container
docker run -p 8080:80 plc-web:dev
```

## Deployment

```bash
# Build and push
gcloud builds submit --tag gcr.io/PROJECT_ID/plc-web

# Deploy
gcloud run deploy plc-web \
  --image gcr.io/PROJECT_ID/plc-web \
  --platform managed \
  --region europe-west3 \
  --allow-unauthenticated
```
