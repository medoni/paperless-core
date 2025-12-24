# Artifact Registry Repository
resource "google_artifact_registry_repository" "repo" {
  count = var.create_artifact_repo ? 1 : 0

  project       = var.project_id
  location      = var.region
  repository_id = var.repository_name
  description   = "Container repository for ${var.image_name}"
  format        = "DOCKER"

  labels = {
    environment = var.environment
    managed_by  = "opentofu"
  }
}

# Local exec to build and push image using Cloud Build
resource "null_resource" "build_and_push" {
  triggers = {
    # Rebuild when source changes
    source_hash = var.source_hash != null ? var.source_hash : timestamp()
    image_tag   = var.image_tag
  }

  provisioner "local-exec" {
    command = <<-EOT
      gcloud builds submit ${var.source_path} \
        --project=${var.project_id} \
        --region=${var.region} \
        --config=${var.cloudbuild_config_path != null ? var.cloudbuild_config_path : "${var.source_path}/cloudbuild.yaml"} \
        --substitutions=_IMAGE_URI=${var.image_uri}${length(var.build_args) > 0 ? ",${join(",", [for k, v in var.build_args : "_${k}=${v}"])}" : ""}
    EOT
  }

  depends_on = [google_artifact_registry_repository.repo]
}
