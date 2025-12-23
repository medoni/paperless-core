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

variable "service_name" {
  description = "Name of the Cloud Run service"
  type        = string
}

variable "image" {
  description = "Container image to deploy"
  type        = string
}

variable "env_vars" {
  description = "Environment variables for the service"
  type        = map(string)
  default     = {}
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

variable "memory_limit" {
  description = "Memory limit (e.g., 512Mi, 1Gi)"
  type        = string
  default     = "512Mi"
}

variable "cpu_limit" {
  description = "CPU limit (e.g., 1000m = 1 CPU)"
  type        = string
  default     = "1000m"
}

variable "timeout" {
  description = "Request timeout in seconds"
  type        = number
  default     = 60
}

variable "health_check_path" {
  description = "Path for health check"
  type        = string
  default     = "/health"
}

variable "service_account_email" {
  description = "Service account email for the service"
  type        = string
  default     = null
}

variable "allow_unauthenticated" {
  description = "Allow unauthenticated access"
  type        = bool
  default     = false
}
