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

output "api_service_url" {
  description = "Cloud Run API service URL"
  value       = module.api_service.service_url
}

output "api_image_uri" {
  description = "PLC API container image URI"
  value       = module.plc_api_build.image_uri
}

output "frontend_service_url" {
  description = "Cloud Run Frontend service URL"
  value       = module.frontend_service.service_url
}

output "frontend_image_uri" {
  description = "PLC Frontend container image URI"
  value       = module.plc_frontend_build.image_uri
}

# output "classifier_function_name" {
#   description = "Document classifier function name"
#   value       = module.classifier_function.function_name
# }

output "app_service_account_email" {
  description = "Application service account email"
  value       = google_service_account.app_service_account.email
}

# Generate deployment info file
resource "local_file" "deployment_info" {
  filename = "${path.module}/deployment-info.txt"
  content  = <<-EOT
    PaperlessCore - Deployment Info
    Environment: ${var.environment}
    Generated: ${timestamp()}

    ================================================================================
    DEPLOYED SERVICES
    ================================================================================

    PLC Frontend (Cloud Run)
    URL:    ${module.frontend_service.service_url}
    Status: Public (unauthenticated access enabled)
    Image:  ${module.plc_frontend_build.image_uri}

    PLC API (Cloud Run)
    URL:    ${module.api_service.service_url}
    Status: Public (unauthenticated access enabled)
    Image:  ${module.plc_api_build.image_uri}

    Endpoints:
      - Health:  ${module.api_service.service_url}/health
      - Version: ${module.api_service.service_url}/api/version

    ================================================================================
    INFRASTRUCTURE
    ================================================================================

    Project:        ${var.project_id}
    Region:         ${var.region}

    Storage Bucket: ${module.document_storage.bucket_name}
    Bucket URL:     ${module.document_storage.bucket_url}

    Service Account: ${google_service_account.app_service_account.email}

    ================================================================================
  EOT
}
