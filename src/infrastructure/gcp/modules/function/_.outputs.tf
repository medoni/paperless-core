output "function_name" {
  description = "Name of the Cloud Function"
  value       = google_cloudfunctions2_function.function.name
}

output "function_url" {
  description = "URL of the Cloud Function"
  value       = google_cloudfunctions2_function.function.service_config[0].uri
}

output "function_id" {
  description = "ID of the Cloud Function"
  value       = google_cloudfunctions2_function.function.id
}
