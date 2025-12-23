# Service account for application
resource "google_service_account" "app_service_account" {
  account_id   = "plc-app-${var.environment}"
  display_name = "PaperlessCore Application (${var.environment})"
  description  = "Service account for PaperlessCore application runtime"
}

# IAM bindings
resource "google_project_iam_member" "app_firestore_user" {
  project = var.project_id
  role    = "roles/datastore.user"
  member  = "serviceAccount:${google_service_account.app_service_account.email}"
}

resource "google_project_iam_member" "app_storage_object_admin" {
  project = var.project_id
  role    = "roles/storage.objectAdmin"
  member  = "serviceAccount:${google_service_account.app_service_account.email}"
}

resource "google_project_iam_member" "app_documentai_user" {
  project = var.project_id
  role    = "roles/documentai.apiUser"
  member  = "serviceAccount:${google_service_account.app_service_account.email}"
}