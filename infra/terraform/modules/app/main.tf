terraform {
  required_version = ">= 1.7.0"
}

variable "project_name" {
  type = string
}

variable "image_tag" {
  type = string
}

output "service_url" {
  value       = "todo-service-url"
  description = "Replace when compute service is implemented."
}
