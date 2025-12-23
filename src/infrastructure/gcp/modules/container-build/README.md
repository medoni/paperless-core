# Container Build Module

OpenTofu module for building and pushing Docker containers to Google Artifact Registry using Cloud Build.

## Features

- Creates Artifact Registry repository (optional)
- Builds Docker image using Cloud Build with `cloudbuild.yaml`
- Pushes image to Artifact Registry
- Supports custom build arguments via substitutions
- Trigger rebuilds based on source changes

## Requirements

- A `cloudbuild.yaml` file must exist in the `source_path` directory
- The cloudbuild.yaml must accept `_IMAGE_URI` substitution
- Optional: Custom build args are passed as `_VAR_NAME` substitutions

## Usage

```hcl
module "app_build" {
  source = "../../modules/container-build"

  project_id        = "my-project"
  region            = "europe-west1"
  environment       = "dev"
  repository_name   = "my-app-repo"
  image_name        = "my-app"
  image_tag         = "v1.0.0"
  source_path       = "${path.module}/../../../app"
  dockerfile_path   = "Dockerfile"

  image_uri = "europe-west1-docker.pkg.dev/my-project/my-app-repo/my-app:v1.0.0"

  build_args = {
    BUILD_ENV = "production"
  }
}
```

## Prerequisites

- Google Cloud Build API enabled
- Artifact Registry API enabled
- Appropriate IAM permissions for Cloud Build service account
- gcloud CLI configured

## Required APIs

```bash
gcloud services enable cloudbuild.googleapis.com
gcloud services enable artifactregistry.googleapis.com
```

## Variables

See [_.variables.tf](_.variables.tf) for all available variables.

## Outputs

- `image_uri` - Full URI of the built image
- `repository_id` - ID of the Artifact Registry repository
- `repository_name` - Name of the repository
