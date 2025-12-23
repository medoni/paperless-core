resource "google_cloud_run_v2_service" "service" {
  name     = var.service_name
  location = var.region
  project  = var.project_id

  template {
    scaling {
      min_instance_count = var.min_instances
      max_instance_count = var.max_instances
    }

    containers {
      image = var.image

      # Resource limits
      resources {
        limits = {
          memory = var.memory_limit
          cpu    = var.cpu_limit
        }
      }

      # Environment variables
      dynamic "env" {
        for_each = var.env_vars
        content {
          name  = env.key
          value = env.value
        }
      }

      # Startup probe
      startup_probe {
        http_get {
          path = var.health_check_path
        }
        initial_delay_seconds = 0
        timeout_seconds       = 1
        period_seconds        = 3
        failure_threshold     = 10
      }

      # Liveness probe
      liveness_probe {
        http_get {
          path = var.health_check_path
        }
        initial_delay_seconds = 0
        timeout_seconds       = 1
        period_seconds        = 10
        failure_threshold     = 3
      }
    }

    # Timeout
    timeout = "${var.timeout}s"

    # Service account
    service_account = var.service_account_email

    # Labels
    labels = {
      environment = var.environment
      managed_by  = "opentofu"
    }
  }

  # Traffic routing
  traffic {
    type    = "TRAFFIC_TARGET_ALLOCATION_TYPE_LATEST"
    percent = 100
  }

  lifecycle {
    ignore_changes = [
      template[0].containers[0].image, # Allow CI/CD to update image
    ]
  }
}

# IAM policy for public access (if enabled)
resource "google_cloud_run_v2_service_iam_member" "public_access" {
  count = var.allow_unauthenticated ? 1 : 0

  project  = var.project_id
  location = var.region
  name     = google_cloud_run_v2_service.service.name
  role     = "roles/run.invoker"
  member   = "allUsers"
}
