# PaperlessCore - GCP Infrastructure

OpenTofu configuration for Google Cloud Platform infrastructure.

## Prerequisites

1. GCP Project created (see [GCP Setup Guide](../../../docs/gcp-setup-guide.md))
2. OpenTofu installed
3. GCP credentials configured

## Directory Structure

```
gcp/
├── envs/
│   └── dev/              # Development environment
│       ├── main.tf       # Main configuration
│       ├── variables.tf  # Variable definitions
│       ├── outputs.tf    # Output definitions
│       └── bootstrap.sh  # Bootstrap script
└── modules/
    ├── storage/          # Cloud Storage module
    ├── cloudrun/         # Cloud Run module
    └── function/         # Cloud Functions module
```

## Quick Start

### 1. Run Bootstrap

```bash
cd src/infrastructure/gcp/envs/dev
./bootstrap.sh
```

### 2. Initialize OpenTofu

```bash
cd src/infrastructure/gcp/envs/dev
source .env
tofu init
```

### 3. Deploy Infrastructure

```bash
tofu plan
tofu apply
```

## Modules

### Storage Module

Cloud Storage buckets with versioning, lifecycle rules, and access controls.

### Cloud Run Module

Serverless containers with auto-scaling and health checks.

### Cloud Functions Module

Event-driven functions with automatic source code packaging.

## State Management

Terraform state stored in Google Cloud Storage:
- Bucket: `paperless-core-dev-terraform-state`
- Versioning enabled
- Encrypted at rest

## Important Notes

First deployment will fail for Cloud Run/Functions because container images don't exist yet. Deploy base infrastructure first, then build and push images before deploying services.

## Troubleshooting

### "Backend configuration changed"

```bash
tofu init -reconfigure
```

### "Permission denied"

```bash
gcloud projects get-iam-policy $PROJECT_ID
```

## References

- [GCP Setup Guide](../../../docs/gcp-setup-guide.md)
- [ADR-004: OpenTofu vs Terraform](../../../docs/adr/adr-004-opentofu-vs-terraform.md)
