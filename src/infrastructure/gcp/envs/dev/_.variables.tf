variable "project_id" {
  description = "GCP Project ID"
  type        = string
  default     = "paperless-core-dev"
}

variable "region" {
  description = "GCP Region for resources"
  type        = string
  default     = "europe-west1" # Belgium
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "document_lifecycle_rules" {
  description = "Lifecycle rules for document storage bucket"
  type = list(object({
    action = object({
      type          = string
      storage_class = optional(string)
    })
    condition = object({
      age                   = optional(number)
      created_before        = optional(string)
      with_state            = optional(string)
      matches_storage_class = optional(list(string))
      num_newer_versions    = optional(number)
    })
  }))
  default = [
    {
      action = {
        type          = "SetStorageClass"
        storage_class = "NEARLINE"
      }
      condition = {
        age        = 365 # Move to Nearline after 1 year
        with_state = "LIVE"
      }
    },
    {
      action = {
        type          = "SetStorageClass"
        storage_class = "COLDLINE"
      }
      condition = {
        age        = 1825 # Move to Coldline after 5 years
        with_state = "LIVE"
      }
    }
  ]
}
