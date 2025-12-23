variable "project_id" {
  description = "GCP Project ID"
  type        = string
}

variable "region" {
  description = "GCP Region"
  type        = string
}

variable "environment" {
  description = "Environment (dev, staging, prod)"
  type        = string
}

variable "function_name" {
  description = "Name of the Cloud Function"
  type        = string
}

variable "runtime" {
  description = "Runtime for the function (e.g., nodejs20, python311)"
  type        = string
  default     = "nodejs20"
}

variable "entry_point" {
  description = "Entry point function name"
  type        = string
}

variable "source_dir" {
  description = "Directory containing function source code"
  type        = string
}

variable "env_vars" {
  description = "Environment variables for the function"
  type        = map(string)
  default     = {}
}

variable "memory" {
  description = "Memory allocation in MB"
  type        = number
  default     = 256
}

variable "timeout" {
  description = "Timeout in seconds"
  type        = number
  default     = 60
}

variable "min_instances" {
  description = "Minimum number of instances"
  type        = number
  default     = 0
}

variable "max_instances" {
  description = "Maximum number of instances"
  type        = number
  default     = 10
}

variable "event_trigger" {
  description = "Event trigger configuration"
  type = object({
    event_type = string
    resource   = string
  })
  default = null
}

variable "retry_on_failure" {
  description = "Retry on failure"
  type        = bool
  default     = true
}

variable "service_account_email" {
  description = "Service account email for the function"
  type        = string
  default     = null
}
