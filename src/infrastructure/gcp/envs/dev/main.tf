

# Storage bucket for documents
module "document_storage" {
  source = "../../modules/storage"

  project_id      = var.project_id
  region          = var.region
  environment     = var.environment
  bucket_name     = "${var.project_id}-documents"
  versioning      = true
  lifecycle_rules = var.document_lifecycle_rules
}

# Firestore is created manually via gcloud
# See: gcp-setup-guide.md

# Container build for PLC API
module "plc_api_build" {
  source = "../../modules/container-build"

  project_id           = var.project_id
  region               = var.region
  environment          = var.environment
  repository_name      = "plc-api-repo"
  image_name           = "plc-api"
  image_tag            = "latest"
  source_path          = "${path.module}/../../../../backend/PLC.Api"
  create_artifact_repo = true

  image_uri = "${var.region}-docker.pkg.dev/${var.project_id}/plc-api-repo/plc-api:latest"
}

# Cloud Run service for API
module "api_service" {
  source = "../../modules/cloudrun"

  project_id   = var.project_id
  region       = var.region
  environment  = var.environment
  service_name = "plc-api"
  image        = module.plc_api_build.image_uri

  env_vars = {
    ASPNETCORE_ENVIRONMENT = var.environment == "dev" ? "Development" : "Production"
    PROJECT_ID             = var.project_id
    STORAGE_BUCKET         = module.document_storage.bucket_name
    FIRESTORE_PROJECT      = var.project_id
  }

  min_instances = 0
  max_instances = 10

  memory_limit = "512Mi"
  cpu_limit    = "1000m"
  timeout      = 60

  allow_unauthenticated = var.environment == "dev" ? true : false

  depends_on = [module.plc_api_build]
}

# Container build for PLC Frontend
module "plc_frontend_build" {
  source = "../../modules/container-build"

  project_id           = var.project_id
  region               = var.region
  environment          = var.environment
  repository_name      = "plc-frontend-repo"
  image_name           = "plc-frontend"
  image_tag            = "latest"
  source_path          = "${path.module}/../../../../frontend"
  create_artifact_repo = true

  image_uri = "${var.region}-docker.pkg.dev/${var.project_id}/plc-frontend-repo/plc-frontend:latest"

  build_args = {
    VITE_API_URL = module.api_service.service_url
  }

  depends_on = [module.api_service]
}

# Cloud Run service for Frontend
module "frontend_service" {
  source = "../../modules/cloudrun"

  project_id   = var.project_id
  region       = var.region
  environment  = var.environment
  service_name = "plc-frontend"
  image        = module.plc_frontend_build.image_uri

  env_vars = {}

  min_instances = 0
  max_instances = 10

  memory_limit = "512Mi"
  cpu_limit    = "1000m"
  timeout      = 60

  allow_unauthenticated = var.environment == "dev" ? true : false

  depends_on = [module.plc_frontend_build]
}

# Cloud Function for document classification
# Commented out until function code exists
# Uncomment after creating: src/functions/document-classifier
# module "classifier_function" {
#   source = "../../modules/function"
#
#   project_id    = var.project_id
#   region        = var.region
#   environment   = var.environment
#   function_name = "plc-document-classifier"
#   runtime       = "nodejs20"
#   entry_point   = "classifyDocument"
#   source_dir    = "${path.module}/../../../../functions/document-classifier"
#
#   env_vars = {
#     ENVIRONMENT    = var.environment
#     PROJECT_ID     = var.project_id
#     STORAGE_BUCKET = module.document_storage.bucket_name
#   }
#
#   event_trigger = {
#     event_type = "google.cloud.storage.object.v1.finalized"
#     resource   = module.document_storage.bucket_name
#   }
#
#   memory    = 256
#   timeout   = 60
#   max_instances = 10
# }
