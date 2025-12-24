# Pub/Sub Topic for Document Change Events
# All workers subscribe to this topic to process documents idempotently

resource "google_pubsub_topic" "document_changed" {
  project = var.project_id
  name    = "document-changed"

  labels = {
    environment = var.environment
    managed-by  = "terraform"
  }

  message_retention_duration = "86400s" # 24 hours
}

# Subscription for Document Classifier Worker
resource "google_pubsub_subscription" "document_classifier" {
  project = var.project_id
  name    = "document-changed-classifier-sub"
  topic   = google_pubsub_topic.document_changed.name

  # Acknowledge deadline: how long worker has to process message
  ack_deadline_seconds = 60

  # Retry policy
  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }

  # Message retention
  message_retention_duration = "86400s" # 24 hours

  # Dead letter policy (future: handle failed messages)
  # dead_letter_policy {
  #   dead_letter_topic     = google_pubsub_topic.document_changed_dlq.id
  #   max_delivery_attempts = 5
  # }

  labels = {
    environment = var.environment
    worker      = "classifier"
    managed-by  = "terraform"
  }
}

# Subscription for OCR Worker (TODO: will be created later)
# Uncomment when OCR worker is implemented
# resource "google_pubsub_subscription" "document_ocr" {
#   project = var.project_id
#   name    = "document-changed-ocr-sub"
#   topic   = google_pubsub_topic.document_changed.name
#
#   ack_deadline_seconds = 300 # OCR takes longer
#
#   retry_policy {
#     minimum_backoff = "10s"
#     maximum_backoff = "600s"
#   }
#
#   message_retention_duration = "86400s"
#
#   labels = {
#     environment = var.environment
#     worker      = "ocr"
#     managed-by  = "terraform"
#   }
# }

# Subscription for Data Extraction Worker (TODO: will be created later)
# Uncomment when extraction worker is implemented
# resource "google_pubsub_subscription" "document_extraction" {
#   project = var.project_id
#   name    = "document-changed-extraction-sub"
#   topic   = google_pubsub_topic.document_changed.name
#
#   ack_deadline_seconds = 60
#
#   retry_policy {
#     minimum_backoff = "10s"
#     maximum_backoff = "600s"
#   }
#
#   message_retention_duration = "86400s"
#
#   labels = {
#     environment = var.environment
#     worker      = "extraction"
#     managed-by  = "terraform"
#   }
# }
