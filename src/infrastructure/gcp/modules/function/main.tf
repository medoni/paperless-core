# Archive source code
data "archive_file" "function_source" {
  type        = "zip"
  source_dir  = var.source_dir
  output_path = "${path.module}/.tmp/${var.function_name}.zip"
}

# Storage bucket for function source
resource "google_storage_bucket" "function_bucket" {
  name     = "${var.project_id}-function-source"
  location = var.region
  project  = var.project_id

  uniform_bucket_level_access = true
  force_destroy               = var.environment == "dev" ? true : false

  labels = {
    environment = var.environment
    managed_by  = "opentofu"
  }
}

# Upload source code
resource "google_storage_bucket_object" "function_source" {
  name   = "${var.function_name}/${data.archive_file.function_source.output_md5}.zip"
  bucket = google_storage_bucket.function_bucket.name
  source = data.archive_file.function_source.output_path
}

# Cloud Function (2nd generation)
resource "google_cloudfunctions2_function" "function" {
  name     = var.function_name
  location = var.region
  project  = var.project_id

  build_config {
    runtime     = var.runtime
    entry_point = var.entry_point

    source {
      storage_source {
        bucket = google_storage_bucket.function_bucket.name
        object = google_storage_bucket_object.function_source.name
      }
    }
  }

  service_config {
    max_instance_count = var.max_instances
    min_instance_count = var.min_instances
    available_memory   = "${var.memory}M"
    timeout_seconds    = var.timeout

    environment_variables = var.env_vars

    service_account_email = var.service_account_email
  }

  # Event trigger (if configured)
  dynamic "event_trigger" {
    for_each = var.event_trigger != null ? [var.event_trigger] : []
    content {
      trigger_region = var.region
      event_type     = event_trigger.value.event_type

      event_filters {
        attribute = "bucket"
        value     = event_trigger.value.resource
      }

      retry_policy = var.retry_on_failure ? "RETRY_POLICY_RETRY" : "RETRY_POLICY_DO_NOT_RETRY"
    }
  }

  labels = {
    environment = var.environment
    managed_by  = "opentofu"
  }
}
