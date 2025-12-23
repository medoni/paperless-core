resource "google_storage_bucket" "bucket" {
  name     = var.bucket_name
  location = var.region
  project  = var.project_id

  # Storage class
  storage_class = var.storage_class

  # Uniform bucket-level access (recommended)
  uniform_bucket_level_access = true

  # Versioning
  versioning {
    enabled = var.versioning
  }

  # Lifecycle rules
  dynamic "lifecycle_rule" {
    for_each = var.lifecycle_rules
    content {
      action {
        type          = lifecycle_rule.value.action.type
        storage_class = lookup(lifecycle_rule.value.action, "storage_class", null)
      }
      condition {
        age                   = lookup(lifecycle_rule.value.condition, "age", null)
        created_before        = lookup(lifecycle_rule.value.condition, "created_before", null)
        with_state            = lookup(lifecycle_rule.value.condition, "with_state", null)
        matches_storage_class = lookup(lifecycle_rule.value.condition, "matches_storage_class", null)
        num_newer_versions    = lookup(lifecycle_rule.value.condition, "num_newer_versions", null)
      }
    }
  }

  # Public access prevention
  public_access_prevention = "enforced"

  # Labels
  labels = {
    environment = var.environment
    managed_by  = "opentofu"
    purpose     = "document-storage"
  }

  # Force destroy (only for dev)
  force_destroy = var.environment == "dev" ? true : false
}

# CORS configuration (for web uploads)
resource "google_storage_bucket" "bucket_with_cors" {
  count = var.enable_cors ? 1 : 0

  name     = var.bucket_name
  location = var.region
  project  = var.project_id

  cors {
    origin          = var.cors_origins
    method          = ["GET", "HEAD", "PUT", "POST", "DELETE"]
    response_header = ["*"]
    max_age_seconds = 3600
  }
}
