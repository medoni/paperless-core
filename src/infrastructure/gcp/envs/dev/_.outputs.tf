output "project_id" {
  description = "GCP Project ID"
  value       = var.project_id
}

output "region" {
  description = "GCP Region"
  value       = var.region
}

output "document_storage_bucket" {
  description = "Document storage bucket name"
  value       = module.document_storage.bucket_name
}

output "document_storage_bucket_url" {
  description = "Document storage bucket URL"
  value       = module.document_storage.bucket_url
}

# Commented out until modules are deployed
# output "api_service_url" {
#   description = "Cloud Run API service URL"
#   value       = module.api_service.service_url
# }

# output "classifier_function_name" {
#   description = "Document classifier function name"
#   value       = module.classifier_function.function_name
# }

output "app_service_account_email" {
  description = "Application service account email"
  value       = google_service_account.app_service_account.email
}
