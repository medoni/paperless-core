output "image_uri" {
  description = "Full URI of the built container image"
  value       = var.image_uri
}

output "repository_id" {
  description = "ID of the Artifact Registry repository"
  value       = var.create_artifact_repo ? google_artifact_registry_repository.repo[0].id : null
}

output "repository_name" {
  description = "Name of the Artifact Registry repository"
  value       = var.repository_name
}
