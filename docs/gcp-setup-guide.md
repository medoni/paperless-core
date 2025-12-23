# GCP Project Setup Guide

Step-by-step guide to set up the PaperlessCore development environment on Google Cloud Platform.

## Prerequisites

- Google Cloud account with billing enabled
- `gcloud` CLI installed ([Installation Guide](https://cloud.google.com/sdk/docs/install))
- OpenTofu installed ([Installation Guide](https://opentofu.org/docs/intro/install/))

## 1. Initial Setup

### Authenticate with Google Cloud

```bash
gcloud auth login
gcloud auth application-default login
```

### Set Project Variables

```bash
export PROJECT_ID="paperless-core-dev"
export PROJECT_NAME="PaperlessCore Development"
export REGION="europe-west3"  # Frankfurt, Germany
export ZONE="europe-west3-a"
export BILLING_ACCOUNT_ID="YOUR_BILLING_ACCOUNT_ID"
```

Find your billing account ID:
```bash
gcloud billing accounts list
```

## 2. Create GCP Project

### Create Project

```bash
gcloud projects create $PROJECT_ID \
  --name="$PROJECT_NAME" \
  --set-as-default
```

### Link Billing Account

```bash
gcloud billing projects link $PROJECT_ID \
  --billing-account=$BILLING_ACCOUNT_ID
```

### Set Default Project

```bash
gcloud config set project $PROJECT_ID
gcloud config set compute/region $REGION
gcloud config set compute/zone $ZONE
```

## 3. Enable Required APIs

### Core APIs

```bash
# Enable essential APIs
gcloud services enable \
  cloudresourcemanager.googleapis.com \
  serviceusage.googleapis.com \
  iam.googleapis.com \
  compute.googleapis.com \
  storage.googleapis.com \
  cloudbuild.googleapis.com \
  cloudfunctions.googleapis.com \
  run.googleapis.com \
  firestore.googleapis.com \
  logging.googleapis.com \
  monitoring.googleapis.com
```

### Document Processing APIs

```bash
# OCR and document processing
gcloud services enable \
  documentai.googleapis.com \
  vision.googleapis.com
```

### API Gateway (Important: Check Regional Availability)

```bash
# API Gateway for unified API management
gcloud services enable \
  apigateway.googleapis.com \
  servicemanagement.googleapis.com \
  servicecontrol.googleapis.com
```

**Note:** API Gateway availability in `europe-west3` (Frankfurt):
- As of 2025, API Gateway has limited regional support
- Check current availability: https://cloud.google.com/api-gateway/docs/locations
- Alternative regions in Europe:
  - `europe-west1` (Belgium) - Full API Gateway support
  - `europe-west4` (Netherlands) - Full API Gateway support

**Decision Required:**
- If API Gateway not available in `europe-west3`, consider:
  - Option A: Use `europe-west1` (Belgium) instead
  - Option B: Deploy API Gateway in `europe-west1`, other services in `europe-west3`
  - Option C: Use Cloud Run with custom domain instead of API Gateway

### Storage and Database

```bash
# Cloud Storage for documents
gcloud services enable \
  storage-api.googleapis.com

# Firestore (Native mode)
# Note: This is done via console or separate command
```

## 4. Service Account Setup

### Create Terraform/OpenTofu Service Account

```bash
export SA_NAME="opentofu-admin"
export SA_EMAIL="${SA_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"

# Create service account
gcloud iam service-accounts create $SA_NAME \
  --display-name="OpenTofu Infrastructure Admin" \
  --description="Service account for OpenTofu infrastructure management"
```

### Grant Your User IAM Admin Rights (Required First!)

```bash
# Get your user email
export USER_EMAIL=$(gcloud config get-value account)

# Grant yourself Project IAM Admin (required to manage IAM policies)
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="user:${USER_EMAIL}" \
  --role="roles/resourcemanager.projectIamAdmin"

# Grant yourself Service Account Admin (required to create service accounts)
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="user:${USER_EMAIL}" \
  --role="roles/iam.serviceAccountAdmin"

# Grant yourself Owner role (provides full control)
# Note: In production, use more granular permissions
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="user:${USER_EMAIL}" \
  --role="roles/owner"
```

**Why needed:** Your user account needs permission to create and manage IAM policies. Without these roles, OpenTofu will fail with "Error 403: Policy update access denied."

### Grant Service Account Permissions

```bash
# Grant Editor role (broad permissions for infrastructure management)
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/editor"

# Grant Storage Admin (for Terraform state backend)
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/storage.admin"

# Grant Service Account Admin (to manage other service accounts)
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/iam.serviceAccountAdmin"

# Grant Security Admin (to manage IAM policies via OpenTofu)
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/iam.securityAdmin"
```

### Create and Download Key

```bash
# Create key file
gcloud iam service-accounts keys create ~/gcp-keys/${PROJECT_ID}-opentofu-admin.json \
  --iam-account=$SA_EMAIL

# Set environment variable
export GOOGLE_APPLICATION_CREDENTIALS=~/gcp-keys/${PROJECT_ID}-opentofu-admin.json
```

**Security Note:** Never commit service account keys to Git. Add to `.gitignore`.

## 5. Create Terraform State Backend

### Create GCS Bucket for State

```bash
export STATE_BUCKET="${PROJECT_ID}-terraform-state"
export USER_EMAIL=$(gcloud config get-value account)

# Create bucket in specified region
gsutil mb -p $PROJECT_ID -c STANDARD -l $REGION gs://${STATE_BUCKET}

# Enable versioning (protect against accidental deletion)
gsutil versioning set on gs://${STATE_BUCKET}

# Enable uniform bucket-level access
gsutil uniformbucketlevelaccess set on gs://${STATE_BUCKET}

# Grant your user access to the state bucket (CRITICAL!)
gcloud storage buckets add-iam-policy-binding gs://${STATE_BUCKET} \
  --member="user:${USER_EMAIL}" \
  --role="roles/storage.objectAdmin"

# Block public access
gsutil iam ch -d allUsers gs://${STATE_BUCKET} 2>/dev/null || true
gsutil iam ch -d allAuthenticatedUsers gs://${STATE_BUCKET} 2>/dev/null || true
```

**IMPORTANT:** Without the storage.objectAdmin role on the state bucket, OpenTofu will fail with "Access denied" when trying to read/write the state file, even if you have Owner role on the project!

## 6. Initialize Firestore

### Create Firestore Database (Native Mode)

```bash
# Create Firestore in Native mode
gcloud firestore databases create \
  --location=$REGION \
  --type=firestore-native
```

**Note:** Firestore location cannot be changed after creation. Choose carefully:
- `europe-west3` (Frankfurt) - Preferred for Germany
- `europe-west1` (Belgium) - Alternative

## 7. Verify Setup

### Check Project Configuration

```bash
# Display current project
gcloud config get-value project

# List enabled services
gcloud services list --enabled

# Verify service account
gcloud iam service-accounts list

# Check Firestore
gcloud firestore databases list
```

### Test API Gateway Availability (if using)

```bash
# Try to list API Gateway configs (should return empty, not error)
gcloud api-gateway apis list --project=$PROJECT_ID
```

If this returns an error about regional availability, API Gateway is not available in your chosen region.

## 8. Environment Configuration

### Save Configuration

Create a file `infrastructure/gcp/envs/dev/.env` (do NOT commit):

```bash
# GCP Configuration
export PROJECT_ID="paperless-core-dev"
export REGION="europe-west3"
export ZONE="europe-west3-a"
export STATE_BUCKET="paperless-core-dev-terraform-state"
export GOOGLE_APPLICATION_CREDENTIALS=~/gcp-keys/paperless-core-dev-opentofu-admin.json

# Application Configuration
export ENVIRONMENT="dev"
export APP_NAME="paperless-core"
```

Add to `.gitignore`:
```
infrastructure/gcp/envs/*/.env
*.json
!**/example.json
```

## 9. Regional Services Summary

### Available in europe-west3 (Frankfurt)
- Cloud Storage
- Cloud Functions (2nd gen)
- Cloud Run
- Firestore
- Document AI
- Vision API
- Cloud Build
- Compute Engine

### Check Availability for API Gateway
- Documentation: https://cloud.google.com/api-gateway/docs/locations
- If NOT available in Frankfurt:
  - Deploy API Gateway in `europe-west1` (Belgium)
  - Use Cloud Run in `europe-west3` for compute
  - Network latency between regions: ~5-10ms (acceptable)

## Next Steps

After completing this setup:

1. Initialize OpenTofu configuration ([infrastructure/gcp/README.md](../infrastructure/gcp/README.md))
2. Deploy base infrastructure (Storage, Functions, Firestore)
3. Set up CI/CD pipeline
4. Deploy first services

## Troubleshooting

### "API not enabled" errors
```bash
# Check which APIs are enabled
gcloud services list --enabled

# Enable missing API
gcloud services enable <api-name>.googleapis.com
```

### "Error 403: Policy update access denied" when running OpenTofu
This error typically indicates missing permissions. Check:

```bash
# 1. Verify your user has necessary IAM roles
export USER_EMAIL=$(gcloud config get-value account)
gcloud projects get-iam-policy $PROJECT_ID \
  --flatten="bindings[].members" \
  --filter="bindings.members:user:${USER_EMAIL}"

# 2. Verify access to Terraform state bucket
gcloud storage buckets get-iam-policy gs://${PROJECT_ID}-terraform-state \
  --flatten="bindings[].members" \
  --filter="bindings.members:user:${USER_EMAIL}"

# 3. If missing, add storage permissions
gcloud storage buckets add-iam-policy-binding gs://${PROJECT_ID}-terraform-state \
  --member="user:${USER_EMAIL}" \
  --role="roles/storage.objectAdmin"
```

### "Insufficient permissions" errors
```bash
# Check service account permissions
gcloud projects get-iam-policy $PROJECT_ID \
  --flatten="bindings[].members" \
  --filter="bindings.members:serviceAccount:${SA_EMAIL}"
```

### Billing issues
```bash
# Verify billing is linked
gcloud billing projects describe $PROJECT_ID
```

### OpenTofu can't read/write state file
```bash
# This is the most common issue! Grant storage access:
export USER_EMAIL=$(gcloud config get-value account)
gcloud storage buckets add-iam-policy-binding gs://${PROJECT_ID}-terraform-state \
  --member="user:${USER_EMAIL}" \
  --role="roles/storage.objectAdmin"
```

## Security Checklist

- [ ] Service account key stored securely (NOT in Git)
- [ ] State bucket has versioning enabled
- [ ] State bucket blocks public access
- [ ] Firestore security rules configured (separate step)
- [ ] API keys rotated regularly
- [ ] IAM permissions follow least privilege principle

## Cost Estimation

Expected monthly costs for DEV environment:
- Cloud Storage (state + documents): ~$0.50
- Firestore (low usage): ~$1.00
- Cloud Functions (light usage): ~$2.00
- Cloud Run (minimal): ~$1.00
- Document AI (development): ~$1.00
- Networking: ~$0.50

**Total: ~$6/month** (well under 10 EUR budget)

## References

- GCP Regions: https://cloud.google.com/about/locations
- API Gateway Locations: https://cloud.google.com/api-gateway/docs/locations
- Service Account Best Practices: https://cloud.google.com/iam/docs/best-practices-service-accounts
- Terraform State in GCS: https://developer.hashicorp.com/terraform/language/settings/backends/gcs
