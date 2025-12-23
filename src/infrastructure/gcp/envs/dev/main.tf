

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

# Cloud Run service for API
# Commented out until container image is built
# Uncomment after: gcloud builds submit --tag gcr.io/PROJECT_ID/plc-api
# module "api_service" {
#   source = "../../modules/cloudrun"
#
#   project_id   = var.project_id
#   region       = var.region
#   environment  = var.environment
#   service_name = "plc-api"
#   image        = "gcr.io/${var.project_id}/plc-api:latest"
#
#   env_vars = {
#     ENVIRONMENT       = var.environment
#     PROJECT_ID        = var.project_id
#     STORAGE_BUCKET    = module.document_storage.bucket_name
#     FIRESTORE_PROJECT = var.project_id
#   }
#
#   min_instances = 0
#   max_instances = 10
#
#   memory_limit = "512Mi"
#   cpu_limit    = "1000m"
#   timeout      = 60
#
#   allow_unauthenticated = var.environment == "dev" ? true : false
# }

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
