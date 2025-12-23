#!/bin/bash
set -e

echo "=========================================="
echo "PaperlessCore - Development Bootstrap"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Configuration
PROJECT_ID="${PROJECT_ID:-paperless-core-dev}"
REGION="${REGION:-europe-west3}"
ZONE="${ZONE:-europe-west3-a}"

echo -e "${GREEN}[1/6] Checking prerequisites...${NC}"

# Check gcloud
if ! command -v gcloud &> /dev/null; then
    echo -e "${RED}Error: gcloud CLI not found. Please install: https://cloud.google.com/sdk/docs/install${NC}"
    exit 1
fi

# Check OpenTofu
if ! command -v tofu &> /dev/null; then
    echo -e "${YELLOW}Warning: OpenTofu not found. Install: https://opentofu.org/docs/intro/install/${NC}"
    echo "You can continue, but infrastructure deployment will fail."
fi

# Check Node.js
if ! command -v node &> /dev/null; then
    echo -e "${YELLOW}Warning: Node.js not found. Frontend development will not work.${NC}"
fi

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "${YELLOW}Warning: .NET SDK not found. Backend development will not work.${NC}"
fi

echo -e "${GREEN}✓ Prerequisites checked${NC}"
echo ""

echo -e "${GREEN}[2/6] Configuring GCP project...${NC}"

# Set project
gcloud config set project $PROJECT_ID
gcloud config set compute/region $REGION
gcloud config set compute/zone $ZONE

echo -e "${GREEN}✓ GCP project configured${NC}"
echo ""

echo -e "${GREEN}[3/6] Enabling required APIs...${NC}"

# Enable APIs (only essential ones for bootstrap)
gcloud services enable \
    cloudresourcemanager.googleapis.com \
    storage.googleapis.com \
    cloudbuild.googleapis.com \
    run.googleapis.com \
    cloudfunctions.googleapis.com \
    firestore.googleapis.com \
    --quiet

echo -e "${GREEN}✓ APIs enabled${NC}"
echo ""

echo -e "${GREEN}[4/6] Creating Terraform state bucket...${NC}"

STATE_BUCKET="${PROJECT_ID}-terraform-state"
USER_EMAIL=$(gcloud config get-value account 2>/dev/null)

# Create bucket if it doesn't exist
if ! gsutil ls gs://${STATE_BUCKET} &> /dev/null; then
    gsutil mb -p $PROJECT_ID -c STANDARD -l $REGION gs://${STATE_BUCKET}
    gsutil versioning set on gs://${STATE_BUCKET}
    gsutil uniformbucketlevelaccess set on gs://${STATE_BUCKET}
    echo -e "${GREEN}✓ State bucket created${NC}"
else
    echo -e "${YELLOW}✓ State bucket already exists${NC}"
fi

# Grant user access to state bucket (CRITICAL for OpenTofu!)
echo "Granting storage access to ${USER_EMAIL}..."
if gcloud storage buckets add-iam-policy-binding gs://${STATE_BUCKET} \
    --member="user:${USER_EMAIL}" \
    --role="roles/storage.objectAdmin" &> /dev/null; then
    echo -e "${GREEN}✓ Storage access granted${NC}"
else
    echo -e "${YELLOW}⚠ Could not grant storage access (may already exist)${NC}"
fi

echo ""

echo -e "${GREEN}[5/6] Setting up local environment...${NC}"

# Get project root (5 levels up from this script)
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../../.." && pwd)"

# Create .env file for frontend
if [ ! -f "$PROJECT_ROOT/src/frontend/.env" ]; then
    cp "$PROJECT_ROOT/src/frontend/.env.example" "$PROJECT_ROOT/src/frontend/.env"
    echo -e "${GREEN}✓ Frontend .env created${NC}"
else
    echo -e "${YELLOW}✓ Frontend .env already exists${NC}"
fi

# Create infrastructure .env file
mkdir -p "$PROJECT_ROOT/src/infrastructure/gcp/envs/dev"
if [ ! -f "$PROJECT_ROOT/src/infrastructure/gcp/envs/dev/.env" ]; then
    cat > "$PROJECT_ROOT/src/infrastructure/gcp/envs/dev/.env" <<EOF
# GCP Configuration
export PROJECT_ID="$PROJECT_ID"
export REGION="$REGION"
export ZONE="$ZONE"
export STATE_BUCKET="$STATE_BUCKET"
export GOOGLE_APPLICATION_CREDENTIALS=~/gcp-keys/${PROJECT_ID}-opentofu-admin.json

# Application Configuration
export ENVIRONMENT="dev"
export APP_NAME="paperless-core"
EOF
    echo -e "${GREEN}✓ Infrastructure .env created${NC}"
else
    echo -e "${YELLOW}✓ Infrastructure .env already exists${NC}"
fi

echo ""

echo -e "${GREEN}[6/6] Installing dependencies...${NC}"

# Frontend dependencies
if command -v node &> /dev/null; then
    echo "Installing frontend dependencies..."
    cd "$PROJECT_ROOT/src/frontend"
    npm install
    echo -e "${GREEN}✓ Frontend dependencies installed${NC}"
fi

# Backend dependencies
if command -v dotnet &> /dev/null; then
    echo "Restoring backend dependencies..."
    cd "$PROJECT_ROOT/src/backend/PLC.Api"
    dotnet restore
    echo -e "${GREEN}✓ Backend dependencies restored${NC}"
fi

echo ""
echo "=========================================="
echo -e "${GREEN}Bootstrap Complete!${NC}"
echo "=========================================="
echo ""
echo "Next steps:"
echo ""
echo "1. Follow GCP setup guide to create service account:"
echo "   docs/gcp-setup-guide.md"
echo ""
echo "2. Initialize OpenTofu:"
echo "   cd src/infrastructure/gcp/envs/dev && tofu init"
echo ""
echo "3. Start development:"
echo "   - Frontend: cd src/frontend && npm run dev"
echo "   - Backend:  cd src/backend/PLC.Api && dotnet run"
echo ""
