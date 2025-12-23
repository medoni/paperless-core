variable "project_id" {
  description = "GCP Project ID"
  type        = string
}

variable "region" {
  description = "GCP Region for Artifact Registry and Cloud Build"
  type        = string
}

variable "environment" {
  description = "Environment (dev, staging, prod)"
  type        = string
}

variable "repository_name" {
  description = "Name of the Artifact Registry repository"
  type        = string
}

variable "image_name" {
  description = "Name of the container image"
  type        = string
}

variable "image_tag" {
  description = "Tag for the container image"
  type        = string
  default     = "latest"
}

variable "source_path" {
  description = "Path to the source code directory (where Dockerfile is located)"
  type        = string
}

variable "dockerfile_path" {
  description = "Relative path to Dockerfile (optional, defaults to Dockerfile in source_path)"
  type        = string
  default     = null
}

variable "build_args" {
  description = "Build arguments to pass to Docker build"
  type        = map(string)
  default     = {}
}

variable "source_hash" {
  description = "Hash of source files to trigger rebuild (optional)"
  type        = string
  default     = null
}

variable "create_artifact_repo" {
  description = "Whether to create the Artifact Registry repository"
  type        = bool
  default     = true
}

variable "image_uri" {
  description = "Full URI of the image (region-docker.pkg.dev/project/repo/image:tag)"
  type        = string
}
