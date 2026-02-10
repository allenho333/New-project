terraform {
  required_version = ">= 1.7.0"
}

variable "project_name" {
  type = string
}

output "vpc_id" {
  value       = "todo-vpc-id"
  description = "Replace when VPC resources are implemented."
}
